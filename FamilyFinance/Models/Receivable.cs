using System.Text.Json.Serialization;

namespace FamilyFinance.Models;

public class Receivable
{
    public int Id { get; set; }
    public int SnapshotId { get; set; }
    [JsonIgnore]
    public Snapshot Snapshot { get; set; } = default!;
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public DateOnly? ExpectedDate { get; set; }
    public ReceivableStatus Status { get; set; } = ReceivableStatus.Open;
}

