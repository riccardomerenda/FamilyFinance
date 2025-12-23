using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
namespace FamilyFinance.Services;

/// <summary>
/// Facade service for backward compatibility with pages still using Finance.*
/// This delegates all calls to the granular services.
/// TODO: Gradually migrate pages to use services directly and remove this facade.
/// </summary>
public class FinanceService
{
    private readonly ISnapshotService _snapshotService;
    private readonly IAccountService _accountService;
    private readonly IGoalService _goalService;
    private readonly IPortfolioService _portfolioService;
    private readonly IBudgetService _budgetService;
    public readonly AuthService Auth;
    
    public FinanceService(
        ISnapshotService snapshotService,
        IAccountService accountService,
        IGoalService goalService,
        IPortfolioService portfolioService,
        IBudgetService budgetService,
        AuthService authService)
    {
        _snapshotService = snapshotService;
        _accountService = accountService;
        _goalService = goalService;
        _portfolioService = portfolioService;
        _budgetService = budgetService;
        Auth = authService;
    }
    
    // Snapshots
    public Task<List<Snapshot>> GetSnapshotsAsync(int familyId) => _snapshotService.GetAllAsync(familyId);
    public Task<Snapshot?> GetSnapshotAsync(int id) => _snapshotService.GetByIdAsync(id);
    public Task<Snapshot?> GetLatestSnapshotAsync(int familyId) => _snapshotService.GetLatestAsync(familyId);
    public Task<List<SnapshotSummary>> GetSnapshotsWithTotalsAsync(int familyId) => _snapshotService.GetAllWithTotalsAsync(familyId);
    public Task<Totals> CalculateTotalsAsync(Snapshot s) => _snapshotService.CalculateTotalsAsync(s);
    public Task DeleteSnapshotAsync(int id) => _snapshotService.DeleteAsync(id);
    public Task<int> SaveSnapshotAsync(int familyId, int? id, DateOnly date, 
        List<(int AccountId, decimal Amount, decimal ContributionBasis)> accounts,
        List<(string Name, decimal CostBasis, decimal Value, int? PortfolioId)> investments,
        List<(string Description, decimal Amount, ReceivableStatus Status, DateOnly? ExpectedDate)> receivables)
        => _snapshotService.SaveAsync(familyId, id, date, accounts, investments, receivables);
    public Task SaveMonthlyExpensesAsync(int snapshotId, List<(int CategoryId, decimal Amount, string? Notes)> expenses)
        => _snapshotService.SaveExpensesAsync(snapshotId, expenses);
    public Task<List<MonthlyExpense>> GetMonthlyExpensesAsync(int snapshotId)
        => _snapshotService.GetExpensesAsync(snapshotId);
    
    // Accounts
    public Task<List<Account>> GetAccountsAsync(int familyId) => _accountService.GetAllAsync(familyId);
    public Task<List<Account>> GetActiveAccountsAsync(int familyId) => _accountService.GetActiveAsync(familyId);
    
    // Goals
    public Task<List<Goal>> GetGoalsAsync(int familyId) => _goalService.GetAllAsync(familyId);
    public Task SaveGoalAsync(Goal goal) => _goalService.SaveAsync(goal);
    public Task DeleteGoalAsync(int id) => _goalService.DeleteAsync(id);
    
    // Portfolios
    public Task<List<Portfolio>> GetPortfoliosAsync(int familyId) => _portfolioService.GetAllAsync(familyId);
    public Task SavePortfolioAsync(Portfolio portfolio) => _portfolioService.SaveAsync(portfolio);
    public Task DeletePortfolioAsync(int id) => _portfolioService.DeleteAsync(id);
    
    // Budget
    public Task<List<BudgetCategory>> GetBudgetCategoriesAsync(int familyId) => _budgetService.GetCategoriesAsync(familyId);
}
