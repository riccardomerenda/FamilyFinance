using FamilyFinance.Data;
using FamilyFinance.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Services;

public class DemoDataSeeder
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public const string DemoEmail = "demo@familyfinance.app";
    public const string DemoPassword = "demo2026";
    public const string DemoFamilyName = "Famiglia Demo";

    public DemoDataSeeder(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task SeedDemoDataAsync()
    {
        // Check if demo user already exists
        var existingUser = await _userManager.FindByEmailAsync(DemoEmail);
        if (existingUser != null)
        {
            // Ensure password is up to date
            if (!await _userManager.CheckPasswordAsync(existingUser, DemoPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
                await _userManager.ResetPasswordAsync(existingUser, token, DemoPassword);
            }
            return; // Data assumed to exist
        }

        // Create demo family
        var family = new Family { Name = DemoFamilyName };
        _db.Families.Add(family);
        await _db.SaveChangesAsync();

        // Create demo user
        var user = new AppUser
        {
            UserName = DemoEmail,
            Email = DemoEmail,
            DisplayName = "Utente Demo",
            FamilyId = family.Id,
            Role = UserRole.Admin,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, DemoPassword);
        if (!result.Succeeded) throw new Exception("Failed to create demo user");

        await SeedFinancialDataAsync(family.Id, user.Id);
    }

    public async Task ForceResetAsync(int familyId, string userId)
    {
        // 1. Delete all dependent data
        _db.MonthlyExpenses.RemoveRange(_db.MonthlyExpenses.Where(x => x.Snapshot != null && x.Snapshot.FamilyId == familyId));
        _db.SnapshotLines.RemoveRange(_db.SnapshotLines.Where(x => x.Snapshot.FamilyId == familyId));
        _db.InvestmentAssets.RemoveRange(_db.InvestmentAssets.Where(x => x.Snapshot.FamilyId == familyId));
        _db.Receivables.RemoveRange(_db.Receivables.Where(x => x.Snapshot.FamilyId == familyId)); // Linked to snapshot
        _db.Snapshots.RemoveRange(_db.Snapshots.Where(x => x.FamilyId == familyId));
        
        _db.Goals.RemoveRange(_db.Goals.Where(x => x.FamilyId == familyId));
        _db.BudgetCategories.RemoveRange(_db.BudgetCategories.Where(x => x.FamilyId == familyId));
        
        // These might have constraints if linked to other things, but in demo mode should be fine
        _db.Portfolios.RemoveRange(_db.Portfolios.Where(x => x.FamilyId == familyId));
        _db.Accounts.RemoveRange(_db.Accounts.Where(x => x.FamilyId == familyId));
        _db.ImportBatches.RemoveRange(_db.ImportBatches); // Global cleanup or filtered if we had FamilyId (we don't yet, so clear all for demo safety)
        
        await _db.SaveChangesAsync();

        // 2. Re-seed
        await SeedFinancialDataAsync(familyId, userId);
    }

    private async Task SeedFinancialDataAsync(int familyId, string userId)
    {
        // Create demo accounts
        var accounts = new List<Account>
        {
            new() { Name = "Conto Corrente BancaX", Category = AccountCategory.Liquidity, FamilyId = familyId, Owner = "Famiglia", CreatedBy = userId },
            new() { Name = "Conto Deposito", Category = AccountCategory.Liquidity, FamilyId = familyId, Owner = "Famiglia", IsInterest = true, CreatedBy = userId },
            new() { Name = "Fondo Pensione", Category = AccountCategory.Pension, FamilyId = familyId, Owner = "Mario", CreatedBy = userId },
            new() { Name = "Polizza Vita", Category = AccountCategory.Insurance, FamilyId = familyId, Owner = "Laura", CreatedBy = userId },
        };
        _db.Accounts.AddRange(accounts);
        await _db.SaveChangesAsync();

        // Create demo portfolios
        var portfolios = new List<Portfolio>
        {
            new() { Name = "PAC Lungo Termine", Description = "Piano di accumulo 20 anni", TimeHorizonYears = 20, TargetYear = 2045, Color = "#6366f1", FamilyId = familyId, CreatedBy = userId },
            new() { Name = "Crypto", Description = "Bitcoin ed Ethereum", TimeHorizonYears = 5, Color = "#f59e0b", FamilyId = familyId, CreatedBy = userId },
            new() { Name = "ETF Dividendi", Description = "Income strategy", TimeHorizonYears = 10, Color = "#10b981", FamilyId = familyId, CreatedBy = userId },
        };
        _db.Portfolios.AddRange(portfolios);
        await _db.SaveChangesAsync();

        // Create demo goals
        var goals = new List<Goal>
        {
            new() { Name = "Fondo Emergenza", Target = 15000, AllocatedAmount = 12000, Deadline = new DateOnly(2025, 6, 1), Priority = GoalPriority.High, Category = GoalCategory.Liquidity, FamilyId = familyId, CreatedBy = userId },
            new() { Name = "Vacanza Giappone", Target = 8000, AllocatedAmount = 3500, Deadline = new DateOnly(2025, 9, 1), Priority = GoalPriority.Medium, Category = GoalCategory.Liquidity, FamilyId = familyId, CreatedBy = userId },
            new() { Name = "Anticipo Casa", Target = 50000, AllocatedAmount = 22000, Deadline = new DateOnly(2027, 12, 1), Priority = GoalPriority.High, Category = GoalCategory.Investments, FamilyId = familyId, CreatedBy = userId },
            new() { Name = "Auto Nuova", Target = 25000, AllocatedAmount = 8000, Deadline = new DateOnly(2026, 6, 1), Priority = GoalPriority.Low, Category = GoalCategory.Liquidity, FamilyId = familyId, CreatedBy = userId },
            new() { Name = "Università Figli", Target = 80000, AllocatedAmount = 15000, Deadline = new DateOnly(2035, 9, 1), Priority = GoalPriority.Medium, Category = GoalCategory.Investments, FamilyId = familyId, CreatedBy = userId },
        };
        _db.Goals.AddRange(goals);
        await _db.SaveChangesAsync();

        // Create demo budget categories
        var budgetCategories = new List<BudgetCategory>
        {
            new() { Name = "Casa", Icon = "🏠", Color = "#6366f1", MonthlyBudget = 1200, FamilyId = familyId, CreatedBy = userId },
            new() { Name = "Alimentari", Icon = "🛒", Color = "#10b981", MonthlyBudget = 600, FamilyId = familyId, CreatedBy = userId },
            new() { Name = "Trasporti", Icon = "🚗", Color = "#f59e0b", MonthlyBudget = 300, FamilyId = familyId, CreatedBy = userId },
            new() { Name = "Utenze", Icon = "💡", Color = "#ef4444", MonthlyBudget = 250, FamilyId = familyId, CreatedBy = userId },
            new() { Name = "Svago", Icon = "🎬", Color = "#8b5cf6", MonthlyBudget = 200, FamilyId = familyId, CreatedBy = userId },
            new() { Name = "Salute", Icon = "💊", Color = "#ec4899", MonthlyBudget = 150, FamilyId = familyId, CreatedBy = userId },
        };
        _db.BudgetCategories.AddRange(budgetCategories);
        await _db.SaveChangesAsync();

        // Create demo snapshots (last 6 months)
        var today = DateOnly.FromDateTime(DateTime.Today);
        var snapshotDates = Enumerable.Range(0, 6)
            .Select(i => new DateOnly(today.Year, today.Month, 1).AddMonths(-i))
            .Reverse()
            .ToList();

        decimal baseTotal = 120000;
        var random = new Random(42); // Fixed seed for consistent demo data

        foreach (var date in snapshotDates)
        {
            var snapshot = new Snapshot
            {
                SnapshotDate = date,
                FamilyId = familyId,
                Notes = $"Snapshot {date:MMMM yyyy}",
                CreatedBy = userId
            };
            _db.Snapshots.Add(snapshot);
            await _db.SaveChangesAsync();

            // Growth factor (simulate ~0.5-1.5% monthly growth)
            var growthFactor = 1 + (decimal)(random.NextDouble() * 0.01 + 0.005);
            baseTotal *= growthFactor;

            // Add snapshot lines (account balances)
            var liquidityBase = baseTotal * 0.25m;
            var lines = new List<SnapshotLine>
            {
                new() { SnapshotId = snapshot.Id, AccountId = accounts[0].Id, Amount = liquidityBase * 0.7m },
                new() { SnapshotId = snapshot.Id, AccountId = accounts[1].Id, Amount = liquidityBase * 0.3m },
                new() { SnapshotId = snapshot.Id, AccountId = accounts[2].Id, Amount = 18000 + (snapshotDates.IndexOf(date) * 500), ContributionBasis = 15000 + (snapshotDates.IndexOf(date) * 400) },
                new() { SnapshotId = snapshot.Id, AccountId = accounts[3].Id, Amount = 8000 + (snapshotDates.IndexOf(date) * 200), ContributionBasis = 7500 + (snapshotDates.IndexOf(date) * 150) },
            };
            _db.SnapshotLines.AddRange(lines);

            // Add investments
            var investments = new List<InvestmentAsset>
            {
                new() { SnapshotId = snapshot.Id, Name = "VWCE", Broker = "Directa", CostBasis = 35000, Value = 35000 * (1 + (snapshotDates.IndexOf(date) * 0.02m)), PortfolioId = portfolios[0].Id },
                new() { SnapshotId = snapshot.Id, Name = "SWDA", Broker = "Directa", CostBasis = 15000, Value = 15000 * (1 + (snapshotDates.IndexOf(date) * 0.018m)), PortfolioId = portfolios[0].Id },
                new() { SnapshotId = snapshot.Id, Name = "Bitcoin", Broker = "Kraken", CostBasis = 5000, Value = 5000 * (1 + (snapshotDates.IndexOf(date) * 0.05m) - 0.1m), PortfolioId = portfolios[1].Id },
                new() { SnapshotId = snapshot.Id, Name = "VHYL", Broker = "Degiro", CostBasis = 10000, Value = 10000 * (1 + (snapshotDates.IndexOf(date) * 0.015m)), PortfolioId = portfolios[2].Id },
            };
            _db.InvestmentAssets.AddRange(investments);

            // Add receivables (only for recent snapshots)
            if (snapshotDates.IndexOf(date) >= snapshotDates.Count - 2)
            {
                var receivables = new List<Receivable>
                {
                    new() { SnapshotId = snapshot.Id, Description = "Rimborso 730", Amount = 850, Status = ReceivableStatus.Open, ExpectedDate = date.AddMonths(2) },
                };
                _db.Receivables.AddRange(receivables);
            }

            // Add monthly expenses for ALL snapshots (simulating history)
            var expenses = new List<MonthlyExpense>();
            
            // Randomize amounts slightly
            decimal noise() => (decimal)(random.NextDouble() * 0.2 - 0.1); // +/- 10%
            
            // Helper to create expense with detailed notes
            MonthlyExpense CreateExpense(int catIdx, decimal total, params (string Desc, decimal Amount)[] parts)
            {
                var remaining = total - parts.Sum(p => p.Amount);
                // Distribute remaining or add as "Altro" if distinct
                var notesParts = new List<string>();
                foreach(var p in parts) notesParts.Add($"{p.Desc} ({p.Amount:N2} €)");
                
                if (remaining != 0)
                {
                    // Adjust last or add extra
                    if (parts.Any()) 
                    {
                        // Simply adjust the first one to match total exactly (simplification for demo)
                         // But for cleanliness let's just use the exact parts for the total
                         // Logic below re-calculates total from parts to be consistent
                    }
                }
                
                return new MonthlyExpense 
                { 
                    SnapshotId = snapshot.Id, 
                    CategoryId = budgetCategories[catIdx].Id, 
                    Amount = total, 
                    Notes = string.Join("; ", notesParts) 
                };
            }

            // Case 1: Casa (Affitto + Condominio)
            var rent = 1200m;
            var condo = Math.Round(50 * (1 + noise()), 2);
            expenses.Add(CreateExpense(0, rent + condo, ("Affitto", rent), ("Condominio", condo)));

            // Case 2: Alimentari (Grocery runs)
            var grocTotal = Math.Round(600 * (1 + noise()), 2);
            var g1 = Math.Round(grocTotal * 0.4m, 2);
            var g2 = Math.Round(grocTotal * 0.3m, 2);
            var g3 = grocTotal - g1 - g2;
            expenses.Add(CreateExpense(1, grocTotal, ("Esselunga", g1), ("Coop", g2), ("Mercato", g3)));

            // Case 3: Trasporti
            var transpTotal = Math.Round(300 * (1 + noise()), 2);
            var t1 = Math.Round(transpTotal * 0.6m, 2);
            var t2 = transpTotal - t1;
            expenses.Add(CreateExpense(2, transpTotal, ("Benzina Q8", t1), ("Telepass", t2)));

            // Case 4: Utenze
            var utilTotal = Math.Round(250 * (1 + noise()), 2);
            var u1 = Math.Round(utilTotal * 0.5m, 2);
            var u2 = utilTotal - u1;
            expenses.Add(CreateExpense(3, utilTotal, ("Enel Energia", u1), ("Hera Gas", u2)));

            // Case 5: Svago
            var entTotal = Math.Round(200 * (1 + noise()), 2);
            expenses.Add(CreateExpense(4, entTotal, ("Cinema", Math.Round(entTotal*0.3m, 2)), ("Pizza", Math.Round(entTotal*0.7m, 2))));

            // Case 6: Salute (random)
            if (random.NextDouble() > 0.3) 
            {
                var healthTotal = Math.Round(100 * (1 + noise()), 2);
                expenses.Add(CreateExpense(5, healthTotal, ("Farmacia", healthTotal)));
            }

            _db.MonthlyExpenses.AddRange(expenses);

            await _db.SaveChangesAsync();
    }
}
}

