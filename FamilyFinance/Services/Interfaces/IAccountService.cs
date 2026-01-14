using FamilyFinance.Models;

namespace FamilyFinance.Services.Interfaces;

public interface IAccountService
{
    Task<List<Account>> GetAllAsync(int familyId);
    Task<List<Account>> GetActiveAsync(int familyId);
    Task<Account?> GetByIdAsync(int id);
    Task SaveAsync(Account account);
    Task DeleteAsync(int id);
    
    /// <summary>
    /// Updates the account's current balance by adding a delta (can be negative for expenses)
    /// </summary>
    Task<ServiceResult> UpdateBalanceAsync(int accountId, decimal delta);
    
    /// <summary>
    /// Sets the account's current balance to a specific value
    /// </summary>
    Task<ServiceResult> SetBalanceAsync(int accountId, decimal newBalance);
}

