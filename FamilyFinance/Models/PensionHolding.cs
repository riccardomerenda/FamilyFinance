namespace FamilyFinance.Models;

/// <summary>
/// Represents a live pension/insurance holding.
/// Unlike SnapshotLine (which is snapshot-bound), this entity exists continuously
/// and is updated when contributions are made or values change.
/// </summary>
public class PensionHolding : IFamilyOwned
{
    public int Id { get; set; }
    
    // Family ownership (IFamilyOwned)
    public int FamilyId { get; set; }
    public Family? Family { get; set; }
    
    // Account relationship (Pension or Insurance type)
    public int AccountId { get; set; }
    public Account Account { get; set; } = default!;
    
    // Values
    public decimal ContributionBasis { get; set; }  // Total contributions made
    public decimal CurrentValue { get; set; }       // Current market value
    
    // Timestamps
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    // Computed Properties
    public decimal GainLoss => CurrentValue - ContributionBasis;
    public decimal GainLossPercent => ContributionBasis > 0 
        ? (GainLoss / ContributionBasis) * 100 : 0;
}
