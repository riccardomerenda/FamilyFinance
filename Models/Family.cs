namespace FamilyFinance.Models;

public class Family
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public List<AppUser> Members { get; set; } = new();
    public List<Account> Accounts { get; set; } = new();
    public List<Snapshot> Snapshots { get; set; } = new();
    public List<Goal> Goals { get; set; } = new();
    public List<Portfolio> Portfolios { get; set; } = new();
    public List<BudgetCategory> BudgetCategories { get; set; } = new();
}

