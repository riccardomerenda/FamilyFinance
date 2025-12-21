using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Tests.Services;

public class AccountServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _service = new AccountService(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var family = new Family { Id = 1, Name = "Test Family" };
        _context.Families.Add(family);

        _context.Accounts.AddRange(
            new Account { Id = 1, Name = "Checking", Category = AccountCategory.Liquidity, FamilyId = 1 },
            new Account { Id = 2, Name = "Savings", Category = AccountCategory.Liquidity, FamilyId = 1 },
            new Account { Id = 3, Name = "Pension Fund", Category = AccountCategory.Pension, FamilyId = 1, IsActive = true },
            new Account { Id = 4, Name = "Inactive Account", Category = AccountCategory.Liquidity, FamilyId = 1, IsActive = false },
            new Account { Id = 5, Name = "Other Family Account", Category = AccountCategory.Liquidity, FamilyId = 2 }
        );

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyFamilyAccounts()
    {
        // Act
        var accounts = await _service.GetAllAsync(1);

        // Assert
        Assert.Equal(4, accounts.Count);
        Assert.All(accounts, a => Assert.Equal(1, a.FamilyId));
    }

    [Fact]
    public async Task GetAllAsync_FiltersOutOtherFamilies()
    {
        // Act
        var accounts = await _service.GetAllAsync(1);

        // Assert
        Assert.DoesNotContain(accounts, a => a.Name == "Other Family Account");
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveAccounts()
    {
        // Act
        var accounts = await _service.GetActiveAsync(1);

        // Assert
        Assert.Equal(3, accounts.Count);
        Assert.All(accounts, a => Assert.True(a.IsActive));
        Assert.DoesNotContain(accounts, a => a.Name == "Inactive Account");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectAccount()
    {
        // Act
        var account = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(account);
        Assert.Equal("Checking", account.Name);
    }

    [Fact]
    public async Task SaveAsync_CreatesNewAccount()
    {
        // Arrange
        var newAccount = new Account
        {
            Name = "New Investment",
            Category = AccountCategory.Liquidity,
            FamilyId = 1
        };

        // Act
        await _service.SaveAsync(newAccount);

        // Assert
        var accounts = await _service.GetAllAsync(1);
        Assert.Contains(accounts, a => a.Name == "New Investment");
    }

    [Fact]
    public async Task SaveAsync_UpdatesExistingAccount()
    {
        // Arrange
        var account = await _context.Accounts.FindAsync(1);
        account!.Name = "Updated Checking";

        // Act
        await _service.SaveAsync(account);

        // Assert
        var updated = await _context.Accounts.FindAsync(1);
        Assert.Equal("Updated Checking", updated!.Name);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesAccount()
    {
        // Act
        await _service.DeleteAsync(1);

        // Assert
        var account = await _context.Accounts.FindAsync(1);
        Assert.NotNull(account);
        Assert.False(account.IsActive); // Soft delete sets IsActive = false
    }

    [Fact]
    public async Task GetAllAsync_GroupsByCategory()
    {
        // Act
        var accounts = await _service.GetAllAsync(1);

        // Assert
        var liquidityAccounts = accounts.Where(a => a.Category == AccountCategory.Liquidity).ToList();
        var pensionAccounts = accounts.Where(a => a.Category == AccountCategory.Pension).ToList();

        Assert.Equal(3, liquidityAccounts.Count); // Including inactive
        Assert.Single(pensionAccounts);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
