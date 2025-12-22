namespace FamilyFinance.Services;

/// <summary>
/// Service for displaying toast notifications to users
/// </summary>
public class NotificationService
{
    public event Action<ToastMessage>? OnShow;
    public event Action<Guid>? OnHide;

    public void ShowSuccess(string message, string? title = null)
        => Show(new ToastMessage(ToastType.Success, message, title ?? "Successo"));

    public void ShowError(string message, string? title = null)
        => Show(new ToastMessage(ToastType.Error, message, title ?? "Errore"));

    public void ShowWarning(string message, string? title = null)
        => Show(new ToastMessage(ToastType.Warning, message, title ?? "Attenzione"));

    public void ShowInfo(string message, string? title = null)
        => Show(new ToastMessage(ToastType.Info, message, title ?? "Info"));

    private void Show(ToastMessage toast) => OnShow?.Invoke(toast);

    public void Hide(Guid id) => OnHide?.Invoke(id);
}

public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}

public record ToastMessage(
    ToastType Type,
    string Message,
    string Title,
    int DurationMs = 5000
)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; init; } = DateTime.Now;
}

