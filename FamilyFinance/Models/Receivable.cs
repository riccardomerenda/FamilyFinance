namespace FamilyFinance.Models;

public class Receivable
{
    public int Id { get; set; }
    public int SnapshotId { get; set; }
    public Snapshot Snapshot { get; set; } = default!;
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public DateOnly? ExpectedDate { get; set; }
    public ReceivableStatus Status { get; set; } = ReceivableStatus.Open;
}

