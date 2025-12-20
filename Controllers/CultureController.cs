using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFinance.Controllers;

[Route("[controller]/[action]")]
public class CultureController : Controller
{
    public IActionResult Set(string culture, string redirectUri)
    {
        if (!string.IsNullOrWhiteSpace(culture))
        {
            HttpContext.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(
                    new RequestCulture(culture, culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
        }

        // Fallback to home if redirectUri is invalid
        if (string.IsNullOrWhiteSpace(redirectUri) || !redirectUri.StartsWith('/'))
        {
            redirectUri = "/";
        }

        return LocalRedirect(redirectUri);
    }
}
