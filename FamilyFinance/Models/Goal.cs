namespace FamilyFinance.Models;

public class Goal
{
    public int Id { get; set; }  // Changed from long to int (auto-increment)
    public string Name { get; set; } = "";
    public decimal Target { get; set; }
    public decimal AllocatedAmount { get; set; }
    public DateOnly? Deadline { get; set; }  // Changed from string to DateOnly?
    public GoalPriority Priority { get; set; } = GoalPriority.Medium;
    public GoalCategory Category { get; set; } = GoalCategory.Liquidity;
    public bool ShowMonthlyTarget { get; set; } = true;
    
    // Family ownership
    public int FamilyId { get; set; }
    public Family? Family { get; set; }
    
    // Calculated properties
    public decimal Missing => Math.Max(0, Target - AllocatedAmount);
    public decimal ProgressPercent => Target > 0 ? Math.Min(100, (AllocatedAmount / Target) * 100) : 0;
    public bool IsCompleted => AllocatedAmount >= Target;
    
    // Helper for deadline display
    public string DeadlineDisplay => Deadline?.ToString("yyyy-MM") ?? "";
    
    // Calculate months until deadline
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

