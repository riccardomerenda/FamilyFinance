using FamilyFinance.Models;

namespace FamilyFinance.Services.Interfaces;

public interface IAccountService
{
    Task<List<Account>> GetAllAsync(int familyId);
    Task<List<Account>> GetActiveAsync(int familyId);
    Task<Account?> GetByIdAsync(int id);
    Task SaveAsync(Account account);
    Task DeleteAsync(int id);
}

