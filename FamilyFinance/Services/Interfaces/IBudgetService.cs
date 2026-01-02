using FamilyFinance.Models;

namespace FamilyFinance.Services.Interfaces;

public interface IBudgetService
{
    // Categories
    Task<List<BudgetCategory>> GetCategoriesAsync(int familyId, CategoryType? type = null);
    Task<BudgetCategory?> GetCategoryByIdAsync(int id);
    Task SaveCategoryAsync(BudgetCategory category);
    Task DeleteCategoryAsync(int id);
    List<BudgetCategory> GetDefaultCategories();
    Task InitializeCategoriesAsync(int familyId);
    
    // Expenses
    Task<List<MonthlyExpense>> GetExpensesAsync(int snapshotId);
    Task SaveExpensesAsync(int snapshotId, List<(int CategoryId, decimal Amount, string? Notes)> expenses);
    Task AddImportedExpensesAsync(int snapshotId, List<(int CategoryId, decimal Amount, string? Notes)> expenses, string fileName = "Import", string? userId = null);
    
    // Import History
    Task<List<ImportBatch>> GetImportBatchesAsync();
    Task<ServiceResult> RevertImportBatchAsync(int batchId);
    Task<decimal> GetTotalExpensesAsync(int snapshotId);
    Task<decimal> GetTotalBudgetAsync(int familyId);
    Task<BudgetSummary> GetSummaryAsync(int snapshotId, int familyId);
}

public record BudgetSummary(
    decimal TotalBudget,
    decimal TotalSpent,
    decimal Difference,
    decimal PercentUsed,
    List<CategorySummary> Categories
);

public record CategorySummary(
    int Id,
    string Name,
    string Icon,
    string Color,
    decimal Budget,
    decimal Spent,
    decimal Difference,
    decimal PercentUsed,
    bool IsOverBudget
);

