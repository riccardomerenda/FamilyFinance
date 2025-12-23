using System.ComponentModel.DataAnnotations;

namespace FamilyFinance.Models;

/// <summary>
/// Represents a batch of imported transactions.
/// Allows for "Smart Revert" functionality by tracking what was imported and where.
/// </summary>
public class ImportBatch
{
    public int Id { get; set; }

    public DateTime ImportDate { get; set; } = DateTime.UtcNow;

    [Required]
    public string FileName { get; set; } = "";
    
    // Total amount in this batch (for display)
    public decimal TotalAmount { get; set; }
    
    // Number of items imported
    public int ItemCount { get; set; }

    // JSON serialization of the imported items:
    // List<(int CategoryId, decimal Amount, string Notes)>
    // Used to reverse the operation
    public string DetailsJson { get; set; } = "[]";

    public string? CreatedBy { get; set; }
    
    // Optional: Link to specific snapshot if all went to one
    // But import might span multiple months (though UI usually forces one or auto-detects)
    // We'll trust DetailsJson for the rollback logic.
}
