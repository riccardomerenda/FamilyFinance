using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FamilyFinance.Tests.Services;

public class SnapshotServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly SnapshotService _service;

    public SnapshotServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        var logger = new Mock<ILogger<SnapshotService>>();
        _service = new SnapshotService(_context, logger.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var family = new Family { Id = 1, Name = "Test Family" };
        _context.Families.Add(family);

        var account = new Account
        {
            Id = 1,
            Name = "Checking Account",
            Category = AccountCategory.Liquidity,
            FamilyId = 1,
            IsActive = true
        };
        _context.Accounts.Add(account);

        var pensionAccount = new Account
        {
            Id = 2,
            Name = "Pension Fund",
            Category = AccountCategory.Pension,
            FamilyId = 1,
            IsActive = true
        };
        _context.Accounts.Add(pensionAccount);

        var portfolio = new Portfolio
        {
            Id = 1,
            Name = "Long Term",
            FamilyId = 1
        };
        _context.Portfolios.Add(portfolio);

        var snapshot1 = new Snapshot
        {
            Id = 1,
            SnapshotDate = new DateOnly(2024, 1, 1),
            FamilyId = 1
        };
        _context.Snapshots.Add(snapshot1);
        _context.SaveChanges();

        _context.SnapshotLines.AddRange(
            new SnapshotLine { Id = 1, SnapshotId = 1, AccountId = 1, Amount = 10000, ContributionBasis = 10000 },
            new SnapshotLine { Id = 2, SnapshotId = 1, AccountId = 2, Amount = 5000, ContributionBasis = 4500 }
        );

        _context.InvestmentAssets.Add(new InvestmentAsset
        {
            Id = 1,
            SnapshotId = 1,
            Name = "VWCE",
            CostBasis = 8000,
            Value = 9500,
            PortfolioId = 1
        });

        _context.Receivables.Add(new Receivable
        {
            Id = 1,
            SnapshotId = 1,
            Description = "Loan to friend",
            Amount = 1000,
            Status = ReceivableStatus.Open
        });

        var snapshot2 = new Snapshot
        {
            Id = 2,
            SnapshotDate = new DateOnly(2024, 2, 1),
            FamilyId = 1
        };
        _context.Snapshots.Add(snapshot2);

        // Other family snapshot
        var snapshot3 = new Snapshot
        {
            Id = 3,
            SnapshotDate = new DateOnly(2024, 1, 15),
            FamilyId = 2
        };
        _context.Snapshots.Add(snapshot3);

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyFamilySnapshots()
    {
        // Act
        var snapshots = await _service.GetAllAsync(1);

        // Assert
        Assert.Equal(2, snapshots.Count);
        Assert.All(snapshots, s => Assert.Equal(1, s.FamilyId));
    }

    [Fact]
    public async Task GetAllAsync_OrdersByDateDescending()
    {
        // Act
        var snapshots = await _service.GetAllAsync(1);

        // Assert
        Assert.Equal(new DateOnly(2024, 2, 1), snapshots[0].SnapshotDate);
        Assert.Equal(new DateOnly(2024, 1, 1), snapshots[1].SnapshotDate);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsSnapshotWithIncludes()
    {
        // Act
        var snapshot = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(2, snapshot.Lines.Count);
        Assert.Single(snapshot.Investments);
        Assert.Single(snapshot.Receivables);
    }

    [Fact]
    public async Task GetLatestAsync_ReturnsLatestSnapshot()
    {
        // Act
        var snapshot = await _service.GetLatestAsync(1);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(new DateOnly(2024, 2, 1), snapshot.SnapshotDate);
    }

    [Fact]
    public async Task CalculateTotalsAsync_CalculatesCorrectly()
    {
        // Arrange
        var snapshot = await _service.GetByIdAsync(1);

        // Act
        var totals = await _service.CalculateTotalsAsync(snapshot!);

        // Assert
        Assert.Equal(10000, totals.Liquidity);
        Assert.Equal(9500, totals.InvestmentsValue);
        Assert.Equal(8000, totals.InvestmentsCost);
        Assert.Equal(1500, totals.InvestmentsGainLoss);
        Assert.Equal(1000, totals.CreditsOpen);
        Assert.Equal(5000, totals.PensionInsuranceValue);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesSnapshot()
    {
        // Act
        await _service.DeleteAsync(1);

        // Assert
        var snapshot = await _context.Snapshots.FindAsync(1);
        Assert.NotNull(snapshot);
        Assert.True(snapshot!.IsDeleted);
        Assert.NotNull(snapshot.DeletedAt);
    }

    [Fact]
    public async Task GetAllAsync_ExcludesDeletedSnapshots()
    {
        // Arrange - Soft delete a snapshot
        var snapshot = await _context.Snapshots.FindAsync(1);
        snapshot!.IsDeleted = true;
        await _context.SaveChangesAsync();

        // Act
        var snapshots = await _service.GetAllAsync(1);

        // Assert
        Assert.Single(snapshots);
        Assert.DoesNotContain(snapshots, s => s.Id == 1);
    }

    [Fact]
    public async Task GetAllWithTotalsAsync_ReturnsAggregatedData()
    {
        // Act
        var summaries = await _service.GetAllWithTotalsAsync(1);

        // Assert
        Assert.Equal(2, summaries.Count);
        
        // Check first snapshot has calculated values
        var jan = summaries.First(s => s.Date == new DateOnly(2024, 1, 1));
        Assert.Equal(10000, jan.Liquidity);
        Assert.Equal(9500, jan.InvestmentsValue);
    }

    [Fact]
    public async Task SaveAsync_CreatesNewSnapshot()
    {
        // Arrange
        var date = new DateOnly(2024, 3, 1);
        var accounts = new List<(int, decimal, decimal)> { (1, 12000, 12000) };
        var investments = new List<(string, decimal, decimal, int?)> { ("BTC", 1000, 1500, 1) };
        var receivables = new List<(string, decimal, ReceivableStatus, DateOnly?)>();

        // Act
        var result = await _service.SaveAsync(1, null, date, accounts, investments, receivables, "test-user");

        // Assert
        Assert.True(result.Success);
        var snapshot = await _service.GetByIdAsync(result.Value);
        Assert.NotNull(snapshot);
        Assert.Equal(date, snapshot.SnapshotDate);
        Assert.Equal("test-user", snapshot.CreatedBy);
    }

    [Fact]
    public async Task SaveAsync_ValidatesDate()
    {
        // Arrange - Future date too far
        var date = DateOnly.FromDateTime(DateTime.Today.AddMonths(6));
        var accounts = new List<(int, decimal, decimal)>();
        var investments = new List<(string, decimal, decimal, int?)>();
        var receivables = new List<(string, decimal, ReceivableStatus, DateOnly?)>();

        // Act
        var result = await _service.SaveAsync(1, null, date, accounts, investments, receivables, "test-user");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("futuro", result.Error?.ToLower() ?? "");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

