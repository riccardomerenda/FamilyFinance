namespace FamilyFinance.Models;

/// <summary>
/// Categoria di spesa con budget mensile
/// </summary>
public class BudgetCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "💰"; // Emoji icon
    public string Color { get; set; } = "#6366f1"; // Hex color
    public decimal MonthlyBudget { get; set; } // Limite mensile
    public int SortOrder { get; set; } // Per ordinamento personalizzato
    public bool IsActive { get; set; } = true;
    
    // Family relationship
    public int FamilyId { get; set; }
    public Family? Family { get; set; }
    
    // Navigation
    public List<MonthlyExpense> Expenses { get; set; } = new();
}

