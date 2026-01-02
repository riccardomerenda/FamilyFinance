namespace FamilyFinance.Models;

/// <summary>
/// Categoria di spesa con budget mensile
/// </summary>
public enum CategoryType
{
    Expense,
    Income
}

/// <summary>
/// Categoria di transazione (Spesa o Entrata)
/// </summary>
public class BudgetCategory : IFamilyOwned
{
    public int Id { get; set; }
    public CategoryType Type { get; set; } = CategoryType.Expense;
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "💰";
    public string Color { get; set; } = "#6366f1";
    public decimal MonthlyBudget { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Family ownership (IFamilyOwned)
    public int FamilyId { get; set; }
    public Family? Family { get; set; }
    
    // Navigation
    public List<MonthlyExpense> Expenses { get; set; } = new();
    
    // === Audit Trail ===
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // === Soft Delete ===
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

