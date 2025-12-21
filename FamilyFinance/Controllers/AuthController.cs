using FamilyFinance.Models;
using FamilyFinance.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFinance.Controllers;

[Route("[controller]/[action]")]
public class AuthController : Controller
{
    private readonly AuthService _auth;
    private readonly SignInManager<AppUser> _signIn;

    public AuthController(AuthService auth, SignInManager<AppUser> signIn)
    {
        _auth = auth;
        _signIn = signIn;
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        var (success, error, user) = await _auth.LoginAsync(email, password);
        
        if (success)
        {
            return LocalRedirect(returnUrl ?? "/");
        }
        
        // Store error in TempData for the login page
        TempData["LoginError"] = error;
        return Redirect("/Account/Login");
    }

    [HttpPost]
    public async Task<IActionResult> Setup(string familyName, string displayName, string email, string password)
    {
        var (success, error) = await _auth.RegisterFirstUserAsync(email, password, displayName, familyName);
        
        if (success)
        {
            // Auto-login after registration
            await _auth.LoginAsync(email, password);
            return Redirect("/");
        }
        
        TempData["SetupError"] = error;
        return Redirect("/Account/Setup");
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return Redirect("/Account/Login");
    }
}

