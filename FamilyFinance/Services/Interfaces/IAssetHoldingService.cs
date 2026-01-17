using FamilyFinance.Models;
using FamilyFinance.Models.Import;

namespace FamilyFinance.Services.Interfaces;

/// <summary>
/// Service for managing live asset holdings.
/// </summary>
public interface IAssetHoldingService
{
    /// <summary>
    /// Gets all asset holdings for a family.
    /// </summary>
    Task<List<AssetHolding>> GetAllAsync(int familyId);
    
    /// <summary>
    /// Gets all asset holdings for a specific portfolio.
    /// </summary>
    Task<List<AssetHolding>> GetByPortfolioAsync(int portfolioId);
    
    /// <summary>
    /// Gets a specific holding by ticker within a family.
    /// </summary>
    Task<AssetHolding?> GetByTickerAsync(int familyId, string ticker);
    
    /// <summary>
    /// Updates holdings from a Directa import.
    /// Creates new holdings or updates existing ones based on ticker match.
    /// </summary>
    Task<int> UpdateFromImportAsync(int familyId, List<DirectaAssetRow> importedAssets, 
                                    Dictionary<string, int> portfolioAssignments);
    
    /// <summary>
    /// Seeds holdings from the latest snapshot (for initial migration).
    /// </summary>
    Task<int> SeedFromLatestSnapshotAsync(int familyId);
    
    /// <summary>
    /// Gets a summary of holdings grouped by portfolio.
    /// </summary>
    Task<List<PortfolioHoldingSummary>> GetSummaryByPortfolioAsync(int familyId);
    
    /// <summary>
    /// Adds a contribution to an asset holding (for PAC-style recurring investments).
    /// Increases the cost basis by the contribution amount.
    /// </summary>
    Task<ServiceResult> AddContributionAsync(int assetHoldingId, decimal amount);
}

/// <summary>
/// Summary of holdings for a single portfolio.
/// </summary>
public class PortfolioHoldingSummary
{
    public int PortfolioId { get; set; }
    public string PortfolioName { get; set; } = "";
    public string PortfolioColor { get; set; } = "#6366f1";
    public decimal TotalCostBasis { get; set; }
    public decimal TotalMarketValue { get; set; }
    public decimal GainLoss => TotalMarketValue - TotalCostBasis;
    public decimal GainLossPercent => TotalCostBasis > 0 ? (GainLoss / TotalCostBasis) * 100 : 0;
    public int HoldingCount { get; set; }
    public List<AssetHolding> Holdings { get; set; } = new();
}
