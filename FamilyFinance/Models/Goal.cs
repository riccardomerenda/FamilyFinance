using System.Text.Json.Serialization;

namespace FamilyFinance.Models;

public class Goal : IFamilyOwned
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Target { get; set; }
    public decimal AllocatedAmount { get; set; }
    public DateOnly? Deadline { get; set; }
    public GoalPriority Priority { get; set; } = GoalPriority.Medium;
    public GoalCategory Category { get; set; } = GoalCategory.Liquidity;
    public bool ShowMonthlyTarget { get; set; } = true;
    
    // Family ownership (IFamilyOwned)
    public int FamilyId { get; set; }
    [JsonIgnore]
    public Family? Family { get; set; }
    
    // === Audit Trail ===
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // === Soft Delete ===
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    
    // === Calculated Properties ===
    public decimal Missing => Math.Max(0, Target - AllocatedAmount);
    public decimal ProgressPercent => Target > 0 ? Math.Min(100, (AllocatedAmount / Target) * 100) : 0;
    public bool IsCompleted => AllocatedAmount >= Target;
    public string DeadlineDisplay => Deadline?.ToString("yyyy-MM") ?? "";
    
    public int MonthsUntilDeadline
    {
        get
        {
            if (!Deadline.HasValue) return 0;
            var target = new DateTime(Deadline.Value.Year, Deadline.Value.Month, 1);
            var now = DateTime.Today;
            var months = ((target.Year - now.Year) * 12) + target.Month - now.Month;
            return Math.Max(0, months);
        }
    }
}

