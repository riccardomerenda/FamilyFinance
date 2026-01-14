namespace FamilyFinance.Models;

/// <summary>
/// Represents a recurring transaction (subscription, salary, etc.)
/// </summary>
public class RecurringTransaction : IFamilyOwned
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public RecurrenceFrequency Frequency { get; set; } = RecurrenceFrequency.Monthly;
    public int? DayOfMonth { get; set; } // 1-31 for monthly recurrence
    public int? DayOfWeek { get; set; } // 0-6 for weekly recurrence
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Relationships
    public int? CategoryId { get; set; }
    public BudgetCategory? Category { get; set; }
    
    public int? AccountId { get; set; }
    public Account? Account { get; set; }
    
    // Notification settings
    public bool NotifyBeforeDue { get; set; } = false;
    public int NotifyDaysBefore { get; set; } = 3;
    
    // Family ownership (IFamilyOwned)
    public int FamilyId { get; set; }
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
    
    // === Computed Properties ===
    
    /// <summary>
    /// Calculates the next occurrence date from a given date
    /// </summary>
    public DateOnly? GetNextOccurrence(DateOnly from)
    {
        if (!IsActive || (EndDate.HasValue && from > EndDate.Value))
            return null;
            
        var effectiveFrom = from < StartDate ? StartDate : from;
        
        return Frequency switch
        {
            RecurrenceFrequency.Daily => effectiveFrom.AddDays(1),
            RecurrenceFrequency.Weekly => GetNextWeekly(effectiveFrom),
            RecurrenceFrequency.Monthly => GetNextMonthly(effectiveFrom),
            RecurrenceFrequency.Yearly => GetNextYearly(effectiveFrom),
            _ => null
        };
    }
    
    private DateOnly GetNextWeekly(DateOnly from)
    {
        var targetDay = DayOfWeek ?? 1; // Default Monday
        var daysUntil = ((targetDay - (int)from.DayOfWeek + 7) % 7);
        if (daysUntil == 0) daysUntil = 7;
        return from.AddDays(daysUntil);
    }
    
    private DateOnly GetNextMonthly(DateOnly from)
    {
        var targetDay = DayOfMonth ?? 1;
        var nextMonth = new DateOnly(from.Year, from.Month, 1).AddMonths(from.Day >= targetDay ? 1 : 0);
        var day = Math.Min(targetDay, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
        return new DateOnly(nextMonth.Year, nextMonth.Month, day);
    }
    
    private DateOnly GetNextYearly(DateOnly from)
    {
        var targetDay = DayOfMonth ?? 1;
        var targetMonth = StartDate.Month;
        var nextYear = from.Year + (from.Month > targetMonth || (from.Month == targetMonth && from.Day >= targetDay) ? 1 : 0);
        var day = Math.Min(targetDay, DateTime.DaysInMonth(nextYear, targetMonth));
        return new DateOnly(nextYear, targetMonth, day);
    }
}
