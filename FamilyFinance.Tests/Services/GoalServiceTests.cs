using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

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
        var logger = new Mock<ILogger<GoalService>>();
        _service = new GoalService(_context, logger.Object);

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
                Deadline = new DateOnly(2026, 12, 1),
                Priority = GoalPriority.High,
                FamilyId = 1
            },
            new Goal
            {
                Id = 2,
                Name = "Vacation",
                Target = 3000,
                AllocatedAmount = 3000, // Completed
                Deadline = new DateOnly(2025, 6, 1),
                Priority = GoalPriority.Medium,
                FamilyId = 1
            },
            new Goal
            {
                Id = 3,
                Name = "New Car",
                Target = 25000,
                AllocatedAmount = 2000,
                Deadline = new DateOnly(2028, 12, 1),
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
    public async Task DeleteAsync_SoftDeletesGoal()
    {
        // Act
        await _service.DeleteAsync(1);

        // Assert - Goal still exists but is marked as deleted
        var goal = await _context.Goals.FindAsync(1);
        Assert.NotNull(goal);
        Assert.True(goal!.IsDeleted);
        Assert.NotNull(goal.DeletedAt);
    }

    [Fact]
    public async Task GetAllAsync_ExcludesDeletedGoals()
    {
        // Arrange - Soft delete a goal
        var goal = await _context.Goals.FindAsync(1);
        goal!.IsDeleted = true;
        await _context.SaveChangesAsync();

        // Act
        var goals = await _service.GetAllAsync(1);

        // Assert
        Assert.DoesNotContain(goals, g => g.Id == 1);
    }
    
    [Fact]
    public void Goal_MonthsUntilDeadline_CalculatesCorrectly()
    {
        // Arrange - Set deadline 6 months from now
        var futureDate = DateTime.Today.AddMonths(6);
        var goal = new Goal 
        { 
            Target = 10000, 
            Deadline = new DateOnly(futureDate.Year, futureDate.Month, 1) 
        };

        // Assert
        Assert.Equal(6, goal.MonthsUntilDeadline);
    }
    
    [Fact]
    public void Goal_MonthsUntilDeadline_ReturnsZero_WhenNoDeadline()
    {
        // Arrange
        var goal = new Goal { Target = 10000, Deadline = null };

        // Assert
        Assert.Equal(0, goal.MonthsUntilDeadline);
    }
    
    [Fact]
    public void Goal_DeadlineDisplay_FormatsCorrectly()
    {
        // Arrange
        var goal = new Goal { Deadline = new DateOnly(2026, 12, 1) };

        // Assert
        Assert.Equal("2026-12", goal.DeadlineDisplay);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
