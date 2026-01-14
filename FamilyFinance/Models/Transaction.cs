namespace FamilyFinance.Models;

/// <summary>
/// Represents an individual financial transaction (expense or income)
/// </summary>
public class Transaction : IFamilyOwned
{
    public long Id { get; set; }
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string Description { get; set; } = "";
    public string? Notes { get; set; }
    public string? Payee { get; set; }
    
    // Relationships
    public int? CategoryId { get; set; }
    public BudgetCategory? Category { get; set; }
    
    public int? AccountId { get; set; }
    public Account? Account { get; set; }
    
    // Family ownership (IFamilyOwned)
    public int FamilyId { get; set; }
    public Family? Family { get; set; }
    
    // Import metadata
    public string? ImportSource { get; set; } // "manual", "csv", "api"
    public string? ExternalId { get; set; } // For deduplication
    public int? ImportBatchId { get; set; }
    public ImportBatch? ImportBatch { get; set; }
    
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
