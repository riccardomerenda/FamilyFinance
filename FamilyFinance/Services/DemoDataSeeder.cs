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
    public const string DemoPassword = "demo2024";
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
            return; // Demo data already exists
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
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to create demo user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Create demo accounts
        var accounts = new List<Account>
        {
            new() { Name = "Conto Corrente BancaX", Category = AccountCategory.Liquidity, FamilyId = family.Id, Owner = "Famiglia" },
            new() { Name = "Conto Deposito", Category = AccountCategory.Liquidity, FamilyId = family.Id, Owner = "Famiglia", IsInterest = true },
            new() { Name = "Fondo Pensione", Category = AccountCategory.Pension, FamilyId = family.Id, Owner = "Mario" },
            new() { Name = "Polizza Vita", Category = AccountCategory.Insurance, FamilyId = family.Id, Owner = "Laura" },
        };
        _db.Accounts.AddRange(accounts);
        await _db.SaveChangesAsync();

        // Create demo portfolios
        var portfolios = new List<Portfolio>
        {
            new() { Name = "PAC Lungo Termine", Description = "Piano di accumulo 20 anni", TimeHorizonYears = 20, TargetYear = 2045, Color = "#6366f1", FamilyId = family.Id },
            new() { Name = "Crypto", Description = "Bitcoin ed Ethereum", TimeHorizonYears = 5, Color = "#f59e0b", FamilyId = family.Id },
            new() { Name = "ETF Dividendi", Description = "Income strategy", TimeHorizonYears = 10, Color = "#10b981", FamilyId = family.Id },
        };
        _db.Portfolios.AddRange(portfolios);
        await _db.SaveChangesAsync();

        // Create demo goals
        var goals = new List<Goal>
        {
            new() { Name = "Fondo Emergenza", Target = 15000, AllocatedAmount = 12000, Deadline = "2025-06", Priority = GoalPriority.High, Category = GoalCategory.Liquidity, FamilyId = family.Id },
            new() { Name = "Vacanza Giappone", Target = 8000, AllocatedAmount = 3500, Deadline = "2025-09", Priority = GoalPriority.Medium, Category = GoalCategory.Liquidity, FamilyId = family.Id },
            new() { Name = "Anticipo Casa", Target = 50000, AllocatedAmount = 22000, Deadline = "2027-12", Priority = GoalPriority.High, Category = GoalCategory.Investments, FamilyId = family.Id },
            new() { Name = "Auto Nuova", Target = 25000, AllocatedAmount = 8000, Deadline = "2026-06", Priority = GoalPriority.Low, Category = GoalCategory.Liquidity, FamilyId = family.Id },
            new() { Name = "Università Figli", Target = 80000, AllocatedAmount = 15000, Deadline = "2035-09", Priority = GoalPriority.Medium, Category = GoalCategory.Investments, FamilyId = family.Id },
        };
        _db.Goals.AddRange(goals);
        await _db.SaveChangesAsync();

        // Create demo budget categories
        var budgetCategories = new List<BudgetCategory>
        {
            new() { Name = "Casa", Icon = "🏠", Color = "#6366f1", MonthlyBudget = 1200, FamilyId = family.Id },
            new() { Name = "Alimentari", Icon = "🛒", Color = "#10b981", MonthlyBudget = 600, FamilyId = family.Id },
            new() { Name = "Trasporti", Icon = "🚗", Color = "#f59e0b", MonthlyBudget = 300, FamilyId = family.Id },
            new() { Name = "Utenze", Icon = "💡", Color = "#ef4444", MonthlyBudget = 250, FamilyId = family.Id },
            new() { Name = "Svago", Icon = "🎬", Color = "#8b5cf6", MonthlyBudget = 200, FamilyId = family.Id },
            new() { Name = "Salute", Icon = "💊", Color = "#ec4899", MonthlyBudget = 150, FamilyId = family.Id },
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
                FamilyId = family.Id,
                Notes = $"Snapshot {date:MMMM yyyy}",
                CreatedBy = user.DisplayName
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

            // Add monthly expenses (for the latest snapshot)
            if (date == snapshotDates.Last())
            {
                var expenses = new List<MonthlyExpense>
                {
                    new() { SnapshotId = snapshot.Id, CategoryId = budgetCategories[0].Id, Amount = 1150 },
                    new() { SnapshotId = snapshot.Id, CategoryId = budgetCategories[1].Id, Amount = 580 },
                    new() { SnapshotId = snapshot.Id, CategoryId = budgetCategories[2].Id, Amount = 220 },
                    new() { SnapshotId = snapshot.Id, CategoryId = budgetCategories[3].Id, Amount = 180 },
                    new() { SnapshotId = snapshot.Id, CategoryId = budgetCategories[4].Id, Amount = 250 },
                    new() { SnapshotId = snapshot.Id, CategoryId = budgetCategories[5].Id, Amount = 45 },
                };
                _db.MonthlyExpenses.AddRange(expenses);
            }

            await _db.SaveChangesAsync();
        }
    }
}

