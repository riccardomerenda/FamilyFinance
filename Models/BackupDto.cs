namespace FamilyFinance.Models;

/// <summary>
/// DTO for backup file deserialization
/// </summary>
public class BackupDto
{
    public DateTime ExportDate { get; set; }
    public string Version { get; set; } = "1.0";
    public List<AccountDto> Accounts { get; set; } = new();
    public List<PortfolioDto> Portfolios { get; set; } = new();
    public List<GoalDto> Goals { get; set; } = new();
    public List<SnapshotDto> Snapshots { get; set; } = new();
}

public class AccountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Owner { get; set; } = "";
    public bool IsInterest { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PortfolioDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int? TimeHorizonYears { get; set; }
    public int? TargetYear { get; set; }
    public string Color { get; set; } = "#6366f1";
    public bool IsActive { get; set; } = true;
}

public class GoalDto
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Target { get; set; }
    public decimal AllocatedAmount { get; set; }
    public string Deadline { get; set; } = "";
    public string Priority { get; set; } = "Medium";
    public string Category { get; set; } = "Liquidity";
    public bool ShowMonthlyTarget { get; set; } = true;
}

public class SnapshotDto
{
    public int Id { get; set; }
    public string Date { get; set; } = "";
    public List<SnapshotLineDto> Lines { get; set; } = new();
    public List<InvestmentDto> Investments { get; set; } = new();
    public List<ReceivableDto> Receivables { get; set; } = new();
}

public class SnapshotLineDto
{
    public int AccountId { get; set; }
    public string? AccountName { get; set; }
    public decimal Amount { get; set; }
    public decimal ContributionBasis { get; set; }
}

public class InvestmentDto
{
    public string Name { get; set; } = "";
    public decimal CostBasis { get; set; }
    public decimal Value { get; set; }
    public int? PortfolioId { get; set; }
    public string? PortfolioName { get; set; }
}

public class ReceivableDto
{
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Open";
}

/// <summary>
/// Import preview result
/// </summary>
public class ImportPreview
{
    public bool IsValid { get; set; }
    public string? Error { get; set; }
    public DateTime? ExportDate { get; set; }
    public string Version { get; set; } = "";
    
    public int TotalAccounts { get; set; }
    public int NewAccounts { get; set; }
    public int ExistingAccounts { get; set; }
    
    public int TotalPortfolios { get; set; }
    public int NewPortfolios { get; set; }
    public int ExistingPortfolios { get; set; }
    
    public int TotalGoals { get; set; }
    public int NewGoals { get; set; }
    public int ExistingGoals { get; set; }
    
    public int TotalSnapshots { get; set; }
    public int NewSnapshots { get; set; }
    public int ExistingSnapshots { get; set; }
    
    public BackupDto? Data { get; set; }
}

/// <summary>
/// Import result
/// </summary>
public class ImportResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    
    public int AccountsImported { get; set; }
    public int PortfoliosImported { get; set; }
    public int GoalsImported { get; set; }
    public int SnapshotsImported { get; set; }
}

