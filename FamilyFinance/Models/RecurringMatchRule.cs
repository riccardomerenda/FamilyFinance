namespace FamilyFinance.Models;

/// <summary>
/// Represents a learned association between transaction descriptions and recurring transactions.
/// When a user manually links a transaction to a recurring, the system extracts
/// keywords and saves them to suggest the same recurring in future imports.
/// </summary>
public class RecurringMatchRule : BaseEntity
{
    public int FamilyId { get; set; }
    public int RecurringTransactionId { get; set; }
    
    /// <summary>
    /// Extracted keyword from transaction description (lowercase).
    /// Examples: "lucia merenda", "asilo", "garage"
    /// </summary>
    public string Keyword { get; set; } = "";
    
    /// <summary>
    /// Number of times this rule has been used.
    /// Higher count = higher confidence.
    /// </summary>
    public int UsageCount { get; set; } = 1;
    
    // Navigation
    public RecurringTransaction? RecurringTransaction { get; set; }
}
