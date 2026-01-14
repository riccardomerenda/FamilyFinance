using FamilyFinance.Models;

namespace FamilyFinance.Services.Interfaces;

/// <summary>
/// Represents an upcoming transaction occurrence
/// </summary>
public class UpcomingTransaction
{
    public RecurringTransaction Recurring { get; set; } = default!;
    public DateOnly NextDate { get; set; }
    public int DaysUntil { get; set; }
}

/// <summary>
/// Service interface for managing recurring transactions
/// </summary>
public interface IRecurringTransactionService
{
    /// <summary>
    /// Get all recurring transactions for a family
    /// </summary>
    Task<List<RecurringTransaction>> GetAllAsync(int familyId, bool activeOnly = true);
    
    /// <summary>
    /// Get a single recurring transaction by ID
    /// </summary>
    Task<RecurringTransaction?> GetByIdAsync(int id);
    
    /// <summary>
    /// Create or update a recurring transaction
    /// </summary>
    Task<ServiceResult> SaveAsync(RecurringTransaction recurring);
    
    /// <summary>
    /// Soft-delete a recurring transaction
    /// </summary>
    Task<ServiceResult> DeleteAsync(int id);
    
    /// <summary>
    /// Get upcoming transactions for the next N days
    /// </summary>
    Task<List<UpcomingTransaction>> GetUpcomingAsync(int familyId, int daysAhead = 30);
    
    /// <summary>
    /// Get total monthly amount by transaction type
    /// </summary>
    Task<decimal> GetMonthlyTotalAsync(int familyId, TransactionType type);
    
    /// <summary>
    /// Generate transactions from recurring templates for a given month
    /// </summary>
    Task<List<Transaction>> GenerateTransactionsForMonthAsync(int familyId, int year, int month);
    Task<ServiceResult<int>> ProcessRecurringForMonthAsync(int familyId, int year, int month);
}
