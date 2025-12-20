using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFinance.Controllers;

[Route("[controller]")]
public class CultureController : Controller
{
    [HttpGet("Set/{culture}")]
    public IActionResult Set(string culture)
    {
        // Validate culture
        if (culture != "it-IT" && culture != "en-US")
        {
            culture = "it-IT";
        }

        // Set culture cookie
        HttpContext.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture, culture)),
            new CookieOptions 
            { 
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            }
        );

        // Get redirect URL from Referer, but NOT if it's the Culture controller itself
        var referer = Request.Headers.Referer.ToString();
        var returnUrl = "/";
        
        if (!string.IsNullOrEmpty(referer) && 
            Uri.TryCreate(referer, UriKind.Absolute, out var refererUri) &&
            !refererUri.AbsolutePath.StartsWith("/Culture"))
        {
            returnUrl = refererUri.AbsolutePath;
        }

        return Redirect(returnUrl);
    }
}
