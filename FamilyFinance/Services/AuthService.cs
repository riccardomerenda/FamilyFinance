using FamilyFinance.Data;
using FamilyFinance.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Services;

public class AuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly AppDbContext _db;

    public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
    }

    /// <summary>
    /// Register first user as Admin with a new Family
    /// </summary>
    public async Task<(bool Success, string? Error)> RegisterFirstUserAsync(string email, string password, string displayName, string familyName)
    {
        // Create family
        var family = new Family { Name = familyName };
        _db.Families.Add(family);
        await _db.SaveChangesAsync();

        // Create user as Admin
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            FamilyId = family.Id,
            Role = UserRole.Admin
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            // Rollback family creation
            _db.Families.Remove(family);
            await _db.SaveChangesAsync();
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // Create default accounts for the family
        await CreateDefaultAccountsAsync(family.Id);

        return (true, null);
    }

    /// <summary>
    /// Register additional user to existing family
    /// </summary>
    public async Task<(bool Success, string? Error)> RegisterUserAsync(string email, string password, string displayName, int familyId, UserRole role)
    {
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            FamilyId = familyId,
            Role = role
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error, AppUser? User)> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return (false, "Email o password non corretti", null);
        }

        var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: true);
        
        if (result.Succeeded)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return (true, null, user);
        }

        if (result.IsLockedOut)
        {
            return (false, "Account bloccato per troppi tentativi. Riprova tra 5 minuti.", null);
        }

        return (false, "Email o password non corretti", null);
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<AppUser?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<AppUser?> GetUserWithFamilyAsync(string userId)
    {
        return await _db.Users
            .Include(u => u.Family)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<List<AppUser>> GetFamilyMembersAsync(int familyId)
    {
        return await _db.Users
            .Where(u => u.FamilyId == familyId)
            .OrderBy(u => u.DisplayName)
            .ToListAsync();
    }

    public async Task<bool> HasAnyUsersAsync()
    {
        return await _db.Users.AnyAsync();
    }

    public async Task<(bool Success, string? Error)> UpdateUserRoleAsync(string userId, UserRole newRole, string adminUserId)
    {
        var adminUser = await _db.Users.FindAsync(adminUserId);
        if (adminUser?.Role != UserRole.Admin)
        {
            return (false, "Solo gli admin possono modificare i ruoli");
        }

        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            return (false, "Utente non trovato");
        }

        if (user.Id == adminUserId && newRole != UserRole.Admin)
        {
            // Check if there's another admin
            var otherAdmins = await _db.Users.CountAsync(u => u.FamilyId == user.FamilyId && u.Role == UserRole.Admin && u.Id != userId);
            if (otherAdmins == 0)
            {
                return (false, "Deve esserci almeno un Admin nella famiglia");
            }
        }

        user.Role = newRole;
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteUserAsync(string userId, string adminUserId)
    {
        var adminUser = await _db.Users.FindAsync(adminUserId);
        if (adminUser?.Role != UserRole.Admin)
        {
            return (false, "Solo gli admin possono eliminare utenti");
        }

        if (userId == adminUserId)
        {
            return (false, "Non puoi eliminare te stesso");
        }

        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            return (false, "Utente non trovato");
        }

        await _userManager.DeleteAsync(user);
        return (true, null);
    }

    /// <summary>
    /// Get user by ID with family information
    /// </summary>
    public async Task<AppUser?> GetUserByIdAsync(string userId)
    {
        return await _db.Users
            .Include(u => u.Family)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    /// <summary>
    /// Update user profile (display name)
    /// </summary>
    public async Task<(bool Success, string? Error)> UpdateProfileAsync(string userId, string displayName)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            return (false, "Utente non trovato");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return (false, "Il nome non può essere vuoto");
        }

        if (displayName.Length > 100)
        {
            return (false, "Il nome non può superare i 100 caratteri");
        }

        user.DisplayName = displayName.Trim();
        await _db.SaveChangesAsync();
        return (true, null);
    }

    /// <summary>
    /// Change user password
    /// </summary>
    public async Task<(bool Success, string? Error)> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "Utente non trovato");
        }

        // Verify current password
        var passwordCheck = await _userManager.CheckPasswordAsync(user, currentPassword);
        if (!passwordCheck)
        {
            return (false, "La password attuale non è corretta");
        }

        // Change password
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return (true, null);
    }

    private async Task CreateDefaultAccountsAsync(int familyId)
    {
        var defaultAccounts = new List<Account>
        {
            new() { Name = "Conto Principale", Category = AccountCategory.Liquidity, FamilyId = familyId },
            new() { Name = "Conto Risparmi", Category = AccountCategory.Liquidity, FamilyId = familyId },
        };

        _db.Accounts.AddRange(defaultAccounts);
        await _db.SaveChangesAsync();
    }
}

