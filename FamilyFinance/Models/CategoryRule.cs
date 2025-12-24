namespace FamilyFinance.Models;

/// <summary>
/// Represents a learned categorization rule from user behavior.
/// When a user manually categorizes a transaction, the system extracts
/// keywords and saves them to suggest the same category in future imports.
/// </summary>
public class CategoryRule : BaseEntity
{
    public int FamilyId { get; set; }
    public int CategoryId { get; set; }
    
    /// <summary>
    /// Extracted keyword from transaction description (lowercase).
    /// Examples: "amazon", "netflix", "esselunga"
    /// </summary>
    public string Keyword { get; set; } = "";
    
    /// <summary>
    /// Number of times this rule has been matched/used.
    /// Higher count = higher confidence in the rule.
    /// </summary>
    public int UsageCount { get; set; } = 1;
    
    // Navigation
    public BudgetCategory? Category { get; set; }
}
