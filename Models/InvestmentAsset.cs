namespace FamilyFinance.Models;

public class InvestmentAsset
{
    public int Id { get; set; }
    public int SnapshotId { get; set; }
    public Snapshot Snapshot { get; set; } = default!;
    public string Broker { get; set; } = "Directa";
    public string Name { get; set; } = ""; // Es: VWCE
    public decimal CostBasis { get; set; } // Costo di carico (quanto investito)
    public decimal Value { get; set; } // Valore corrente di mercato
    
    // Calculated properties
    public decimal GainLoss => Value - CostBasis;
    public decimal GainLossPercent => CostBasis > 0 ? (GainLoss / CostBasis) * 100 : 0;
}

