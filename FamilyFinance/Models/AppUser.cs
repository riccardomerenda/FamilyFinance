using Microsoft.AspNetCore.Identity;

namespace FamilyFinance.Models;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = "";
    public int FamilyId { get; set; }
    public Family Family { get; set; } = default!;
    public UserRole Role { get; set; } = UserRole.Member;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}

public enum UserRole
{
    Admin,      // Can edit everything, manage users
    Member,     // Can edit everything
    Viewer      // Read-only access
}

