using FamilyFinance.Data;
using FamilyFinance.Models;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Services;

/// <summary>
/// Service for logging and querying user activities
/// </summary>
public class ActivityLogService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ActivityLogService> _logger;

    // Retention period in days
    private const int RetentionDays = 90;

    public ActivityLogService(
        AppDbContext db, 
        IHttpContextAccessor httpContextAccessor,
        ILogger<ActivityLogService> logger)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Log an activity
    /// </summary>
    public async Task LogAsync(
        ActivityAction action,
        int familyId,
        string? userId = null,
        string? userEmail = null,
        string? userDisplayName = null,
        string? entityType = null,
        string? entityId = null,
        string? entityName = null,
        string? details = null)
    {
        try
        {
            var log = new ActivityLog
            {
                Action = action,
                FamilyId = familyId,
                UserId = userId,
                UserEmail = userEmail,
                UserDisplayName = userDisplayName,
                EntityType = entityType,
                EntityId = entityId,
                EntityName = entityName,
                Details = details,
                IpAddress = GetIpAddress(),
                UserAgent = GetUserAgent()
            };

            _db.ActivityLogs.Add(log);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Activity logged: {Action} by {UserEmail} on {EntityType} {EntityId}",
                action, userEmail ?? "unknown", entityType ?? "-", entityId ?? "-");
        }
        catch (Exception ex)
        {
            // Don't let logging failures break the application
            _logger.LogError(ex, "Failed to log activity: {Action}", action);
        }
    }

    /// <summary>
    /// Log a login event
    /// </summary>
    public Task LogLoginAsync(AppUser user)
        => LogAsync(ActivityAction.Login, user.FamilyId, user.Id, user.Email, user.DisplayName);

    /// <summary>
    /// Log a logout event
    /// </summary>
    public Task LogLogoutAsync(AppUser user)
        => LogAsync(ActivityAction.Logout, user.FamilyId, user.Id, user.Email, user.DisplayName);

    /// <summary>
    /// Log a failed login attempt
    /// </summary>
    public async Task LogLoginFailedAsync(string email)
    {
        // Try to find the user to get family ID
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        var familyId = user?.FamilyId ?? 0;

        await LogAsync(
            ActivityAction.LoginFailed,
            familyId,
            user?.Id,
            email,
            user?.DisplayName,
            details: "Failed login attempt");
    }

    /// <summary>
    /// Log a password change
    /// </summary>
    public Task LogPasswordChangedAsync(AppUser user)
        => LogAsync(ActivityAction.PasswordChanged, user.FamilyId, user.Id, user.Email, user.DisplayName);

    /// <summary>
    /// Log entity creation
    /// </summary>
    public Task LogCreateAsync(AppUser user, string entityType, string entityId, string entityName)
        => LogAsync(ActivityAction.Create, user.FamilyId, user.Id, user.Email, user.DisplayName,
            entityType, entityId, entityName);

    /// <summary>
    /// Log entity update
    /// </summary>
    public Task LogUpdateAsync(AppUser user, string entityType, string entityId, string entityName)
        => LogAsync(ActivityAction.Update, user.FamilyId, user.Id, user.Email, user.DisplayName,
            entityType, entityId, entityName);

    /// <summary>
    /// Log entity deletion
    /// </summary>
    public Task LogDeleteAsync(AppUser user, string entityType, string entityId, string entityName)
        => LogAsync(ActivityAction.Delete, user.FamilyId, user.Id, user.Email, user.DisplayName,
            entityType, entityId, entityName);

    /// <summary>
    /// Log data export
    /// </summary>
    public Task LogExportAsync(AppUser user, string exportType)
        => LogAsync(ActivityAction.Export, user.FamilyId, user.Id, user.Email, user.DisplayName,
            details: $"Exported: {exportType}");

    /// <summary>
    /// Log data import
    /// </summary>
    public Task LogImportAsync(AppUser user, string importDetails)
        => LogAsync(ActivityAction.Import, user.FamilyId, user.Id, user.Email, user.DisplayName,
            details: importDetails);

    /// <summary>
    /// Log user management actions
    /// </summary>
    public Task LogUserManagementAsync(AppUser admin, ActivityAction action, AppUser targetUser, string? details = null)
        => LogAsync(action, admin.FamilyId, admin.Id, admin.Email, admin.DisplayName,
            EntityTypes.User, targetUser.Id, targetUser.DisplayName, details);

    /// <summary>
    /// Get activity logs for a family with optional filters
    /// </summary>
    public async Task<List<ActivityLog>> GetLogsAsync(
        int familyId,
        int limit = 100,
        string? userId = null,
        ActivityAction? action = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _db.ActivityLogs
            .Where(l => l.FamilyId == familyId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(l => l.UserId == userId);

        if (action.HasValue)
            query = query.Where(l => l.Action == action.Value);

        if (fromDate.HasValue)
            query = query.Where(l => l.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.Timestamp <= toDate.Value);

        return await query
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Get activity statistics for a family
    /// </summary>
    public async Task<ActivityStats> GetStatsAsync(int familyId, int days = 30)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        
        var logs = await _db.ActivityLogs
            .Where(l => l.FamilyId == familyId && l.Timestamp >= since)
            .ToListAsync();

        return new ActivityStats
        {
            TotalActivities = logs.Count,
            Logins = logs.Count(l => l.Action == ActivityAction.Login),
            FailedLogins = logs.Count(l => l.Action == ActivityAction.LoginFailed),
            Creates = logs.Count(l => l.Action == ActivityAction.Create),
            Updates = logs.Count(l => l.Action == ActivityAction.Update),
            Deletes = logs.Count(l => l.Action == ActivityAction.Delete),
            Exports = logs.Count(l => l.Action == ActivityAction.Export),
            Imports = logs.Count(l => l.Action == ActivityAction.Import),
            UniqueUsers = logs.Where(l => l.UserId != null).Select(l => l.UserId).Distinct().Count(),
            LastActivity = logs.MaxBy(l => l.Timestamp)?.Timestamp
        };
    }

    /// <summary>
    /// Get list of users who have activity in the family
    /// </summary>
    public async Task<List<(string UserId, string DisplayName, string Email)>> GetActiveUsersAsync(int familyId)
    {
        return await _db.ActivityLogs
            .Where(l => l.FamilyId == familyId && l.UserId != null)
            .Select(l => new { l.UserId, l.UserDisplayName, l.UserEmail })
            .Distinct()
            .Select(l => ValueTuple.Create(l.UserId!, l.UserDisplayName ?? "", l.UserEmail ?? ""))
            .ToListAsync();
    }

    /// <summary>
    /// Clean up old logs beyond retention period
    /// </summary>
    public async Task CleanupOldLogsAsync()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-RetentionDays);
        
        var oldLogs = await _db.ActivityLogs
            .Where(l => l.Timestamp < cutoffDate)
            .ToListAsync();

        if (oldLogs.Any())
        {
            _db.ActivityLogs.RemoveRange(oldLogs);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Cleaned up {Count} activity logs older than {Days} days", oldLogs.Count, RetentionDays);
        }
    }

    private string? GetIpAddress()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Check for forwarded IP (behind proxy/load balancer)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',').First().Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private string? GetUserAgent()
    {
        try
        {
            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault();
            // Truncate to reasonable length
            return userAgent?.Length > 200 ? userAgent[..200] : userAgent;
        }
        catch
        {
            return null;
        }
    }
}

public class ActivityStats
{
    public int TotalActivities { get; set; }
    public int Logins { get; set; }
    public int FailedLogins { get; set; }
    public int Creates { get; set; }
    public int Updates { get; set; }
    public int Deletes { get; set; }
    public int Exports { get; set; }
    public int Imports { get; set; }
    public int UniqueUsers { get; set; }
    public DateTime? LastActivity { get; set; }
}

