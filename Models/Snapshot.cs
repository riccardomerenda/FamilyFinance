namespace FamilyFinance.Models;

public class Snapshot
{
    public int Id { get; set; }
    public DateOnly SnapshotDate { get; set; }
    public List<SnapshotLine> Lines { get; set; } = new();
    public List<InvestmentAsset> Investments { get; set; } = new();
    public List<Receivable> Receivables { get; set; } = new();
}

