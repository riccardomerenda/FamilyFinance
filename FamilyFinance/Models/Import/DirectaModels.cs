namespace FamilyFinance.Models.Import;

/// <summary>
/// Represents a single asset row parsed from Directa portfolio CSV export.
/// </summary>
public class DirectaAssetRow
{
    public string Ticker { get; set; } = "";
    public string? ISIN { get; set; }
    public string? FullName { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostBasis { get; set; }    // "Valore di carico"
    public decimal CurrentValue { get; set; } // "Valore attuale"
    public decimal Price { get; set; }        // Unit price
    
    // Computed
    public decimal GainLoss => CurrentValue - CostBasis;
    public decimal GainLossPercent => CostBasis > 0 ? (GainLoss / CostBasis) * 100 : 0;
}

/// <summary>
/// Result of parsing a Directa portfolio CSV file.
/// </summary>
public class DirectaImportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExtractionDate { get; set; }
    public string? AccountInfo { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TotalCostBasis { get; set; }
    public List<DirectaAssetRow> Assets { get; set; } = new();
}
