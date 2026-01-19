using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Models.Import;
using FamilyFinance.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamilyFinance.Services;

/// <summary>
/// Service for managing live asset holdings.
/// </summary>
public class AssetHoldingService : IAssetHoldingService
{
    private readonly AppDbContext _db;
    private readonly ISnapshotService _snapshotService;
    private readonly ILogger<AssetHoldingService> _logger;

    public AssetHoldingService(
        AppDbContext db,
        ISnapshotService snapshotService,
        ILogger<AssetHoldingService> logger)
    {
        _db = db;
        _snapshotService = snapshotService;
        _logger = logger;
    }

    public async Task<List<AssetHolding>> GetAllAsync(int familyId)
    {
        return await _db.AssetHoldings
            .Include(a => a.Portfolio)
            .Where(a => a.FamilyId == familyId && a.Quantity > 0)
            .OrderBy(a => a.Portfolio.Name)
            .ThenBy(a => a.Ticker)
            .ToListAsync();
    }

    public async Task<List<AssetHolding>> GetByPortfolioAsync(int portfolioId)
    {
        return await _db.AssetHoldings
            .Include(a => a.Portfolio)
            .Where(a => a.PortfolioId == portfolioId && a.Quantity > 0)
            .OrderBy(a => a.Ticker)
            .ToListAsync();
    }

    public async Task<AssetHolding?> GetByTickerAsync(int familyId, string ticker)
    {
        return await _db.AssetHoldings
            .Include(a => a.Portfolio)
            .FirstOrDefaultAsync(a => a.FamilyId == familyId && a.Ticker == ticker);
    }

    public async Task<int> UpdateFromImportAsync(int familyId, List<DirectaAssetRow> importedAssets, 
                                                  Dictionary<string, int> portfolioAssignments,
                                                  DateTime? extractionDate = null)
    {
        _logger.LogInformation("Updating holdings for family {FamilyId} with {Count} imported assets (extraction: {Date})", 
            familyId, importedAssets.Count, extractionDate?.ToString("g") ?? "now");

        // Use extraction date from CSV if provided, otherwise use current time
        var updateTimestamp = extractionDate ?? DateTime.UtcNow;

        var existingHoldings = await _db.AssetHoldings
            .Where(a => a.FamilyId == familyId)
            .ToListAsync();

        var updatedCount = 0;

        foreach (var asset in importedAssets)
        {
            if (string.IsNullOrEmpty(asset.Ticker)) continue;

            // Find portfolio assignment
            if (!portfolioAssignments.TryGetValue(asset.Ticker, out var portfolioId))
            {
                _logger.LogWarning("No portfolio assignment for ticker {Ticker}, skipping", asset.Ticker);
                continue;
            }

            // Find existing holding by ticker
            var existing = existingHoldings.FirstOrDefault(h => 
                h.Ticker.Equals(asset.Ticker, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                // Update existing holding
                existing.Quantity = asset.Quantity;
                existing.AverageCostBasis = asset.Quantity > 0 ? asset.CostBasis / asset.Quantity : 0;
                existing.CurrentPrice = asset.Quantity > 0 ? asset.CurrentValue / asset.Quantity : 0;
                existing.PortfolioId = portfolioId;
                existing.Name = asset.FullName;
                existing.ISIN = asset.ISIN;
                existing.LastUpdated = updateTimestamp;
                
                _logger.LogDebug("Updated holding {Ticker}: Qty={Qty}, Cost={Cost}, Value={Value}", 
                    asset.Ticker, asset.Quantity, asset.CostBasis, asset.CurrentValue);
            }
            else
            {
                // Create new holding
                var newHolding = new AssetHolding
                {
                    FamilyId = familyId,
                    PortfolioId = portfolioId,
                    Ticker = asset.Ticker,
                    Name = asset.FullName,
                    ISIN = asset.ISIN,
                    Broker = "Directa",
                    Quantity = asset.Quantity,
                    AverageCostBasis = asset.Quantity > 0 ? asset.CostBasis / asset.Quantity : 0,
                    CurrentPrice = asset.Quantity > 0 ? asset.CurrentValue / asset.Quantity : 0,
                    LastUpdated = updateTimestamp
                };

                _db.AssetHoldings.Add(newHolding);
                _logger.LogDebug("Created new holding {Ticker}: Qty={Qty}, Cost={Cost}, Value={Value}", 
                    asset.Ticker, asset.Quantity, asset.CostBasis, asset.CurrentValue);
            }

            updatedCount++;
        }

        // Mark any existing holdings not in the import as potentially sold (set qty to 0)
        var importedTickers = importedAssets.Select(a => a.Ticker.ToUpperInvariant()).ToHashSet();
        foreach (var holding in existingHoldings)
        {
            if (!importedTickers.Contains(holding.Ticker.ToUpperInvariant()) && holding.Quantity > 0)
            {
                // This asset was not in the import - might be sold
                // For now, we'll keep it but set quantity to 0
                holding.Quantity = 0;
                holding.LastUpdated = updateTimestamp;
                _logger.LogInformation("Asset {Ticker} not in import, setting quantity to 0", holding.Ticker);
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Successfully updated {Count} holdings for family {FamilyId}", 
            updatedCount, familyId);

        return updatedCount;
    }

    public async Task<int> SeedFromLatestSnapshotAsync(int familyId)
    {
        _logger.LogInformation("Seeding holdings from latest snapshot for family {FamilyId}", familyId);

        // Check if holdings already exist
        var existingCount = await _db.AssetHoldings.CountAsync(h => h.FamilyId == familyId);
        if (existingCount > 0)
        {
            _logger.LogInformation("Family {FamilyId} already has {Count} holdings, skipping seed", 
                familyId, existingCount);
            return 0;
        }

        // Get latest snapshot with investments
        var latest = await _snapshotService.GetLatestAsync(familyId);
        if (latest == null)
        {
            _logger.LogInformation("No snapshots found for family {FamilyId}", familyId);
            return 0;
        }

        // Get the full snapshot with investments
        var fullSnapshot = await _snapshotService.GetByIdAsync(latest.Id);
        if (fullSnapshot?.Investments == null || !fullSnapshot.Investments.Any())
        {
            _logger.LogInformation("Latest snapshot has no investments for family {FamilyId}", familyId);
            return 0;
        }

        // Get default portfolio (first one, or create one)
        var defaultPortfolio = await _db.Portfolios
            .Where(p => p.FamilyId == familyId && !p.IsDeleted)
            .FirstOrDefaultAsync();

        if (defaultPortfolio == null)
        {
            _logger.LogWarning("No portfolio found for family {FamilyId}, cannot seed holdings", familyId);
            return 0;
        }

        var seededCount = 0;
        foreach (var inv in fullSnapshot.Investments)
        {
            var holding = new AssetHolding
            {
                FamilyId = familyId,
                PortfolioId = inv.PortfolioId ?? defaultPortfolio.Id,
                Ticker = inv.Name, // In the old model, Name was used as ticker
                Name = inv.Name,
                Broker = inv.Broker,
                // Old model stores totals, we need to estimate per-unit values
                // Since we don't have quantity, we'll store as-is with qty=1
                Quantity = 1,
                AverageCostBasis = inv.CostBasis,
                CurrentPrice = inv.Value,
                LastUpdated = DateTime.UtcNow
            };

            _db.AssetHoldings.Add(holding);
            seededCount++;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} holdings from snapshot for family {FamilyId}", 
            seededCount, familyId);

        return seededCount;
    }

    public async Task<List<PortfolioHoldingSummary>> GetSummaryByPortfolioAsync(int familyId)
    {
        var holdings = await GetAllAsync(familyId);

        return holdings
            .GroupBy(h => h.PortfolioId)
            .Select(g => new PortfolioHoldingSummary
            {
                PortfolioId = g.Key,
                PortfolioName = g.First().Portfolio?.Name ?? "Unknown",
                PortfolioColor = g.First().Portfolio?.Color ?? "#6366f1",
                TotalCostBasis = g.Sum(h => h.TotalCostBasis),
                TotalMarketValue = g.Sum(h => h.MarketValue),
                HoldingCount = g.Count(),
                Holdings = g.ToList()
            })
            .OrderBy(p => p.PortfolioName)
            .ToList();
    }
    
    public async Task<ServiceResult> AddContributionAsync(int assetHoldingId, decimal amount)
    {
        try
        {
            var holding = await _db.AssetHoldings.FindAsync(assetHoldingId);
            if (holding == null)
                return ServiceResult.Fail("Asset holding not found");
            
            if (amount <= 0)
                return ServiceResult.Fail("Contribution amount must be positive");
            
            // For PAC-style contributions, we increase the total cost basis
            // without changing quantity (we don't know price at contribution time)
            // This effectively increases AverageCostBasis proportionally
            var currentTotalCost = holding.TotalCostBasis;
            var newTotalCost = currentTotalCost + amount;
            
            // If we have quantity, recalculate average cost
            if (holding.Quantity > 0)
            {
                holding.AverageCostBasis = newTotalCost / holding.Quantity;
            }
            else
            {
                // No quantity yet, just set the cost basis directly
                holding.AverageCostBasis = amount;
                holding.Quantity = 1; // Placeholder until real import
            }
            
            holding.LastUpdated = DateTime.UtcNow;
            
            await _db.SaveChangesAsync();
            _logger.LogInformation("Added contribution of {Amount} to asset holding {Id} ({Ticker})", 
                amount, assetHoldingId, holding.Ticker);
            
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding contribution to asset holding {Id}", assetHoldingId);
            return ServiceResult.Fail($"Error adding contribution: {ex.Message}");
        }
    }
}
