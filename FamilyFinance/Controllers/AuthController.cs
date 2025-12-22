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
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return Redirect($"/Account/Login?error={Uri.EscapeDataString("Email e Password sono obbligatori")}");
        }

        var (success, error, user) = await _auth.LoginAsync(email, password);
        
        if (success)
        {
            return LocalRedirect(returnUrl ?? "/dashboard");
        }
        
        // Pass error via query string
        return Redirect($"/Account/Login?error={Uri.EscapeDataString(error)}");
    }

    [HttpPost]
    public async Task<IActionResult> Setup(string familyName, string displayName, string email, string password)
    {
        var (success, error) = await _auth.RegisterFirstUserAsync(email, password, displayName, familyName);
        
        if (success)
        {
            // Auto-login after registration
            await _auth.LoginAsync(email, password);
            return Redirect("/dashboard");
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

