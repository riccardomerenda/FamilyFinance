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

        modelBuilder.Entity<SnapshotLine>().HasIndex(x => new { x.SnapshotId, x.AccountId }).IsUnique();
        
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
    }
}
