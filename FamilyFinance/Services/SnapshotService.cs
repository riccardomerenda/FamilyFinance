using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Services;

public class SnapshotService : ISnapshotService
{
    private readonly AppDbContext _db;

    public SnapshotService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Snapshot>> GetAllAsync(int familyId)
        => await _db.Snapshots
            .Where(s => s.FamilyId == familyId)
            .OrderByDescending(s => s.SnapshotDate)
            .ToListAsync();

    public async Task<Snapshot?> GetByIdAsync(int id)
        => await _db.Snapshots
            .Include(s => s.Lines).ThenInclude(l => l.Account)
            .Include(s => s.Investments).ThenInclude(i => i.Portfolio)
            .Include(s => s.Receivables)
            .Include(s => s.Expenses).ThenInclude(e => e.Category)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Snapshot?> GetLatestAsync(int familyId)
        => await _db.Snapshots
            .Include(s => s.Lines).ThenInclude(l => l.Account)
            .Include(s => s.Investments).ThenInclude(i => i.Portfolio)
            .Include(s => s.Receivables)
            .Include(s => s.Expenses).ThenInclude(e => e.Category)
            .Where(s => s.FamilyId == familyId)
            .OrderByDescending(s => s.SnapshotDate)
            .FirstOrDefaultAsync();

    public async Task<int> SaveAsync(int familyId, int? snapshotId, DateOnly date,
        List<(int AccountId, decimal Amount, decimal ContributionBasis)> accountAmounts,
        List<(string Name, decimal CostBasis, decimal Value, int? PortfolioId)> investments,
        List<(string Description, decimal Amount, ReceivableStatus Status, DateOnly? ExpectedDate)> receivables)
    {
        Snapshot snapshot;
        if (snapshotId is null)
        {
            snapshot = new Snapshot { SnapshotDate = date, FamilyId = familyId };
            _db.Snapshots.Add(snapshot);
            await _db.SaveChangesAsync();
        }
        else
        {
            snapshot = await _db.Snapshots
                .Include(s => s.Lines)
                .Include(s => s.Investments)
                .Include(s => s.Receivables)
                .FirstAsync(s => s.Id == snapshotId.Value);
            snapshot.SnapshotDate = date;
        }

        // Upsert Lines
        foreach (var (accountId, amount, contribBasis) in accountAmounts)
        {
            var existing = snapshot.Lines.FirstOrDefault(l => l.AccountId == accountId);
            if (existing is null)
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
                existing.Amount = amount;
                existing.ContributionBasis = contribBasis;
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
        return snapshot.Id;
    }

    public async Task DeleteAsync(int id)
    {
        var snapshot = await _db.Snapshots
            .Include(x => x.Lines)
            .Include(x => x.Investments)
            .Include(x => x.Receivables)
            .Include(x => x.Expenses)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (snapshot != null)
        {
            _db.Snapshots.Remove(snapshot);
            await _db.SaveChangesAsync();
        }
    }

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
}

