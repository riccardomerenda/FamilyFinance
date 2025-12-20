using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFinance.Controllers;

[Route("[controller]/[action]")]
public class CultureController : Controller
{
    public IActionResult Set(string culture, string? redirectUri)
    {
        // Clean the culture parameter (remove any trailing ? or whitespace)
        culture = culture?.Trim().TrimEnd('?') ?? "it-IT";
        
        if (!string.IsNullOrWhiteSpace(culture))
        {
            try
            {
                HttpContext.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(
                        new RequestCulture(culture, culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );
            }
            catch
            {
                // Invalid culture, use default
                culture = "it-IT";
                HttpContext.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(
                        new RequestCulture(culture, culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );
            }
        }

        // Try to get redirect URL from Referer header
        var returnUrl = redirectUri;
        
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            var referer = Request.Headers.Referer.ToString();
            if (!string.IsNullOrEmpty(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
            {
                returnUrl = refererUri.PathAndQuery;
            }
        }

        // Final fallback to home
        if (string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith('/'))
        {
            returnUrl = "/";
        }

        return Redirect(returnUrl);
    }
}
