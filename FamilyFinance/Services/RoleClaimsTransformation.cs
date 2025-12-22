using System.Security.Claims;
using FamilyFinance.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace FamilyFinance.Services;

/// <summary>
/// Transforms claims to include the user's role from the AppUser entity.
/// This bridges the gap between the AppUser.Role property and ASP.NET Identity's role system.
/// </summary>
public class RoleClaimsTransformation : IClaimsTransformation
{
    private readonly UserManager<AppUser> _userManager;

    public RoleClaimsTransformation(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Clone the principal to avoid modifying the original
        var identity = principal.Identity as ClaimsIdentity;
        if (identity == null || !identity.IsAuthenticated)
        {
            return principal;
        }

        // Check if role claim already exists (avoid duplicates)
        var existingRoleClaim = identity.FindFirst(ClaimTypes.Role);
        if (existingRoleClaim != null)
        {
            return principal;
        }

        // Get the user from the database
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return principal;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return principal;
        }

        // Add the role claim based on the user's Role property
        var roleName = user.Role.ToString(); // "Admin", "Member", or "Viewer"
        identity.AddClaim(new Claim(ClaimTypes.Role, roleName));

        return principal;
    }
}

