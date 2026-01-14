namespace FamilyFinance.Models;

/// <summary>
/// Monthly report data transfer object
/// </summary>
public class MonthlyReportData
{
    public DateOnly Period { get; set; }
    public string FamilyName { get; set; } = "";
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetSavings => TotalIncome - TotalExpenses;
    public decimal SavingsRate => TotalIncome > 0 ? (NetSavings / TotalIncome) * 100 : 0;
    public decimal NetWorth { get; set; }
    public decimal NetWorthChange { get; set; }
    public List<ReportCategorySummary> ExpensesByCategory { get; set; } = new();
    public List<ReportCategorySummary> IncomeByCategory { get; set; } = new();
    public List<AccountSummary> AccountBalances { get; set; } = new();
}

/// <summary>
/// Yearly report data transfer object with monthly breakdown
/// </summary>
public class YearlyReportData
{
    public int Year { get; set; }
    public string FamilyName { get; set; } = "";
    public List<MonthlyReportData> Months { get; set; } = new();
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalSavings => TotalIncome - TotalExpenses;
    public decimal AverageMonthlySavings => Months.Count > 0 ? TotalSavings / Months.Count : 0;
    public decimal NetWorthStart { get; set; }
    public decimal NetWorthEnd { get; set; }
    public decimal NetWorthChange => NetWorthEnd - NetWorthStart;
    public List<ReportCategorySummary> ExpensesByCategory { get; set; } = new();
    public List<ReportCategorySummary> IncomeByCategory { get; set; } = new();
}

/// <summary>
/// Summary of transactions by category for reports
/// </summary>
public class ReportCategorySummary
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public string CategoryIcon { get; set; } = "💰";
    public string CategoryColor { get; set; } = "#6366f1";
    public decimal Amount { get; set; }
    public decimal Budget { get; set; }
    public decimal Percentage { get; set; }
    public int TransactionCount { get; set; }
}

/// <summary>
/// Summary of account balances
/// </summary>
public class AccountSummary
{
    public int AccountId { get; set; }
    public string AccountName { get; set; } = "";
    public AccountCategory Category { get; set; }
    public decimal Balance { get; set; }
    public decimal PreviousBalance { get; set; }
    public decimal Change => Balance - PreviousBalance;
}

/// <summary>
/// Report type enumeration
/// </summary>
public enum ReportType
{
    Monthly,
    Yearly,
    NetWorth,
    BudgetVsActual
}
