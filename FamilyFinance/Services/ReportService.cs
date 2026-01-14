using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamilyFinance.Services;

/// <summary>
/// Service for generating financial reports and exporting them to PDF/Excel
/// </summary>
public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    private readonly ISnapshotService _snapshotService;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<ReportService> _logger;
    
    public ReportService(
        AppDbContext db, 
        ISnapshotService snapshotService,
        ITransactionService transactionService,
        ILogger<ReportService> logger)
    {
        _db = db;
        _snapshotService = snapshotService;
        _transactionService = transactionService;
        _logger = logger;
    }
    
    public async Task<MonthlyReportData> GenerateMonthlyReportAsync(int familyId, int year, int month)
    {
        var family = await _db.Families.FindAsync(familyId);
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        
        // Get snapshot for this month
        var snapshot = await _db.Snapshots
            .Include(s => s.Lines).ThenInclude(l => l.Account)
            .Include(s => s.Expenses).ThenInclude(e => e.Category)
            .Include(s => s.Incomes).ThenInclude(i => i.Category)
            .Where(s => s.FamilyId == familyId && 
                        s.SnapshotDate.Year == year && 
                        s.SnapshotDate.Month == month &&
                        !s.IsDeleted)
            .FirstOrDefaultAsync();
        
        // Get previous month snapshot for comparison
        var prevMonth = startDate.AddMonths(-1);
        var prevSnapshot = await _db.Snapshots
            .Include(s => s.Lines).ThenInclude(l => l.Account)
            .Where(s => s.FamilyId == familyId && 
                        s.SnapshotDate.Year == prevMonth.Year && 
                        s.SnapshotDate.Month == prevMonth.Month &&
                        !s.IsDeleted)
            .FirstOrDefaultAsync();
        
        // Calculate expenses by category
        var expensesByCategory = await _transactionService.GetSummaryByCategoryAsync(
            familyId, startDate, endDate, TransactionType.Expense);
        
        // If no transactions, fall back to MonthlyExpense data
        if (!expensesByCategory.Any() && snapshot != null)
        {
            expensesByCategory = snapshot.Expenses
                .Where(e => e.Category != null)
                .Select(e => new ReportCategorySummary
                {
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category!.Name,
                    CategoryIcon = e.Category.Icon,
                    CategoryColor = e.Category.Color,
                    Amount = e.Amount,
                    Budget = e.Category.MonthlyBudget
                })
                .ToList();
            
            var total = expensesByCategory.Sum(c => c.Amount);
            foreach (var c in expensesByCategory)
                c.Percentage = total > 0 ? (c.Amount / total) * 100 : 0;
        }
        
        // Calculate income by category
        var incomeByCategory = await _transactionService.GetSummaryByCategoryAsync(
            familyId, startDate, endDate, TransactionType.Income);
        
        if (!incomeByCategory.Any() && snapshot != null)
        {
            incomeByCategory = snapshot.Incomes
                .Where(i => i.Category != null)
                .Select(i => new ReportCategorySummary
                {
                    CategoryId = i.CategoryId,
                    CategoryName = i.Category!.Name,
                    CategoryIcon = i.Category.Icon,
                    CategoryColor = i.Category.Color,
                    Amount = i.Amount
                })
                .ToList();
        }
        
        // Calculate totals
        var totalExpenses = expensesByCategory.Sum(c => c.Amount);
        var totalIncome = incomeByCategory.Sum(c => c.Amount);
        
        // If still no income data, try to get from snapshot
        if (totalIncome == 0 && snapshot != null)
        {
            totalIncome = snapshot.Incomes.Sum(i => i.Amount);
        }
        
        // Calculate net worth from snapshot
        decimal netWorth = 0;
        decimal prevNetWorth = 0;
        var accountBalances = new List<AccountSummary>();
        
        if (snapshot != null)
        {
            netWorth = snapshot.Lines.Sum(l => l.Amount);
            
            foreach (var line in snapshot.Lines.Where(l => l.Account != null))
            {
                var prevBalance = prevSnapshot?.Lines
                    .FirstOrDefault(l => l.AccountId == line.AccountId)?.Amount ?? 0;
                
                accountBalances.Add(new AccountSummary
                {
                    AccountId = line.AccountId,
                    AccountName = line.Account!.Name,
                    Category = line.Account.Category,
                    Balance = line.Amount,
                    PreviousBalance = prevBalance
                });
            }
        }
        
        if (prevSnapshot != null)
        {
            prevNetWorth = prevSnapshot.Lines.Sum(l => l.Amount);
        }
        
        return new MonthlyReportData
        {
            Period = startDate,
            FamilyName = family?.Name ?? "Family",
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetWorth = netWorth,
            NetWorthChange = netWorth - prevNetWorth,
            ExpensesByCategory = expensesByCategory.OrderByDescending(c => c.Amount).ToList(),
            IncomeByCategory = incomeByCategory.OrderByDescending(c => c.Amount).ToList(),
            AccountBalances = accountBalances.OrderBy(a => a.Category).ThenBy(a => a.AccountName).ToList()
        };
    }
    
    public async Task<YearlyReportData> GenerateYearlyReportAsync(int familyId, int year)
    {
        var family = await _db.Families.FindAsync(familyId);
        var months = new List<MonthlyReportData>();
        
        for (int month = 1; month <= 12; month++)
        {
            var monthData = await GenerateMonthlyReportAsync(familyId, year, month);
            if (monthData.TotalExpenses > 0 || monthData.TotalIncome > 0 || monthData.NetWorth > 0)
            {
                months.Add(monthData);
            }
        }
        
        // Get first and last snapshot of the year for net worth change
        var startDate = new DateOnly(year, 1, 1);
        var endDate = new DateOnly(year, 12, 31);
        
        var firstSnapshot = await _db.Snapshots
            .Include(s => s.Lines)
            .Where(s => s.FamilyId == familyId && 
                        s.SnapshotDate >= startDate && 
                        s.SnapshotDate <= endDate &&
                        !s.IsDeleted)
            .OrderBy(s => s.SnapshotDate)
            .FirstOrDefaultAsync();
        
        var lastSnapshot = await _db.Snapshots
            .Include(s => s.Lines)
            .Where(s => s.FamilyId == familyId && 
                        s.SnapshotDate >= startDate && 
                        s.SnapshotDate <= endDate &&
                        !s.IsDeleted)
            .OrderByDescending(s => s.SnapshotDate)
            .FirstOrDefaultAsync();
        
        // Aggregate expenses by category across the year
        var expensesByCategory = months
            .SelectMany(m => m.ExpensesByCategory)
            .GroupBy(c => new { c.CategoryId, c.CategoryName, c.CategoryIcon, c.CategoryColor })
            .Select(g => new ReportCategorySummary
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                CategoryIcon = g.Key.CategoryIcon,
                CategoryColor = g.Key.CategoryColor,
                Amount = g.Sum(c => c.Amount),
                TransactionCount = g.Sum(c => c.TransactionCount)
            })
            .ToList();
        
        var totalExpenses = expensesByCategory.Sum(c => c.Amount);
        foreach (var c in expensesByCategory)
            c.Percentage = totalExpenses > 0 ? (c.Amount / totalExpenses) * 100 : 0;
        
        // Aggregate income by category
        var incomeByCategory = months
            .SelectMany(m => m.IncomeByCategory)
            .GroupBy(c => new { c.CategoryId, c.CategoryName, c.CategoryIcon, c.CategoryColor })
            .Select(g => new ReportCategorySummary
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                CategoryIcon = g.Key.CategoryIcon,
                CategoryColor = g.Key.CategoryColor,
                Amount = g.Sum(c => c.Amount),
                TransactionCount = g.Sum(c => c.TransactionCount)
            })
            .ToList();
        
        return new YearlyReportData
        {
            Year = year,
            FamilyName = family?.Name ?? "Family",
            Months = months,
            TotalIncome = months.Sum(m => m.TotalIncome),
            TotalExpenses = months.Sum(m => m.TotalExpenses),
            NetWorthStart = firstSnapshot?.Lines.Sum(l => l.Amount) ?? 0,
            NetWorthEnd = lastSnapshot?.Lines.Sum(l => l.Amount) ?? 0,
            ExpensesByCategory = expensesByCategory.OrderByDescending(c => c.Amount).ToList(),
            IncomeByCategory = incomeByCategory.OrderByDescending(c => c.Amount).ToList()
        };
    }
    
    public async Task<byte[]> ExportToPdfAsync<T>(T reportData, ReportType type) where T : class
    {
        // QuestPDF implementation will be added when package is installed
        // For now, return empty byte array
        _logger.LogInformation("PDF export requested for {ReportType}", type);
        
        // TODO: Implement with QuestPDF
        // QuestPDF requires a license community registration
        // For now we return a placeholder
        await Task.CompletedTask;
        return Array.Empty<byte>();
    }
    
    public async Task<byte[]> ExportToExcelAsync<T>(T reportData, ReportType type) where T : class
    {
        // ClosedXML implementation will be added when package is installed
        // For now, return empty byte array
        _logger.LogInformation("Excel export requested for {ReportType}", type);
        
        // TODO: Implement with ClosedXML
        await Task.CompletedTask;
        return Array.Empty<byte>();
    }
}
