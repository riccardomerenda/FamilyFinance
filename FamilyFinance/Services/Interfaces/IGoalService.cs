using FamilyFinance.Models;

namespace FamilyFinance.Services.Interfaces;

public interface IGoalService
{
    Task<List<Goal>> GetAllAsync(int familyId);
    Task<Goal?> GetByIdAsync(long id);
    Task SaveAsync(Goal goal);
    Task DeleteAsync(long id);
}

