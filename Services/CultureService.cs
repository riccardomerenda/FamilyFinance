using System.Globalization;

namespace FamilyFinance.Services;

public class CultureService
{
    public event Action? OnCultureChanged;

    public CultureInfo CurrentCulture { get; private set; } = new CultureInfo("it-IT");

    public void SetCulture(string cultureName)
    {
        CurrentCulture = new CultureInfo(cultureName);
        CultureInfo.DefaultThreadCurrentCulture = CurrentCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CurrentCulture;
        OnCultureChanged?.Invoke();
    }

    public static readonly (string Code, string Name)[] SupportedCultures = new[]
    {
        ("it-IT", "Italiano"),
        ("en-US", "English")
    };
}

