using FamilyFinance.Models;

namespace FamilyFinance.Services.Interfaces;

/// <summary>
/// Filter options for querying transactions
/// </summary>
public class TransactionFilter
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int? CategoryId { get; set; }
    public int? AccountId { get; set; }
    public TransactionType? Type { get; set; }
    public string? SearchText { get; set; }
    public int Take { get; set; } = 50;
    public int Skip { get; set; } = 0;
}

/// <summary>
/// Service interface for managing individual transactions
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Get all transactions for a family with optional filtering
    /// </summary>
    Task<List<Transaction>> GetAllAsync(int familyId, TransactionFilter? filter = null);
    
    /// <summary>
    /// Get a single transaction by ID
    /// </summary>
    Task<Transaction?> GetByIdAsync(long id);
    
    /// <summary>
    /// Create or update a transaction
    /// </summary>
    Task<ServiceResult> SaveAsync(Transaction transaction);
    
    /// <summary>
    /// Soft-delete a transaction
    /// </summary>
    Task<ServiceResult> DeleteAsync(long id);
    
    /// <summary>
    /// Get total amount by category for a date range
    /// </summary>
    Task<decimal> GetTotalByCategoryAsync(int familyId, int categoryId, DateOnly from, DateOnly to);
    
    /// <summary>
    /// Get transactions grouped by category for a date range
    /// </summary>
    Task<List<ReportCategorySummary>> GetSummaryByCategoryAsync(int familyId, DateOnly from, DateOnly to, TransactionType type);
    
    /// <summary>
    /// Get count of transactions for a family
    /// </summary>
    Task<int> GetCountAsync(int familyId, TransactionFilter? filter = null);

    /// <summary>
    /// Create multiple transactions in a batch (e.g. from import)
    /// </summary>
    Task<ServiceResult> CreateManyAsync(List<Transaction> transactions);

    /// <summary>
    /// Get all transactions for a specific month
    /// </summary>
    Task<List<Transaction>> GetByMonthAsync(int familyId, int year, int month);
}
