using FamilyFinance.Models;

namespace FamilyFinance.Services.Interfaces;

public interface IPortfolioService
{
    Task<List<Portfolio>> GetAllAsync(int familyId);
    Task<Portfolio?> GetByIdAsync(int id);
    Task SaveAsync(Portfolio portfolio);
    Task DeleteAsync(int id);
}

