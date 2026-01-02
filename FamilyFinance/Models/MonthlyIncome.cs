using System.ComponentModel.DataAnnotations;

namespace FamilyFinance.Models;

public class MonthlyIncome
{
    public int Id { get; set; }

    // Relationship with Snapshot
    // Relationship with Snapshot
    public int SnapshotId { get; set; }
    public Snapshot? Snapshot { get; set; }

    // Linked Category (Unified)
    public int CategoryId { get; set; }
    public BudgetCategory? Category { get; set; }

    // Data
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
