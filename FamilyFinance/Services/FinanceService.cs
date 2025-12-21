using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Services;

/// <summary>
/// Legacy facade service - delegates to domain-specific services.
/// Will be removed once all pages are migrated to use specific services.
/// </summary>
public class FinanceService
{
    private readonly ISnapshotService _snapshots;
    private readonly IAccountService _accounts;
    private readonly IGoalService _goals;
    private readonly IPortfolioService _portfolios;
    private readonly IBudgetService _budget;
    private readonly IImportExportService _importExport;
    private readonly AuthService _auth;

    public FinanceService(
        ISnapshotService snapshots,
        IAccountService accounts,
        IGoalService goals,
        IPortfolioService portfolios,
        IBudgetService budget,
        IImportExportService importExport,
        AuthService auth)
    {
        _snapshots = snapshots;
        _accounts = accounts;
        _goals = goals;
        _portfolios = portfolios;
        _budget = budget;
        _importExport = importExport;
        _auth = auth;
    }

    // Expose AuthService for pages that need it
    public AuthService Auth => _auth;

    // Snapshots - delegate to ISnapshotService
    public Task<List<Snapshot>> GetSnapshotsAsync(int familyId) 
        => _snapshots.GetAllAsync(familyId);
    
    public Task<Snapshot?> GetSnapshotAsync(int id) 
        => _snapshots.GetByIdAsync(id);

    public Task<Snapshot?> GetLatestSnapshotAsync(int familyId) 
        => _snapshots.GetLatestAsync(familyId);

    public Task<Totals> CalculateTotalsAsync(Snapshot snapshot) 
        => _snapshots.CalculateTotalsAsync(snapshot);

    public Task<int> SaveSnapshotAsync(int familyId, int? snapshotId, DateOnly date,
        List<(int AccountId, decimal Amount, decimal ContributionBasis)> accountAmounts,
        List<(string Name, decimal CostBasis, decimal Value, int? PortfolioId)> investments,
        List<(string Description, decimal Amount, ReceivableStatus Status, DateOnly? ExpectedDate)> receivables)
        => _snapshots.SaveAsync(familyId, snapshotId, date, accountAmounts, investments, receivables);

    public Task DeleteSnapshotAsync(int id) 
        => _snapshots.DeleteAsync(id);

    // Accounts - delegate to IAccountService
    public Task<List<Account>> GetAccountsAsync(int familyId) 
        => _accounts.GetAllAsync(familyId);

    public Task<List<Account>> GetActiveAccountsAsync(int familyId) 
        => _accounts.GetActiveAsync(familyId);

    public Task SaveAccountAsync(Account account) 
        => _accounts.SaveAsync(account);

    // Goals - delegate to IGoalService
    public Task<List<Goal>> GetGoalsAsync(int familyId) 
        => _goals.GetAllAsync(familyId);

    public Task SaveGoalAsync(Goal goal) 
        => _goals.SaveAsync(goal);

    public Task DeleteGoalAsync(long id) 
        => _goals.DeleteAsync(id);

    // Portfolios - delegate to IPortfolioService
    public Task<List<Portfolio>> GetPortfoliosAsync(int familyId) 
        => _portfolios.GetAllAsync(familyId);

    public Task<Portfolio?> GetPortfolioAsync(int id) 
        => _portfolios.GetByIdAsync(id);

    public Task SavePortfolioAsync(Portfolio portfolio) 
        => _portfolios.SaveAsync(portfolio);

    public Task DeletePortfolioAsync(int id) 
        => _portfolios.DeleteAsync(id);

    // Budget - delegate to IBudgetService
    public Task<List<BudgetCategory>> GetBudgetCategoriesAsync(int familyId) 
        => _budget.GetCategoriesAsync(familyId);

    public Task SaveBudgetCategoryAsync(BudgetCategory category) 
        => _budget.SaveCategoryAsync(category);

    public Task DeleteBudgetCategoryAsync(int id) 
        => _budget.DeleteCategoryAsync(id);

    public List<BudgetCategory> GetDefaultBudgetCategories() 
        => _budget.GetDefaultCategories();

    public Task InitializeBudgetCategoriesAsync(int familyId) 
        => _budget.InitializeCategoriesAsync(familyId);

    public Task<List<MonthlyExpense>> GetMonthlyExpensesAsync(int snapshotId) 
        => _budget.GetExpensesAsync(snapshotId);

    public Task SaveMonthlyExpensesAsync(int snapshotId, List<(int CategoryId, decimal Amount, string? Notes)> expenses) 
        => _budget.SaveExpensesAsync(snapshotId, expenses);

    public Task<BudgetSummary> GetBudgetSummaryAsync(int snapshotId, int familyId) 
        => _budget.GetSummaryAsync(snapshotId, familyId);

    // Import/Export - delegate to IImportExportService
    public Task<ImportPreview> PreviewImportAsync(BackupDto backup, int familyId) 
        => _importExport.PreviewImportAsync(backup, familyId);

    public Task<ImportResult> ImportDataAsync(BackupDto backup, bool replaceExisting, int familyId) 
        => _importExport.ImportDataAsync(backup, replaceExisting, familyId);

    public Task ClearAllDataAsync(int familyId) 
        => _importExport.ClearAllDataAsync(familyId);
}
