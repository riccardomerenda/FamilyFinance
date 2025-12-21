namespace FamilyFinance.Models;

public class Goal
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Target { get; set; }
    public decimal AllocatedAmount { get; set; } // Importo allocato manualmente
    public string Deadline { get; set; } = ""; 
    public GoalPriority Priority { get; set; } = GoalPriority.Medium;
    public GoalCategory Category { get; set; } = GoalCategory.Liquidity;
    public bool ShowMonthlyTarget { get; set; } = true; // Mostra proiezione mensile
    
    // Family ownership
    public int FamilyId { get; set; }
    public Family? Family { get; set; }
    
    // Calculated properties
    public decimal Missing => Math.Max(0, Target - AllocatedAmount);
    public decimal ProgressPercent => Target > 0 ? Math.Min(100, (AllocatedAmount / Target) * 100) : 0;
    public bool IsCompleted => AllocatedAmount >= Target;
}

