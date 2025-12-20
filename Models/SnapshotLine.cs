namespace FamilyFinance.Models;

public class SnapshotLine
{
    public int Id { get; set; }
    public int SnapshotId { get; set; }
    public Snapshot Snapshot { get; set; } = default!;
    public int AccountId { get; set; }
    public Account Account { get; set; } = default!;
    public decimal Amount { get; set; }
}

