namespace FamilyFinance.Models.Import;

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
    
    // UI State
    public bool IsSelected { get; set; } = true;
    public bool IsDuplicate { get; set; } = false;
    public bool IsLearnedRule { get; set; } = false; // True if categorized by learned rule
}

public class CsvColumnMapping
{
    public int DateIndex { get; set; } = -1;
    public int DescriptionIndex { get; set; } = -1;
    public int AmountIndex { get; set; } = -1;
    public int CategoryIndex { get; set; } = -1; // Optional
    public bool HasHeaderRow { get; set; } = true;
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string DecimalSeparator { get; set; } = ",";
}
