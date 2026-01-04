using FamilyFinance.Models;

namespace FamilyFinance.Services;

/// <summary>
/// Scoped service to manage wizard state across components
/// </summary>
public class WizardStateService
{
    public bool IsOpen { get; private set; }
    public bool IsEditing { get; private set; }
    public int? EditingSnapshotId { get; private set; }
    public int CurrentStep { get; private set; }
    public DateTime SnapshotDate { get; set; } = DateTime.Today;
    
    // Step 1: Liquidity data
    public List<AccountRow> AccountRows { get; set; } = new();
    
    // Step 2: Investment data
    public List<InvestmentRow> InvestmentRows { get; set; } = new();
    
    // Step 3: Expense data
    public List<ExpenseRow> ExpenseRows { get; set; } = new();
    
    // Step 4: Credits data
    public List<CreditRow> CreditRows { get; set; } = new();
    
    public event Action? OnChange;
    
    public void OpenNew(DateTime? suggestedDate = null)
    {
        Reset();
        IsOpen = true;
        IsEditing = false;
        EditingSnapshotId = null;
        CurrentStep = 0;
        SnapshotDate = suggestedDate ?? DateTime.Today;
        NotifyStateChanged();
    }
    
    public void OpenEdit(int snapshotId)
    {
        Reset();
        IsOpen = true;
        IsEditing = true;
        EditingSnapshotId = snapshotId;
        CurrentStep = 0;
        NotifyStateChanged();
    }
    
    public void Close()
    {
        IsOpen = false;
        NotifyStateChanged();
    }
    
    public void NextStep()
    {
        if (CurrentStep < 3)
        {
            CurrentStep++;
            NotifyStateChanged();
        }
    }
    
    public void PreviousStep()
    {
        if (CurrentStep > 0)
        {
            CurrentStep--;
            NotifyStateChanged();
        }
    }
    
    public void GoToStep(int step)
    {
        if (step >= 0 && step <= 3)
        {
            CurrentStep = step;
            NotifyStateChanged();
        }
    }
    
    public void Reset()
    {
        CurrentStep = 0;
        SnapshotDate = DateTime.Today;
        AccountRows.Clear();
        InvestmentRows.Clear();
        ExpenseRows.Clear();
        CreditRows.Clear();
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
    
    // Row classes for wizard data
    public class AccountRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsInterest { get; set; }
        public AccountCategory Category { get; set; }
        public decimal Amount { get; set; }
        public decimal ContributionBasis { get; set; }
    }
    
    public class InvestmentRow
    {
        public string Name { get; set; } = "";
        public decimal CostBasis { get; set; }
        public decimal Value { get; set; }
        public int? PortfolioId { get; set; }
    }
    
    public class ExpenseRow
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Color { get; set; } = "";
        public decimal Budget { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
    
    public class CreditRow
    {
        public string Description { get; set; } = "";
        public decimal Amount { get; set; }
        public ReceivableStatus Status { get; set; } = ReceivableStatus.Open;
    }
}
