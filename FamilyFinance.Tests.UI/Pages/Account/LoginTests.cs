using Bunit;
using FamilyFinance.Pages.Account;
using FamilyFinance.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace FamilyFinance.Tests.UI.Pages.Account;

public class LoginTests : TestContext
{
    private readonly Mock<IAuthService> _authMock;
    private readonly Mock<IStringLocalizer<SharedResource>> _localizerMock;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public LoginTests()
    {
        _authMock = new Mock<IAuthService>();
        _localizerMock = new Mock<IStringLocalizer<SharedResource>>();
        _envMock = new Mock<IWebHostEnvironment>();

        // Setup common mocks
        _envMock.Setup(e => e.EnvironmentName).Returns("Production"); // Default to prod (no demo banner)
        
        // Localizer setup: return the key as value
        _localizerMock.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        // Register services
        Services.AddSingleton(_authMock.Object);
        Services.AddSingleton(_localizerMock.Object);
        Services.AddSingleton(_envMock.Object);
    }

    [Fact]
    public void Login_RendersFormCorrectly()
    {
        // Arrange
        _authMock.Setup(a => a.HasAnyUsersAsync()).ReturnsAsync(true);

        // Act
        var cut = Render<Login>();

        // Assert
        Assert.NotNull(cut.Find("input[name='email']"));
        Assert.NotNull(cut.Find("input[name='password']"));
        Assert.NotNull(cut.Find("button[type='submit']"));
    }

    [Fact]
    public void Login_DisplaysErrorFromQueryString()
    {
        // Arrange
        var errorMessage = "Invalid credentials test";
        _authMock.Setup(a => a.HasAnyUsersAsync()).ReturnsAsync(true);
        
        // Navigate to URL with query string BEFORE rendering
        // [SupplyParameterFromQuery] reads from the current URL
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo($"/Account/Login?error={errorMessage}");

        // Act
        var cut = Render<Login>();

        // Assert
        // Look for the error box div (has text-red-600)
        var errorDiv = cut.Find(".text-red-600");
        Assert.Contains(errorMessage, errorDiv.TextContent);
    }

    [Fact]
    public void Login_ShowsDemoBanner_InProduction()
    {
        // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns("Production");
        _authMock.Setup(a => a.HasAnyUsersAsync()).ReturnsAsync(true);

        // Act
        var cut = Render<Login>();

        // Assert
        // Demo banner has text "Demo Mode" (localized key DemoMode)
        Assert.Contains("DemoMode", cut.Markup); 
        // Or check for specific element class
        Assert.NotNull(cut.Find(".border-amber-400\\/30")); // Escaped slash for css selector
    }
}
