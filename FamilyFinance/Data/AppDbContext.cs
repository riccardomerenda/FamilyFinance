using FamilyFinance.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FamilyFinance.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public DbSet<Family> Families => Set<Family>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Snapshot> Snapshots => Set<Snapshot>();
    public DbSet<SnapshotLine> SnapshotLines => Set<SnapshotLine>();
    public DbSet<InvestmentAsset> InvestmentAssets => Set<InvestmentAsset>();
    public DbSet<Receivable> Receivables => Set<Receivable>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<BudgetCategory> BudgetCategories => Set<BudgetCategory>();
    public DbSet<MonthlyExpense> MonthlyExpenses => Set<MonthlyExpense>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<CategoryRule> CategoryRules => Set<CategoryRule>();
    public DbSet<MonthlyIncome> MonthlyIncomes => Set<MonthlyIncome>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<RecurringTransaction> RecurringTransactions => Set<RecurringTransaction>();
    public DbSet<RecurringMatchRule> RecurringMatchRules => Set<RecurringMatchRule>();
    public DbSet<AssetHolding> AssetHoldings => Set<AssetHolding>();
    public DbSet<PensionHolding> PensionHoldings => Set<PensionHolding>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Important for Identity tables
        
        var dateOnlyConverter = new ValueConverter<DateOnly, string>(
            v => v.ToString("yyyy-MM-dd"), v => DateOnly.Parse(v));

        var nullableDateOnlyConverter = new ValueConverter<DateOnly?, string?>(
            v => v.HasValue ? v.Value.ToString("yyyy-MM-dd") : null,
            v => v == null ? null : DateOnly.Parse(v));

        modelBuilder.Entity<Snapshot>().Property(x => x.SnapshotDate).HasConversion(dateOnlyConverter);
        modelBuilder.Entity<Receivable>().Property(x => x.ExpectedDate).HasConversion(nullableDateOnlyConverter);
        modelBuilder.Entity<Goal>().Property(x => x.Deadline).HasConversion(nullableDateOnlyConverter);
        modelBuilder.Entity<Transaction>().Property(x => x.Date).HasConversion(dateOnlyConverter);
        modelBuilder.Entity<RecurringTransaction>().Property(x => x.StartDate).HasConversion(dateOnlyConverter);
        modelBuilder.Entity<RecurringTransaction>().Property(x => x.EndDate).HasConversion(nullableDateOnlyConverter);

        modelBuilder.Entity<SnapshotLine>().HasIndex(x => new { x.SnapshotId, x.AccountId }).IsUnique();
        
        // Performance indexes for frequent queries
        modelBuilder.Entity<Snapshot>().HasIndex(x => new { x.FamilyId, x.SnapshotDate });
        modelBuilder.Entity<Account>().HasIndex(x => x.FamilyId);
        modelBuilder.Entity<Goal>().HasIndex(x => x.FamilyId);
        modelBuilder.Entity<Portfolio>().HasIndex(x => x.FamilyId);
        modelBuilder.Entity<BudgetCategory>().HasIndex(x => x.FamilyId);
        
        // Family relationships
        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Family)
            .WithMany(f => f.Members)
            .HasForeignKey(u => u.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Account>()
            .HasOne(a => a.Family)
            .WithMany(f => f.Accounts)
            .HasForeignKey(a => a.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Snapshot>()
            .HasOne(s => s.Family)
            .WithMany(f => f.Snapshots)
            .HasForeignKey(s => s.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Goal>()
            .HasOne(g => g.Family)
            .WithMany(f => f.Goals)
            .HasForeignKey(g => g.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Portfolio>()
            .HasOne(p => p.Family)
            .WithMany(f => f.Portfolios)
            .HasForeignKey(p => p.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BudgetCategory>()
            .HasOne(b => b.Family)
            .WithMany(f => f.BudgetCategories)
            .HasForeignKey(b => b.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MonthlyExpense>()
            .HasOne(e => e.Snapshot)
            .WithMany(s => s.Expenses)
            .HasForeignKey(e => e.SnapshotId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MonthlyExpense>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Don't delete expenses when category is deleted

        modelBuilder.Entity<MonthlyExpense>()
            .HasIndex(e => new { e.SnapshotId, e.CategoryId })
            .IsUnique(); // One expense per category per snapshot

        // Activity Log indexes for efficient querying
        modelBuilder.Entity<ActivityLog>()
            .HasIndex(a => new { a.FamilyId, a.Timestamp });
        modelBuilder.Entity<ActivityLog>()
            .HasIndex(a => a.UserId);
        modelBuilder.Entity<ActivityLog>()
            .HasIndex(a => a.Action);

        // CategoryRule indexes for smart categorization
        modelBuilder.Entity<CategoryRule>()
            .HasIndex(r => new { r.FamilyId, r.Keyword });
        modelBuilder.Entity<CategoryRule>()
            .HasOne(r => r.Category)
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MonthlyIncome>()
            .HasOne(i => i.Category)
            .WithMany()
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MonthlyIncome>()
            .HasOne(i => i.Snapshot)
            .WithMany(s => s.Incomes)
            .HasForeignKey(i => i.SnapshotId)
            .OnDelete(DeleteBehavior.Cascade);

        // Transaction configuration
        modelBuilder.Entity<Transaction>()
            .HasIndex(t => new { t.FamilyId, t.Date });
        modelBuilder.Entity<Transaction>()
            .HasIndex(t => new { t.FamilyId, t.CategoryId });
        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.ExternalId);
        
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Family)
            .WithMany()
            .HasForeignKey(t => t.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Account)
            .WithMany()
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.ImportBatch)
            .WithMany()
            .HasForeignKey(t => t.ImportBatchId)
            .OnDelete(DeleteBehavior.SetNull);

        // RecurringTransaction configuration
        modelBuilder.Entity<RecurringTransaction>()
            .HasIndex(r => r.FamilyId);
        
        modelBuilder.Entity<RecurringTransaction>()
            .HasOne(r => r.Family)
            .WithMany()
            .HasForeignKey(r => r.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<RecurringTransaction>()
            .HasOne(r => r.Category)
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<RecurringTransaction>()
            .HasOne(r => r.Account)
            .WithMany()
            .HasForeignKey(r => r.AccountId)
            .OnDelete(DeleteBehavior.SetNull);

        // AssetHolding configuration
        modelBuilder.Entity<AssetHolding>()
            .HasIndex(a => new { a.FamilyId, a.Ticker });
        modelBuilder.Entity<AssetHolding>()
            .HasIndex(a => a.PortfolioId);
        
        modelBuilder.Entity<AssetHolding>()
            .HasOne(a => a.Family)
            .WithMany()
            .HasForeignKey(a => a.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<AssetHolding>()
            .HasOne(a => a.Portfolio)
            .WithMany()
            .HasForeignKey(a => a.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);

        // PensionHolding configuration
        modelBuilder.Entity<PensionHolding>()
            .HasIndex(p => new { p.FamilyId, p.AccountId })
            .IsUnique();
        
        modelBuilder.Entity<PensionHolding>()
            .HasOne(p => p.Family)
            .WithMany()
            .HasForeignKey(p => p.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<PensionHolding>()
            .HasOne(p => p.Account)
            .WithMany()
            .HasForeignKey(p => p.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
