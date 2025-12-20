using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyFinance.Controllers;

[Route("[controller]")]
public class CultureController : Controller
{
    [HttpGet("Set/{culture}")]
    public IActionResult Set(string culture, [FromQuery] string? returnUrl)
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

        // Use returnUrl if provided and valid, otherwise go home
        if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/") && !returnUrl.StartsWith("/Culture"))
        {
            return Redirect(returnUrl);
        }

        return Redirect("/");
    }
}
