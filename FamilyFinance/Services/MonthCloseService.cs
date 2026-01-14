using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilyFinance.Services;

public class MonthCloseService : IMonthCloseService
{
    private readonly ISnapshotService _snapshotService;
    private readonly IAccountService _accountService;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<MonthCloseService> _logger;

    public MonthCloseService(
        ISnapshotService snapshotService,
        IAccountService accountService,
        ITransactionService transactionService,
        ILogger<MonthCloseService> logger)
    {
        _snapshotService = snapshotService;
        _accountService = accountService;
        _transactionService = transactionService;
        _logger = logger;
    }

    public async Task<bool> ShouldPromptMonthCloseAsync(int familyId)
    {
        var dateToClose = await GetMonthToCloseAsync(familyId);
        if (dateToClose == null) return false;

        // Check if user dismissed prompt for this month (ToDo: Implement persistence for dismissed prompts)
        // For now, always prompt if the snapshot is missing
        
        return true;
    }

    public async Task<DateTime?> GetMonthToCloseAsync(int familyId)
    {
        // Default rule: 
        // If today is day 1-10 of month M, check if M-1 is closed.
        // If today is later, maybe we don't annoy user? Or we always check last closed snapshot.
        
        // Let's use a simpler rule: Check if the previous month has a snapshot.
        var today = DateTime.Today;
        var firstOfCurrentMonth = new DateTime(today.Year, today.Month, 1);
        var lastMonth = firstOfCurrentMonth.AddMonths(-1); 
        // Target closing date is the last day of the previous month
        var targetDate = new DateOnly(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));

        var snapshots = await _snapshotService.GetAllAsync(familyId);
        var hasSnapshotForTarget = snapshots.Any(s => s.SnapshotDate.Year == targetDate.Year 
                                                   && s.SnapshotDate.Month == targetDate.Month);

        if (!hasSnapshotForTarget)
        {
            return lastMonth; // Return any date in the target month
        }

        return null;
    }

    public async Task<ServiceResult<int>> CloseMonthAsync(int familyId, int year, int month)
    {
        try
        {
            var closingDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
            _logger.LogInformation("Closing month {Year}-{Month} for family {FamilyId}", year, month, familyId);

            // 1. Get Live Account Balances
            var accounts = await _accountService.GetActiveAsync(familyId);
            var accountLines = accounts.Select(a => (
                AccountId: a.Id, 
                Amount: a.CurrentBalance, 
                ContributionBasis: 0m // Basis not tracked automatically yet
            )).ToList();

            // 2. Get Investments & Receivables (Copy from latest snapshot)
            var latestSnapshot = await _snapshotService.GetLatestAsync(familyId);
            
            var investments = new List<(string Name, decimal CostBasis, decimal Value, int? PortfolioId)>();
            var receivables = new List<(string Description, decimal Amount, ReceivableStatus Status, DateOnly? ExpectedDate)>();

            if (latestSnapshot != null)
            {
                // We need to re-fetch full snapshot to get collections if GetLatestAsync doesn't include them
                // But usually GetLatestAsync logic should be checked. Let's assume we need to be safe.
                var fullLatest = await _snapshotService.GetByIdAsync(latestSnapshot.Id);
                if (fullLatest != null)
                {
                    investments = fullLatest.Investments
                        .Select(i => (i.Name, i.CostBasis, i.Value, i.PortfolioId))
                        .ToList();

                    receivables = fullLatest.Receivables
                        .Where(r => r.Status == ReceivableStatus.Open) // Only copy open receivables
                        .Select(r => (r.Description, r.Amount, r.Status, r.ExpectedDate))
                        .ToList();
                }
            }

            // 3. Create Snapshot
            var snapshotId = await _snapshotService.SaveAsync(
                familyId, 
                null, 
                closingDate, 
                accountLines, 
                investments, 
                receivables
            );

            // 4. Aggregate Transactions for MonthlyExpense/Annual Reports (Legacy Support & Performance)
            // We fetch transactions for that month and save them as MonthlyExpense records
            // enable "Transaction Mode" implicitly for this snapshot
            
            // Note: We don't necessarily NEED to save MonthlyExpense records if the system reads from Transactions.
            // But saving them 'freezes' the values for history, which is good.
            
            var transactions = await _transactionService.GetByMonthAsync(familyId, year, month);
            
            // Group by category
            var expenses = transactions
                .Where(t => t.Type == TransactionType.Expense && t.CategoryId.HasValue)
                .GroupBy(t => t.CategoryId!.Value)
                .Select(g => (
                    CategoryId: g.Key, 
                    Amount: g.Sum(t => t.Amount), 
                    Notes: (string?)"Auto-generated from transactions"
                ))
                .ToList();

            var incomes = transactions
                .Where(t => t.Type == TransactionType.Income && t.CategoryId.HasValue)
                .GroupBy(t => t.CategoryId!.Value)
                .Select(g => (
                    CategoryId: g.Key, 
                    Amount: g.Sum(t => t.Amount), 
                    Notes: (string?)"Auto-generated from transactions"
                ))
                .ToList();

            if (expenses.Any())
                await _snapshotService.SaveExpensesAsync(snapshotId, expenses);
                
            if (incomes.Any())
                await _snapshotService.SaveIncomesAsync(snapshotId, incomes);

            return ServiceResult<int>.Ok(snapshotId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing month {Year}-{Month}", year, month);
            return ServiceResult<int>.Fail("Errore durante la chiusura del mese");
        }
    }

    public Task DismissPromptAsync(int familyId, int year, int month)
    {
        // ToDo: Implement logic to remember dismissal (e.g. in LocalStorage or UserSettings DB)
        return Task.CompletedTask;
    }
}
