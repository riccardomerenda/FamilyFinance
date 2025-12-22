namespace FamilyFinance.Models;

/// <summary>
/// Tracks user activities for audit purposes
/// </summary>
public class ActivityLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // User info
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserDisplayName { get; set; }
    
    // Action details
    public ActivityAction Action { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? EntityName { get; set; }
    public string? Details { get; set; }
    
    // Request info
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    // Family scope
    public int FamilyId { get; set; }
}

public enum ActivityAction
{
    // Authentication
    Login,
    Logout,
    LoginFailed,
    PasswordChanged,
    
    // CRUD Operations
    Create,
    Update,
    Delete,
    
    // Data operations
    Export,
    Import,
    
    // User management
    UserAdded,
    UserRemoved,
    RoleChanged,
    
    // Other
    View,
    Other
}

public static class EntityTypes
{
    public const string Snapshot = "Snapshot";
    public const string Account = "Account";
    public const string Goal = "Goal";
    public const string Portfolio = "Portfolio";
    public const string BudgetCategory = "BudgetCategory";
    public const string User = "User";
    public const string Family = "Family";
}

