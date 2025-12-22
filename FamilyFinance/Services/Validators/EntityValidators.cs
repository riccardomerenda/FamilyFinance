using FamilyFinance.Models;

namespace FamilyFinance.Services.Validators;

/// <summary>
/// Validation methods for all entities
/// </summary>
public static class EntityValidators
{
    // === Goal Validation ===
    
    public static ServiceResult Validate(this Goal goal)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(goal.Name))
            errors.Add("Il nome dell'obiettivo è obbligatorio");
        else if (goal.Name.Length > 100)
            errors.Add("Il nome dell'obiettivo non può superare 100 caratteri");
            
        if (goal.Target <= 0)
            errors.Add("L'importo obiettivo deve essere maggiore di zero");
        else if (goal.Target > 100_000_000)
            errors.Add("L'importo obiettivo non può superare 100 milioni");
            
        if (goal.AllocatedAmount < 0)
            errors.Add("L'importo allocato non può essere negativo");
            
        if (goal.AllocatedAmount > goal.Target * 10)
            errors.Add("L'importo allocato sembra troppo alto rispetto all'obiettivo");
            
        if (goal.Deadline.HasValue && goal.Deadline.Value < DateOnly.FromDateTime(DateTime.Today.AddMonths(-12)))
            errors.Add("La scadenza non può essere nel passato remoto");
            
        if (goal.FamilyId <= 0)
            errors.Add("L'obiettivo deve essere associato a una famiglia");
        
        return errors.Count > 0 ? ServiceResult.Fail(errors) : ServiceResult.Ok();
    }
    
    // === Account Validation ===
    
    public static ServiceResult Validate(this Account account)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(account.Name))
            errors.Add("Il nome del conto è obbligatorio");
        else if (account.Name.Length > 100)
            errors.Add("Il nome del conto non può superare 100 caratteri");
            
        if (string.IsNullOrWhiteSpace(account.Owner))
            errors.Add("Il proprietario del conto è obbligatorio");
            
        if (account.FamilyId <= 0)
            errors.Add("Il conto deve essere associato a una famiglia");
        
        return errors.Count > 0 ? ServiceResult.Fail(errors) : ServiceResult.Ok();
    }
    
    // === Portfolio Validation ===
    
    public static ServiceResult Validate(this Portfolio portfolio)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(portfolio.Name))
            errors.Add("Il nome del portafoglio è obbligatorio");
        else if (portfolio.Name.Length > 100)
            errors.Add("Il nome del portafoglio non può superare 100 caratteri");
            
        if (portfolio.TimeHorizonYears.HasValue && portfolio.TimeHorizonYears < 0)
            errors.Add("L'orizzonte temporale non può essere negativo");
            
        if (portfolio.TimeHorizonYears.HasValue && portfolio.TimeHorizonYears > 50)
            errors.Add("L'orizzonte temporale non può superare 50 anni");
            
        if (portfolio.TargetYear.HasValue)
        {
            var currentYear = DateTime.Today.Year;
            if (portfolio.TargetYear < currentYear - 10)
                errors.Add("L'anno obiettivo è troppo nel passato");
            if (portfolio.TargetYear > currentYear + 60)
                errors.Add("L'anno obiettivo è troppo nel futuro");
        }
        
        if (string.IsNullOrWhiteSpace(portfolio.Color))
            portfolio.Color = "#6366f1"; // Default color
            
        if (portfolio.FamilyId <= 0)
            errors.Add("Il portafoglio deve essere associato a una famiglia");
        
        return errors.Count > 0 ? ServiceResult.Fail(errors) : ServiceResult.Ok();
    }
    
    // === BudgetCategory Validation ===
    
    public static ServiceResult Validate(this BudgetCategory category)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(category.Name))
            errors.Add("Il nome della categoria è obbligatorio");
        else if (category.Name.Length > 50)
            errors.Add("Il nome della categoria non può superare 50 caratteri");
            
        if (category.MonthlyBudget < 0)
            errors.Add("Il budget mensile non può essere negativo");
            
        if (category.MonthlyBudget > 1_000_000)
            errors.Add("Il budget mensile non può superare 1 milione");
            
        if (string.IsNullOrWhiteSpace(category.Icon))
            category.Icon = "💰"; // Default icon
            
        if (string.IsNullOrWhiteSpace(category.Color))
            category.Color = "#6366f1"; // Default color
            
        if (category.FamilyId <= 0)
            errors.Add("La categoria deve essere associata a una famiglia");
        
        return errors.Count > 0 ? ServiceResult.Fail(errors) : ServiceResult.Ok();
    }
    
    // === Snapshot Validation ===
    
    public static ServiceResult ValidateSnapshot(DateOnly date, int familyId)
    {
        var errors = new List<string>();
        
        if (date > DateOnly.FromDateTime(DateTime.Today.AddMonths(1)))
            errors.Add("La data dello snapshot non può essere troppo nel futuro");
            
        if (date < DateOnly.FromDateTime(DateTime.Today.AddYears(-20)))
            errors.Add("La data dello snapshot è troppo nel passato");
            
        if (familyId <= 0)
            errors.Add("Lo snapshot deve essere associato a una famiglia");
        
        return errors.Count > 0 ? ServiceResult.Fail(errors) : ServiceResult.Ok();
    }
    
    // === Investment Asset Validation ===
    
    public static ServiceResult Validate(this InvestmentAsset asset)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(asset.Name))
            errors.Add("Il nome dell'asset è obbligatorio");
        else if (asset.Name.Length > 100)
            errors.Add("Il nome dell'asset non può superare 100 caratteri");
            
        if (asset.Value < 0)
            errors.Add("Il valore dell'asset non può essere negativo");
            
        if (asset.CostBasis < 0)
            errors.Add("Il costo di carico non può essere negativo");
            
        if (asset.Value > 100_000_000)
            errors.Add("Il valore dell'asset sembra troppo alto");
        
        return errors.Count > 0 ? ServiceResult.Fail(errors) : ServiceResult.Ok();
    }
    
    // === Receivable Validation ===
    
    public static ServiceResult Validate(this Receivable receivable)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(receivable.Description))
            errors.Add("La descrizione del credito è obbligatoria");
        else if (receivable.Description.Length > 200)
            errors.Add("La descrizione non può superare 200 caratteri");
            
        if (receivable.Amount <= 0)
            errors.Add("L'importo del credito deve essere maggiore di zero");
            
        if (receivable.Amount > 10_000_000)
            errors.Add("L'importo del credito sembra troppo alto");
        
        return errors.Count > 0 ? ServiceResult.Fail(errors) : ServiceResult.Ok();
    }
}

