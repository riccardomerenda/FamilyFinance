using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Tests.Services;

public class GoalServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly GoalService _service;

    public GoalServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _service = new GoalService(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var family = new Family { Id = 1, Name = "Test Family" };
        _context.Families.Add(family);

        _context.Goals.AddRange(
            new Goal
            {
                Id = 1,
                Name = "Emergency Fund",
                Target = 10000,
                AllocatedAmount = 5000,
                Deadline = "2026-12",
                Priority = GoalPriority.High,
                FamilyId = 1
            },
            new Goal
            {
                Id = 2,
                Name = "Vacation",
                Target = 3000,
                AllocatedAmount = 3000, // Completed
                Deadline = "2025-06",
                Priority = GoalPriority.Medium,
                FamilyId = 1
            },
            new Goal
            {
                Id = 3,
                Name = "New Car",
                Target = 25000,
                AllocatedAmount = 2000,
                Deadline = "2028-12",
                Priority = GoalPriority.Low,
                FamilyId = 1
            },
            new Goal
            {
                Id = 4,
                Name = "Other Family Goal",
                Target = 5000,
                FamilyId = 2
            }
        );

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyFamilyGoals()
    {
        // Act
        var goals = await _service.GetAllAsync(1);

        // Assert
        Assert.Equal(3, goals.Count);
        Assert.All(goals, g => Assert.Equal(1, g.FamilyId));
    }

    [Fact]
    public async Task GetAllAsync_FiltersOutOtherFamilies()
    {
        // Act
        var goals = await _service.GetAllAsync(1);

        // Assert
        Assert.DoesNotContain(goals, g => g.Name == "Other Family Goal");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectGoal()
    {
        // Act
        var goal = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(goal);
        Assert.Equal("Emergency Fund", goal.Name);
    }

    [Fact]
    public async Task SaveAsync_CreatesNewGoal()
    {
        // Arrange
        var newGoal = new Goal
        {
            Name = "House Down Payment",
            Target = 50000,
            Priority = GoalPriority.High,
            FamilyId = 1
        };

        // Act
        await _service.SaveAsync(newGoal);

        // Assert
        var goals = await _service.GetAllAsync(1);
        Assert.Contains(goals, g => g.Name == "House Down Payment");
    }

    [Fact]
    public void Goal_IsCompleted_WhenAllocatedEqualsTarget()
    {
        // Arrange
        var goal = new Goal { Target = 3000, AllocatedAmount = 3000 };

        // Assert
        Assert.True(goal.IsCompleted);
    }

    [Fact]
    public void Goal_IsNotCompleted_WhenAllocatedLessThanTarget()
    {
        // Arrange
        var goal = new Goal { Target = 10000, AllocatedAmount = 5000 };

        // Assert
        Assert.False(goal.IsCompleted);
    }

    [Fact]
    public void Goal_ProgressPercent_CalculatesCorrectly()
    {
        // Arrange
        var goal = new Goal { Target = 10000, AllocatedAmount = 5000 };

        // Assert
        Assert.Equal(50, goal.ProgressPercent);
    }

    [Fact]
    public void Goal_Missing_CalculatesCorrectly()
    {
        // Arrange
        var goal = new Goal { Target = 10000, AllocatedAmount = 5000 };

        // Assert
        Assert.Equal(5000, goal.Missing);
    }

    [Fact]
    public async Task DeleteAsync_RemovesGoal()
    {
        // Act
        await _service.DeleteAsync(1);

        // Assert
        var goal = await _context.Goals.FindAsync(1L);
        Assert.Null(goal);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
