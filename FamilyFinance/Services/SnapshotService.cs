using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamilyFinance.Services;

public class SnapshotService : ISnapshotService
{
    private readonly AppDbContext _db;
    private readonly ILogger<SnapshotService> _logger;
    private readonly IValidator<Snapshot> _snapshotValidator;
    private readonly IValidator<InvestmentAsset> _investmentValidator;
    private readonly IValidator<Receivable> _receivableValidator;

    public SnapshotService(AppDbContext db, ILogger<SnapshotService> logger, 
        IValidator<Snapshot> snapshotValidator,
        IValidator<InvestmentAsset> investmentValidator,
        IValidator<Receivable> receivableValidator)
    {
        _db = db;
        _logger = logger;
        _snapshotValidator = snapshotValidator;
        _investmentValidator = investmentValidator;
        _receivableValidator = receivableValidator;
    }

    public async Task<List<Snapshot>> GetAllAsync(int familyId)
    {
        _logger.LogDebug("Fetching all snapshots for family {FamilyId}", familyId);
        return await _db.Snapshots
            .Where(s => s.FamilyId == familyId && !s.IsDeleted)
            .OrderByDescending(s => s.SnapshotDate)
            .ToListAsync();
    }

    public async Task<Snapshot?> GetByIdAsync(int id)
    {
        _logger.LogDebug("Fetching snapshot {SnapshotId}", id);
        return await _db.Snapshots
            .Include(s => s.Lines).ThenInclude(l => l.Account)
            .Include(s => s.Investments).ThenInclude(i => i.Portfolio)
            .Include(s => s.Receivables)
            .Include(s => s.Expenses).ThenInclude(e => e.Category)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<Snapshot?> GetLatestAsync(int familyId)
    {
        _logger.LogDebug("Fetching latest snapshot for family {FamilyId}", familyId);
        return await _db.Snapshots
            .Include(s => s.Lines).ThenInclude(l => l.Account)
            .Include(s => s.Investments).ThenInclude(i => i.Portfolio)
            .Include(s => s.Receivables)
            .Include(s => s.Expenses).ThenInclude(e => e.Category)
            .Where(s => s.FamilyId == familyId && !s.IsDeleted)
            .OrderByDescending(s => s.SnapshotDate)
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResult<int>> SaveAsync(int familyId, int? snapshotId, DateOnly date,
        List<(int AccountId, decimal Amount, decimal ContributionBasis)> accountAmounts,
        List<(string Name, decimal CostBasis, decimal Value, int? PortfolioId)> investments,
        List<(string Description, decimal Amount, ReceivableStatus Status, DateOnly? ExpectedDate)> receivables,
        string? userId = null)
    {
        // Validate snapshot date
        var snapshotForValidation = new Snapshot { Id = snapshotId ?? 0, FamilyId = familyId, SnapshotDate = date };
        var validationResult = await _snapshotValidator.ValidateAsync(snapshotForValidation);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Snapshot validation failed: {Errors}", string.Join(", ", errors));
            return ServiceResult<int>.Fail(errors);
        }

        // Validate investments
        foreach (var inv in investments.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
        {
            var asset = new InvestmentAsset { Name = inv.Name, CostBasis = inv.CostBasis, Value = inv.Value };
            var invValidation = await _investmentValidator.ValidateAsync(asset);
            if (!invValidation.IsValid)
            {
                var errors = invValidation.Errors.Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Investment validation failed for '{AssetName}': {Errors}", inv.Name, string.Join(", ", errors));
                return ServiceResult<int>.Fail(errors);
            }
        }

        // Validate receivables
        foreach (var rec in receivables.Where(r => !string.IsNullOrWhiteSpace(r.Description)))
        {
            var receivable = new Receivable { Description = rec.Description, Amount = rec.Amount };
            var recValidation = await _receivableValidator.ValidateAsync(receivable);
            if (!recValidation.IsValid)
            {
                var errors = recValidation.Errors.Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Receivable validation failed for '{Description}': {Errors}", rec.Description, string.Join(", ", errors));
                return ServiceResult<int>.Fail(errors);
            }
        }

        Snapshot snapshot;
        if (snapshotId is null)
        {
            snapshot = new Snapshot 
            { 
                SnapshotDate = date, 
                FamilyId = familyId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            _db.Snapshots.Add(snapshot);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Creating new snapshot for {SnapshotDate} for family {FamilyId}", date, familyId);
        }
        else
        {
            var existing = await _db.Snapshots
                .Include(s => s.Lines)
                .Include(s => s.Investments)
                .Include(s => s.Receivables)
                .FirstOrDefaultAsync(s => s.Id == snapshotId.Value && !s.IsDeleted);
            
            if (existing == null)
            {
                _logger.LogWarning("Snapshot {SnapshotId} not found for update", snapshotId);
                return ServiceResult<int>.Fail("Snapshot non trovato");
            }
            
            snapshot = existing;
            snapshot.SnapshotDate = date;
            snapshot.UpdatedAt = DateTime.UtcNow;
            snapshot.UpdatedBy = userId;
            _logger.LogInformation("Updating snapshot {SnapshotId} for {SnapshotDate}", snapshotId, date);
        }

        // Upsert Lines
        foreach (var (accountId, amount, contribBasis) in accountAmounts)
        {
            var existingLine = snapshot.Lines.FirstOrDefault(l => l.AccountId == accountId);
            if (existingLine is null)
            {
                snapshot.Lines.Add(new SnapshotLine
                {
                    SnapshotId = snapshot.Id,
                    AccountId = accountId,
                    Amount = amount,
                    ContributionBasis = contribBasis
                });
            }
            else
            {
                existingLine.Amount = amount;
                existingLine.ContributionBasis = contribBasis;
            }
        }

        // Replace Investments & Receivables
        _db.InvestmentAssets.RemoveRange(snapshot.Investments);
        snapshot.Investments = investments
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => new InvestmentAsset
            {
                SnapshotId = snapshot.Id,
                Name = x.Name.Trim(),
                CostBasis = x.CostBasis,
                Value = x.Value,
                PortfolioId = x.PortfolioId
            }).ToList();

        _db.Receivables.RemoveRange(snapshot.Receivables);
        snapshot.Receivables = receivables
            .Where(r => !string.IsNullOrWhiteSpace(r.Description))
            .Select(r => new Receivable
            {
                SnapshotId = snapshot.Id,
                Description = r.Description.Trim(),
                Amount = r.Amount,
                Status = r.Status,
                ExpectedDate = r.ExpectedDate
            }).ToList();

        await _db.SaveChangesAsync();
        return ServiceResult<int>.Ok(snapshot.Id);
    }

    // Legacy method for backward compatibility
    public async Task<int> SaveAsync(int familyId, int? snapshotId, DateOnly date,
        List<(int AccountId, decimal Amount, decimal ContributionBasis)> accountAmounts,
        List<(string Name, decimal CostBasis, decimal Value, int? PortfolioId)> investments,
        List<(string Description, decimal Amount, ReceivableStatus Status, DateOnly? ExpectedDate)> receivables)
    {
        var result = await SaveAsync(familyId, snapshotId, date, accountAmounts, investments, receivables, null);
        return result.Success ? result.Value : throw new BusinessRuleException(result.Errors);
    }

    public async Task<ServiceResult> DeleteAsync(int id, string? userId = null)
    {
        var snapshot = await _db.Snapshots.FirstOrDefaultAsync(x => x.Id == id);

        if (snapshot == null)
        {
            _logger.LogWarning("Snapshot {SnapshotId} not found for deletion", id);
            return ServiceResult.Fail("Snapshot non trovato");
        }

        // Soft delete
        snapshot.IsDeleted = true;
        snapshot.DeletedAt = DateTime.UtcNow;
        snapshot.DeletedBy = userId;
        
        await _db.SaveChangesAsync();
        _logger.LogInformation("Soft-deleted snapshot {SnapshotId} for date {SnapshotDate}", id, snapshot.SnapshotDate);
        
        return ServiceResult.Ok();
    }

    // Legacy method for backward compatibility
    public async Task DeleteAsync(int id) => await DeleteAsync(id, null);

    public Task<Totals> CalculateTotalsAsync(Snapshot snapshot)
    {
        var lines = snapshot.Lines;
        var liquidity = lines.Where(l => l.Account.Category == AccountCategory.Liquidity).Sum(l => l.Amount);
        var interestLiquidity = lines.Where(l => l.Account.Category == AccountCategory.Liquidity && l.Account.IsInterest).Sum(l => l.Amount);

        var pensionLines = lines.Where(l => l.Account.Category is AccountCategory.Pension or AccountCategory.Insurance);
        var pensionInsuranceValue = pensionLines.Sum(l => l.Amount);
        var pensionInsuranceContrib = pensionLines.Sum(l => l.ContributionBasis);
        var pensionInsuranceGainLoss = pensionInsuranceValue - pensionInsuranceContrib;

        var investmentsValue = snapshot.Investments.Sum(i => i.Value);
        var investmentsCost = snapshot.Investments.Sum(i => i.CostBasis);
        var investmentsGainLoss = investmentsValue - investmentsCost;

        var creditsOpen = snapshot.Receivables.Where(r => r.Status == ReceivableStatus.Open).Sum(r => r.Amount);

        var currentTotal = liquidity + investmentsValue;
        
        return Task.FromResult(new Totals(
            liquidity, investmentsValue, investmentsCost, investmentsGainLoss,
            creditsOpen, pensionInsuranceValue, pensionInsuranceContrib, pensionInsuranceGainLoss,
            currentTotal, currentTotal + creditsOpen, currentTotal + creditsOpen + pensionInsuranceValue,
            interestLiquidity));
    }

    /// <summary>
    /// Optimized method to get all snapshots with calculated totals in a SINGLE database query
    /// Avoids N+1 problem by using projection instead of loading full entities
    /// </summary>
    public async Task<List<SnapshotSummary>> GetAllWithTotalsAsync(int familyId)
    {
        _logger.LogDebug("Fetching all snapshots with totals for family {FamilyId}", familyId);
        return await _db.Snapshots
            .Where(s => s.FamilyId == familyId && !s.IsDeleted)
            .OrderBy(s => s.SnapshotDate)
            .Select(s => new SnapshotSummary(
                s.Id,
                s.SnapshotDate,
                // Liquidity: sum of lines where account category is Liquidity
                s.Lines.Where(l => l.Account.Category == AccountCategory.Liquidity).Sum(l => l.Amount),
                // Investments Value
                s.Investments.Sum(i => i.Value),
                // Investments Cost
                s.Investments.Sum(i => i.CostBasis),
                // Credits Open
                s.Receivables.Where(r => r.Status == ReceivableStatus.Open).Sum(r => r.Amount),
                // Pension/Insurance Value
                s.Lines.Where(l => l.Account.Category == AccountCategory.Pension || l.Account.Category == AccountCategory.Insurance).Sum(l => l.Amount),
                // Pension/Insurance Contributions
                s.Lines.Where(l => l.Account.Category == AccountCategory.Pension || l.Account.Category == AccountCategory.Insurance).Sum(l => l.ContributionBasis),
                // Interest Liquidity
                s.Lines.Where(l => l.Account.Category == AccountCategory.Liquidity && l.Account.IsInterest).Sum(l => l.Amount)
            ))
            .ToListAsync();
    }
}

