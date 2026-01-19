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
    
    /// <summary>
    /// Annualized return percentage based on account creation date.
    /// More accurate for products with periodic contributions (PAC, pension funds).
    /// </summary>
    public decimal AnnualizedReturnPercent
    {
        get
        {
            if (ContributionBasis <= 0 || Account == null)
                return 0;
                
            // Calculate months since account creation
            var monthsActive = ((DateTime.UtcNow.Year - Account.CreatedAt.Year) * 12) 
                             + (DateTime.UtcNow.Month - Account.CreatedAt.Month);
            
            // Minimum 1 month to avoid division by zero
            if (monthsActive < 1)
                monthsActive = 1;
            
            // Simple annualized return: (TotalReturn / MonthsActive) * 12
            var totalReturnPercent = GainLossPercent;
            return (totalReturnPercent / monthsActive) * 12;
        }
    }
}
