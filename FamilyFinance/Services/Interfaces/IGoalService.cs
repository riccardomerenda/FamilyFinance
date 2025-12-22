using FamilyFinance.Models;

namespace FamilyFinance.Services.Interfaces;

public interface IGoalService
{
    Task<List<Goal>> GetAllAsync(int familyId);
    Task<Goal?> GetByIdAsync(int id);
    Task SaveAsync(Goal goal);
    Task DeleteAsync(int id);
}

