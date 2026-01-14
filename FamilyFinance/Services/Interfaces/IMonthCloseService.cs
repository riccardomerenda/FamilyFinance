using FamilyFinance.Models;

namespace FamilyFinance.Services.Interfaces;

public interface IMonthCloseService
{
    /// <summary>
    /// Checks if we should prompt the user to close the previous month
    /// </summary>
    Task<bool> ShouldPromptMonthCloseAsync(int familyId);
    
    /// <summary>
    /// Returns the month that needs to be closed (e.g., date of last month)
    /// </summary>
    Task<DateTime?> GetMonthToCloseAsync(int familyId);

    /// <summary>
    /// Closes the specified month by creating a snapshot from live data
    /// </summary>
    Task<ServiceResult<int>> CloseMonthAsync(int familyId, int year, int month);
    
    /// <summary>
    /// Dismisses the prompt for the specified month
    /// </summary>
    Task DismissPromptAsync(int familyId, int year, int month);
}
