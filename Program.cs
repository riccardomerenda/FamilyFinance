using System.Globalization;
using FamilyFinance.Data;
using FamilyFinance.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure supported cultures
var supportedCultures = new[] { "it-IT", "en-US" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("it-IT")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

// Set default culture
var it = new CultureInfo("it-IT");
CultureInfo.DefaultThreadCurrentCulture = it;
CultureInfo.DefaultThreadCurrentUICulture = it;

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Database SQLite
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Services
builder.Services.AddScoped<FinanceService>();
builder.Services.AddScoped<CultureService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization(localizationOptions);

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
