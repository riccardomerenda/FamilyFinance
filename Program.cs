using System.Globalization;
using FamilyFinance.Data;
using FamilyFinance.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure supported cultures
var supportedCultures = new[] { "it-IT", "en-US" };

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers(); // For CultureController

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Configure request localization
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture("it-IT");
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
});

// Database SQLite
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Services
builder.Services.AddScoped<FinanceService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization();

app.MapControllers(); // Map CultureController
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
