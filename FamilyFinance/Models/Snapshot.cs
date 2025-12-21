namespace FamilyFinance.Models;

public class Snapshot
{
    public int Id { get; set; }
    public DateOnly SnapshotDate { get; set; }
    public string? Notes { get; set; }
    public string? CreatedBy { get; set; }
    public List<SnapshotLine> Lines { get; set; } = new();
    public List<InvestmentAsset> Investments { get; set; } = new();
    public List<Receivable> Receivables { get; set; } = new();
    public List<MonthlyExpense> Expenses { get; set; } = new();
    
    // Family ownership
    public int FamilyId { get; set; }
    public Family? Family { get; set; }
}

