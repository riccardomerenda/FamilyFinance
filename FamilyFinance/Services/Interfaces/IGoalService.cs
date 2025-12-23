using FamilyFinance.Models;

namespace FamilyFinance.Services.Interfaces;

public interface IGoalService
{
    Task<List<Goal>> GetAllAsync(int familyId);
    Task<Goal?> GetByIdAsync(long id);
    Task<ServiceResult> SaveAsync(Goal goal, string? userId = null);
    Task<ServiceResult> DeleteAsync(long id, string? userId = null);
}

