namespace FamilyFinance.Models;

public class InvestmentAsset
{
    public int Id { get; set; }
    public int SnapshotId { get; set; }
    public Snapshot Snapshot { get; set; } = default!;
    public string Broker { get; set; } = "Directa";
    public string Name { get; set; } = ""; // Es: VWCE
    public decimal Value { get; set; }
}

