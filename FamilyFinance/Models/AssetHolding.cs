namespace FamilyFinance.Models;

/// <summary>
/// Represents a live investment holding in a portfolio.
/// Unlike InvestmentAsset (which is snapshot-bound), this entity exists continuously
/// and is updated on each import.
/// </summary>
public class AssetHolding : IFamilyOwned
{
    public int Id { get; set; }
    
    // Family ownership (IFamilyOwned)
    public int FamilyId { get; set; }
    public Family? Family { get; set; }
    
    // Portfolio relationship
    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = default!;
    
    // Asset identifiers
    public string Ticker { get; set; } = string.Empty;
    public string? Name { get; set; }      // Full name (e.g., "VANGUARD FTSE ALL-WORLD")
    public string? ISIN { get; set; }
    public string Broker { get; set; } = "Directa";
    
    // Quantities and prices (per-unit values)
    public decimal Quantity { get; set; }
    public decimal AverageCostBasis { get; set; }  // PMC per unit
    public decimal CurrentPrice { get; set; }
    
    // Timestamps
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    // Computed Properties
    public decimal TotalCostBasis => Quantity * AverageCostBasis;
    public decimal MarketValue => Quantity * CurrentPrice;
    public decimal GainLoss => MarketValue - TotalCostBasis;
    public decimal GainLossPercent => TotalCostBasis > 0 ? (GainLoss / TotalCostBasis) * 100 : 0;
}
