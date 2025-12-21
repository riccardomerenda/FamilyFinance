namespace FamilyFinance.Models;

public class SnapshotLine
{
    public int Id { get; set; }
    public int SnapshotId { get; set; }
    public Snapshot Snapshot { get; set; } = default!;
    public int AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public decimal Amount { get; set; } // Valore corrente
    public decimal ContributionBasis { get; set; } // Contributi versati (per Pensione/Assicurazione)
    
    // Calculated properties (for Pension/Insurance)
    public decimal GainLoss => Amount - ContributionBasis;
    public decimal GainLossPercent => ContributionBasis > 0 ? (GainLoss / ContributionBasis) * 100 : 0;
}

