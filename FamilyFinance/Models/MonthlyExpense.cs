using System.Text.Json.Serialization;

namespace FamilyFinance.Models;

/// <summary>
/// Spesa mensile per categoria (legata a uno Snapshot)
/// </summary>
public class MonthlyExpense
{
    public int Id { get; set; }
    
    // Relazione con Snapshot
    public int SnapshotId { get; set; }
    [JsonIgnore]
    public Snapshot? Snapshot { get; set; }
    
    // Relazione con Categoria
    public int CategoryId { get; set; }
    [JsonIgnore]
    public BudgetCategory? Category { get; set; }
    
    // Dati
    public decimal Amount { get; set; } // Importo speso
    public string? Notes { get; set; } // Note opzionali
    
    // Computed properties
    public decimal BudgetAmount => Category?.MonthlyBudget ?? 0;
    public decimal Difference => BudgetAmount - Amount;
    public decimal PercentUsed => BudgetAmount > 0 ? (Amount / BudgetAmount) * 100 : 0;
    public bool IsOverBudget => Amount > BudgetAmount && BudgetAmount > 0;
}

