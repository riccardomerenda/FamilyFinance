using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using FamilyFinance.Services.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamilyFinance.Services;

public class GoalService : IGoalService
{
    private readonly AppDbContext _db;
    private readonly ILogger<GoalService> _logger;

    public GoalService(AppDbContext db, ILogger<GoalService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<Goal>> GetAllAsync(int familyId)
    {
        _logger.LogDebug("Fetching all goals for family {FamilyId}", familyId);
        return await _db.Goals
            .Where(g => g.FamilyId == familyId && !g.IsDeleted)
            .OrderByDescending(g => g.Id)
            .ToListAsync();
    }

    public async Task<Goal?> GetByIdAsync(int id)
    {
        _logger.LogDebug("Fetching goal {GoalId}", id);
        return await _db.Goals.FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
    }

    public async Task<ServiceResult> SaveAsync(Goal goal, string? userId = null)
    {
        // Validate
        var validation = goal.Validate();
        if (!validation.Success)
        {
            _logger.LogWarning("Goal validation failed: {Errors}", string.Join(", ", validation.Errors));
            return validation;
        }

        if (goal.Id == 0)
        {
            // Create
            goal.CreatedAt = DateTime.UtcNow;
            goal.CreatedBy = userId;
            _db.Goals.Add(goal);
            _logger.LogInformation("Creating new goal '{GoalName}' for family {FamilyId}", goal.Name, goal.FamilyId);
        }
        else
        {
            // Update
            var existing = await _db.Goals.FindAsync(goal.Id);
            if (existing == null || existing.IsDeleted)
            {
                _logger.LogWarning("Goal {GoalId} not found for update", goal.Id);
                return ServiceResult.Fail("Obiettivo non trovato");
            }

            existing.Name = goal.Name;
            existing.Target = goal.Target;
            existing.AllocatedAmount = goal.AllocatedAmount;
            existing.Deadline = goal.Deadline;
            existing.Priority = goal.Priority;
            existing.Category = goal.Category;
            existing.ShowMonthlyTarget = goal.ShowMonthlyTarget;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = userId;
            
            _logger.LogInformation("Updating goal {GoalId} '{GoalName}'", goal.Id, goal.Name);
        }
        
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // Legacy method for backward compatibility
    public async Task SaveAsync(Goal goal) => await SaveAsync(goal, null);

    public async Task<ServiceResult> DeleteAsync(int id, string? userId = null)
    {
        var goal = await _db.Goals.FirstOrDefaultAsync(g => g.Id == id);
        if (goal == null)
        {
            _logger.LogWarning("Goal {GoalId} not found for deletion", id);
            return ServiceResult.Fail("Obiettivo non trovato");
        }

        // Soft delete
        goal.IsDeleted = true;
        goal.DeletedAt = DateTime.UtcNow;
        goal.DeletedBy = userId;
        
        await _db.SaveChangesAsync();
        _logger.LogInformation("Soft-deleted goal {GoalId} '{GoalName}'", id, goal.Name);
        
        return ServiceResult.Ok();
    }

    // Legacy method for backward compatibility
    public async Task DeleteAsync(int id) => await DeleteAsync(id, null);
}

