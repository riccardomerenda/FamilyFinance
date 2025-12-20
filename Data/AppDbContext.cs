using FamilyFinance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FamilyFinance.Data;

public class AppDbContext : DbContext
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Snapshot> Snapshots => Set<Snapshot>();
    public DbSet<SnapshotLine> SnapshotLines => Set<SnapshotLine>();
    public DbSet<InvestmentAsset> InvestmentAssets => Set<InvestmentAsset>();
    public DbSet<Receivable> Receivables => Set<Receivable>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var dateOnlyConverter = new ValueConverter<DateOnly, string>(
            v => v.ToString("yyyy-MM-dd"), v => DateOnly.Parse(v));

        var nullableDateOnlyConverter = new ValueConverter<DateOnly?, string?>(
            v => v.HasValue ? v.Value.ToString("yyyy-MM-dd") : null,
            v => v == null ? null : DateOnly.Parse(v));

        modelBuilder.Entity<Snapshot>().Property(x => x.SnapshotDate).HasConversion(dateOnlyConverter);
        modelBuilder.Entity<Receivable>().Property(x => x.ExpectedDate).HasConversion(nullableDateOnlyConverter);

        modelBuilder.Entity<SnapshotLine>().HasIndex(x => new { x.SnapshotId, x.AccountId }).IsUnique();
        
        // Seed Dati Iniziali (i tuoi conti)
        modelBuilder.Entity<Account>().HasData(
            new Account { Id = 1, Name = "BBVA - Capitale", Category = AccountCategory.Liquidity },
            new Account { Id = 2, Name = "BBVA - Interessi", Category = AccountCategory.Liquidity, IsInterest = true },
            new Account { Id = 3, Name = "ING - Capitale", Category = AccountCategory.Liquidity },
            new Account { Id = 4, Name = "ING - Interessi", Category = AccountCategory.Liquidity, IsInterest = true },
            new Account { Id = 5, Name = "UniCredit Riccardo", Category = AccountCategory.Liquidity, Owner = "Riccardo" },
            new Account { Id = 6, Name = "UniCredit Valentina", Category = AccountCategory.Liquidity, Owner = "Valentina" },
            new Account { Id = 7, Name = "FON.TE", Category = AccountCategory.Pension, Owner = "Riccardo" },
            new Account { Id = 8, Name = "Allianz", Category = AccountCategory.Insurance }
        );
        base.OnModelCreating(modelBuilder);
    }
}

