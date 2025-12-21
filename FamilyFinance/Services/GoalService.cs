using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Services;

public class GoalService : IGoalService
{
    private readonly AppDbContext _db;

    public GoalService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Goal>> GetAllAsync(int familyId)
        => await _db.Goals
            .Where(g => g.FamilyId == familyId)
            .OrderByDescending(g => g.Id)
            .ToListAsync();

    public async Task<Goal?> GetByIdAsync(long id)
        => await _db.Goals.FirstOrDefaultAsync(g => g.Id == id);

    public async Task SaveAsync(Goal goal)
    {
        if (goal.Id == 0)
        {
            goal.Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _db.Goals.Add(goal);
        }
        else
        {
            var existing = await _db.Goals.FindAsync(goal.Id);
            if (existing != null)
            {
                existing.Name = goal.Name;
                existing.Target = goal.Target;
                existing.AllocatedAmount = goal.AllocatedAmount;
                existing.Deadline = goal.Deadline;
                existing.Priority = goal.Priority;
                existing.Category = goal.Category;
                existing.ShowMonthlyTarget = goal.ShowMonthlyTarget;
            }
        }
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var goal = await _db.Goals.FirstOrDefaultAsync(g => g.Id == id);
        if (goal != null)
        {
            _db.Goals.Remove(goal);
            await _db.SaveChangesAsync();
        }
    }
}

