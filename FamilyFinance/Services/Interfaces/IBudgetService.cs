using FamilyFinance.Models;

namespace FamilyFinance.Services.Interfaces;

public interface IBudgetService
{
    // Categories
    Task<List<BudgetCategory>> GetCategoriesAsync(int familyId);
    Task<BudgetCategory?> GetCategoryByIdAsync(int id);
    Task<ServiceResult> SaveCategoryAsync(BudgetCategory category, string? userId = null);
    Task<ServiceResult> DeleteCategoryAsync(int id, string? userId = null);
    List<BudgetCategory> GetDefaultCategories();
    Task InitializeCategoriesAsync(int familyId);
    
    // Expenses
    Task<List<MonthlyExpense>> GetExpensesAsync(int snapshotId);
    Task SaveExpensesAsync(int snapshotId, List<(int CategoryId, decimal Amount, string? Notes)> expenses);
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

