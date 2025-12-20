using FamilyFinance.Data;
using FamilyFinance.Models;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Services;

public class FinanceService
{
    private readonly AppDbContext _db;
    public FinanceService(AppDbContext db) => _db = db;

    // Snapshots
    public async Task<List<Snapshot>> GetSnapshotsAsync()
        => await _db.Snapshots.OrderByDescending(s => s.SnapshotDate).ToListAsync();

    public async Task<Snapshot?> GetSnapshotAsync(int id)
        => await _db.Snapshots
            .Include(s => s.Lines).ThenInclude(l => l.Account)
            .Include(s => s.Investments).ThenInclude(i => i.Portfolio)
            .Include(s => s.Receivables)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Snapshot?> GetLatestSnapshotAsync()
        => await _db.Snapshots
            .Include(s => s.Lines).ThenInclude(l => l.Account)
            .Include(s => s.Investments).ThenInclude(i => i.Portfolio)
            .Include(s => s.Receivables)
            .OrderByDescending(s => s.SnapshotDate)
            .FirstOrDefaultAsync();

    public async Task<List<Account>> GetActiveAccountsAsync()
        => await _db.Accounts.Where(a => a.IsActive).OrderBy(a => a.Category).ThenBy(a => a.Name).ToListAsync();

    public Task<Totals> CalculateTotalsAsync(Snapshot snapshot)
    {
        var lines = snapshot.Lines;
        var liquidity = lines.Where(l => l.Account.Category == AccountCategory.Liquidity).Sum(l => l.Amount);
        var interestLiquidity = lines.Where(l => l.Account.Category == AccountCategory.Liquidity && l.Account.IsInterest).Sum(l => l.Amount);
        var pensionInsurance = lines.Where(l => l.Account.Category is AccountCategory.Pension or AccountCategory.Insurance).Sum(l => l.Amount);
        var investmentsValue = snapshot.Investments.Sum(i => i.Value);
        var investmentsCost = snapshot.Investments.Sum(i => i.CostBasis);
        var investmentsGainLoss = investmentsValue - investmentsCost;
        var creditsOpen = snapshot.Receivables.Where(r => r.Status == ReceivableStatus.Open).Sum(r => r.Amount);

        var currentTotal = liquidity + investmentsValue;
        return Task.FromResult(new Totals(
            liquidity, investmentsValue, investmentsCost, investmentsGainLoss, 
            creditsOpen, pensionInsurance, currentTotal, 
            currentTotal + creditsOpen, currentTotal + creditsOpen + pensionInsurance, 
            interestLiquidity));
    }

    public async Task SaveSnapshotAsync(int? snapshotId, DateOnly date, List<(int AccountId, decimal Amount)> accountAmounts,
        List<(string Name, decimal CostBasis, decimal Value, int? PortfolioId)> investments,
        List<(string Description, decimal Amount, ReceivableStatus Status, DateOnly? ExpectedDate)> receivables)
    {
        Snapshot snapshot;
        if (snapshotId is null) {
            snapshot = new Snapshot { SnapshotDate = date };
            _db.Snapshots.Add(snapshot);
            await _db.SaveChangesAsync(); // Save to get ID
        } else {
            snapshot = await _db.Snapshots.Include(s => s.Lines).Include(s => s.Investments).Include(s => s.Receivables).FirstAsync(s => s.Id == snapshotId.Value);
            snapshot.SnapshotDate = date;
        }

        // Upsert Lines
        foreach (var (accountId, amount) in accountAmounts) {
            var existing = snapshot.Lines.FirstOrDefault(l => l.AccountId == accountId);
            if (existing is null) snapshot.Lines.Add(new SnapshotLine { SnapshotId = snapshot.Id, AccountId = accountId, Amount = amount });
            else existing.Amount = amount;
        }

        // Replace Investments & Receivables
        _db.InvestmentAssets.RemoveRange(snapshot.Investments);
        snapshot.Investments = investments.Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => new InvestmentAsset { 
                SnapshotId = snapshot.Id, 
                Broker = "Directa", 
                Name = x.Name.Trim(), 
                CostBasis = x.CostBasis, 
                Value = x.Value,
                PortfolioId = x.PortfolioId
            }).ToList();

        _db.Receivables.RemoveRange(snapshot.Receivables);
        snapshot.Receivables = receivables.Where(r => !string.IsNullOrWhiteSpace(r.Description))
            .Select(r => new Receivable { SnapshotId = snapshot.Id, Description = r.Description.Trim(), Amount = r.Amount, Status = r.Status, ExpectedDate = r.ExpectedDate }).ToList();

        await _db.SaveChangesAsync();
    }

    public async Task DeleteSnapshotAsync(int id) {
        var s = await _db.Snapshots.Include(x=>x.Lines).Include(x=>x.Investments).Include(x=>x.Receivables).FirstOrDefaultAsync(x=>x.Id == id);
        if(s != null) { _db.Snapshots.Remove(s); await _db.SaveChangesAsync(); }
    }

    // Portfolios
    public async Task<List<Portfolio>> GetPortfoliosAsync()
        => await _db.Portfolios.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();

    public async Task<Portfolio?> GetPortfolioAsync(int id)
        => await _db.Portfolios.FirstOrDefaultAsync(p => p.Id == id);

    public async Task SavePortfolioAsync(Portfolio portfolio)
    {
        if (portfolio.Id == 0)
        {
            portfolio.CreatedAt = DateTime.UtcNow;
            _db.Portfolios.Add(portfolio);
        }
        else
        {
            _db.Portfolios.Update(portfolio);
        }
        await _db.SaveChangesAsync();
    }

    public async Task DeletePortfolioAsync(int id)
    {
        var p = await _db.Portfolios.FirstOrDefaultAsync(x => x.Id == id);
        if (p != null)
        {
            p.IsActive = false; // Soft delete
            await _db.SaveChangesAsync();
        }
    }

    // Goals
    public async Task<List<Goal>> GetGoalsAsync() => await _db.Goals.OrderByDescending(g => g.Id).ToListAsync();
    public async Task SaveGoalAsync(Goal goal) {
        if (goal.Id == 0) { goal.Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); _db.Goals.Add(goal); }
        else _db.Goals.Update(goal);
        await _db.SaveChangesAsync();
    }
    public async Task DeleteGoalAsync(long id) {
        var g = await _db.Goals.FirstOrDefaultAsync(x=>x.Id==id); if(g!=null){_db.Goals.Remove(g); await _db.SaveChangesAsync();}
    }
}
