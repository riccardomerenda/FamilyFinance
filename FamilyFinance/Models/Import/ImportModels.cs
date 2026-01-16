namespace FamilyFinance.Models.Import;

public enum TransactionMatchType { None, Recurring, Receivable }

public class ImportedTransaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); // Temporary ID for UI tracking
    public DateOnly Date { get; set; }
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public string RawCategory { get; set; } = ""; // From bank CSV if available
    
    // Categorization logic results
    public int? SuggestedCategoryId { get; set; }
    public string? SuggestedCategoryName { get; set; }
    public int ConfidenceScore { get; set; } // 0-100
    
    // === Smart Matching Results ===
    public TransactionMatchType MatchType { get; set; } = TransactionMatchType.None;
    public int MatchConfidence { get; set; } // 0-100
    
    // Recurring transaction match
    public int? MatchedRecurringId { get; set; }
    public string? MatchedRecurringName { get; set; }
    
    // Receivable match (credit collection)
    public int? MatchedReceivableId { get; set; }
    public string? MatchedReceivableDesc { get; set; }
    
    // User confirmation state
    public bool IsMatchConfirmed { get; set; } = false;
    
    // UI State
    public bool IsSelected { get; set; } = true;
    public bool IsDuplicate { get; set; } = false;
    public bool IsLearnedRule { get; set; } = false; // True if categorized by learned rule
}

public class CsvColumnMapping
{
    public int SkipRows { get; set; } = 0; // Skip N rows before header (for BBVA etc.)
    public char Delimiter { get; set; } = ','; // CSV delimiter (, or ;)
    public int DateIndex { get; set; } = -1;
    public int DescriptionIndex { get; set; } = -1;
    public int AmountIndex { get; set; } = -1;
    public int CategoryIndex { get; set; } = -1; // Optional
    public bool HasHeaderRow { get; set; } = true;
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string DecimalSeparator { get; set; } = ",";
}
