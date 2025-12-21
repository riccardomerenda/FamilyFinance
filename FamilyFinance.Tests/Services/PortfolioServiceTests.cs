using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Tests.Services;

public class PortfolioServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PortfolioService _service;

    public PortfolioServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _service = new PortfolioService(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var family = new Family { Id = 1, Name = "Test Family" };
        _context.Families.Add(family);

        _context.Portfolios.AddRange(
            new Portfolio
            {
                Id = 1,
                Name = "Long Term DCA",
                Description = "Monthly investments",
                TimeHorizonYears = 20,
                TargetYear = 2045,
                Color = "#10b981",
                FamilyId = 1
            },
            new Portfolio
            {
                Id = 2,
                Name = "Crypto",
                Description = "High risk",
                TimeHorizonYears = 5,
                TargetYear = 2030,
                Color = "#f59e0b",
                FamilyId = 1
            },
            new Portfolio
            {
                Id = 3,
                Name = "Other Family Portfolio",
                FamilyId = 2
            }
        );

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyFamilyPortfolios()
    {
        // Act
        var portfolios = await _service.GetAllAsync(1);

        // Assert
        Assert.Equal(2, portfolios.Count);
        Assert.All(portfolios, p => Assert.Equal(1, p.FamilyId));
    }

    [Fact]
    public async Task GetAllAsync_FiltersOutOtherFamilies()
    {
        // Act
        var portfolios = await _service.GetAllAsync(1);

        // Assert
        Assert.DoesNotContain(portfolios, p => p.Name == "Other Family Portfolio");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectPortfolio()
    {
        // Act
        var portfolio = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(portfolio);
        Assert.Equal("Long Term DCA", portfolio.Name);
    }

    [Fact]
    public async Task SaveAsync_CreatesNewPortfolio()
    {
        // Arrange
        var newPortfolio = new Portfolio
        {
            Name = "Retirement Fund",
            TimeHorizonYears = 30,
            TargetYear = 2055,
            FamilyId = 1
        };

        // Act
        await _service.SaveAsync(newPortfolio);

        // Assert
        var portfolios = await _service.GetAllAsync(1);
        Assert.Contains(portfolios, p => p.Name == "Retirement Fund");
    }

    [Fact]
    public async Task SaveAsync_UpdatesExistingPortfolio()
    {
        // Arrange
        var portfolio = await _context.Portfolios.FindAsync(1);
        portfolio!.Name = "Updated DCA";

        // Act
        await _service.SaveAsync(portfolio);

        // Assert
        var updated = await _context.Portfolios.FindAsync(1);
        Assert.Equal("Updated DCA", updated!.Name);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesPortfolio()
    {
        // Act
        await _service.DeleteAsync(1);

        // Assert
        var portfolio = await _context.Portfolios.FindAsync(1);
        Assert.NotNull(portfolio);
        Assert.False(portfolio.IsActive); // Soft delete sets IsActive = false
    }

    [Fact]
    public async Task Portfolio_HasCorrectTargetYear()
    {
        // Act
        var portfolio = await _service.GetByIdAsync(1);

        // Assert
        Assert.Equal(2045, portfolio!.TargetYear);
    }

    [Fact]
    public async Task Portfolio_HasCorrectTimeHorizon()
    {
        // Act
        var portfolio = await _service.GetByIdAsync(1);

        // Assert
        Assert.Equal(20, portfolio!.TimeHorizonYears);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
