namespace FamilyFinance.Services;

/// <summary>
/// Scoped service to manage privacy mode state across components
/// </summary>
public class PrivacyService
{
    public bool IsPrivacyModeEnabled { get; private set; }
    
    public event Action? OnChange;
    
    public void Toggle()
    {
        IsPrivacyModeEnabled = !IsPrivacyModeEnabled;
        NotifyStateChanged();
    }
    
    public void Enable()
    {
        if (!IsPrivacyModeEnabled)
        {
            IsPrivacyModeEnabled = true;
            NotifyStateChanged();
        }
    }
    
    public void Disable()
    {
        if (IsPrivacyModeEnabled)
        {
            IsPrivacyModeEnabled = false;
            NotifyStateChanged();
        }
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}
