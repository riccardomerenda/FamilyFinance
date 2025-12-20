namespace FamilyFinance.Models;

public class Goal
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Target { get; set; }
    public string Deadline { get; set; } = ""; 
    public GoalPriority Priority { get; set; } = GoalPriority.Medium;
    public GoalCategory Category { get; set; } = GoalCategory.Liquidity;
}

