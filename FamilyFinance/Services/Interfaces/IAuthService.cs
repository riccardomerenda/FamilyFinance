using FamilyFinance.Models;

namespace FamilyFinance.Services.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string? Error)> RegisterFirstUserAsync(string email, string password, string displayName, string familyName);
    Task<(bool Success, string? Error)> RegisterUserAsync(string email, string password, string displayName, int familyId, UserRole role);
    Task<(bool Success, string? Error, AppUser? User)> LoginAsync(string email, string password);
    Task LogoutAsync(AppUser? user = null);
    Task<AppUser?> GetUserByEmailAsync(string email);
    Task<AppUser?> GetUserWithFamilyAsync(string userId);
    Task<List<AppUser>> GetFamilyMembersAsync(int familyId);
    Task<bool> HasAnyUsersAsync();
    Task<(bool Success, string? Error)> UpdateUserRoleAsync(string userId, UserRole newRole, string adminUserId);
    Task<(bool Success, string? Error)> DeleteUserAsync(string userId, string adminUserId);
    Task<AppUser?> GetUserByIdAsync(string userId);
    Task<(bool Success, string? Error)> UpdateProfileAsync(string userId, string displayName);
    Task<(bool Success, string? Error)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}
