using FamilyFinance.Models;

namespace FamilyFinance.Services.Interfaces;

/// <summary>
/// Service for managing live pension/insurance holdings.
/// </summary>
public interface IPensionHoldingService
{
    /// <summary>
    /// Gets all pension/insurance holdings for a family.
    /// </summary>
    Task<List<PensionHolding>> GetAllAsync(int familyId);
    
    /// <summary>
    /// Gets a specific holding by account ID.
    /// </summary>
    Task<PensionHolding?> GetByAccountAsync(int accountId);
    
    /// <summary>
    /// Adds a contribution to a pension holding (increases ContributionBasis).
    /// </summary>
    Task<ServiceResult> AddContributionAsync(int accountId, decimal amount);
    
    /// <summary>
    /// Updates the current value of a pension holding.
    /// </summary>
    Task<ServiceResult> UpdateValueAsync(int accountId, decimal newValue);
    
    /// <summary>
    /// Seeds holdings from the latest snapshot (for initial migration).
    /// </summary>
    Task<int> SeedFromLatestSnapshotAsync(int familyId);
    
    /// <summary>
    /// Ensures a PensionHolding exists for the given account, creating one if needed.
    /// </summary>
    Task<PensionHolding> EnsureHoldingExistsAsync(int familyId, int accountId);
}
