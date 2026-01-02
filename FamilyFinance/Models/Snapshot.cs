namespace FamilyFinance.Models;

public class Snapshot : IFamilyOwned
{
    public int Id { get; set; }
    public DateOnly SnapshotDate { get; set; }
    public string? Notes { get; set; }
    
    // Collections
    public List<SnapshotLine> Lines { get; set; } = new();
    public List<InvestmentAsset> Investments { get; set; } = new();
    public List<Receivable> Receivables { get; set; } = new();
    public List<MonthlyExpense> Expenses { get; set; } = new();
    public List<MonthlyIncome> Incomes { get; set; } = new();
    
    // Family ownership (IFamilyOwned)
    public int FamilyId { get; set; }
    public Family? Family { get; set; }
    
    // === Audit Trail ===
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // === Soft Delete ===
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

