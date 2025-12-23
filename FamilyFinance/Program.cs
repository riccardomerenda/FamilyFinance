using System.Globalization;
using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services;
using FamilyFinance.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.RateLimiting;

// Configure Serilog early for startup logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/familyfinance_.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting FamilyFinance application");

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog();

// Configure supported cultures
var supportedCultures = new[] 
{ 
    new CultureInfo("it-IT"),
    new CultureInfo("en-US") 
};

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Configure request localization
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("it-IT");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    
    // Cookie provider should be first
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
    options.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
});

// Database SQLite
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Password settings (balanced security for family use)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;  // Keep simple for families
    options.Password.RequireNonAlphanumeric = false;  // Keep simple for families
    options.Password.RequiredLength = 8;  // Increased from 6 to 8
    options.Password.RequiredUniqueChars = 4;  // At least 4 different characters
    
    // User settings
    options.User.RequireUniqueEmail = true;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);  // Increased from 5 to 15
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Cookie settings for authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// Data Protection - Persist keys to survive container restarts
var keysFolder = Path.Combine(
    builder.Environment.IsProduction() ? "/app/data" : builder.Environment.ContentRootPath,
    "keys");
Directory.CreateDirectory(keysFolder);

builder.Services.AddDataProtection()
    .SetApplicationName("FamilyFinance")
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder));

// HttpContextAccessor for getting request info in services
builder.Services.AddHttpContextAccessor();

// Services - Interfaces and Implementations
builder.Services.AddValidatorsFromAssemblyContaining<FamilyFinance.Validators.AccountValidator>();
builder.Services.AddScoped<ISnapshotService, SnapshotService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IImportExportService, ImportExportService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthService>(); // Temporary Keep self-registration for backward compatibility if needed, or remove if confident
builder.Services.AddSingleton<LogService>();
builder.Services.AddScoped<NotificationService>();  // Toast notifications
builder.Services.AddScoped<ActivityLogService>();   // Activity audit logging
builder.Services.AddScoped<WizardStateService>();   // Monthly closing wizard state
builder.Services.AddScoped<IInsightService, InsightService>(); // Dashboard insights

// Claims transformation - adds user's Role as a claim for [Authorize(Roles = "...")]
builder.Services.AddScoped<Microsoft.AspNetCore.Authentication.IClaimsTransformation, RoleClaimsTransformation>();

// Legacy facade (will be removed after full migration)
builder.Services.AddScoped<FinanceService>();

// Demo data seeder
builder.Services.AddScoped<DemoDataSeeder>();

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Global Limit: 100 requests per minute per IP
    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? context.Request.Headers.Host.ToString(),
            factory: partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 2,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    // Strict Auth Policy: 5 requests per minute per IP (Protection against Brute Force)
    options.AddPolicy("AuthPolicy", context =>
    {
        return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? context.Request.Headers.Host.ToString(),
            factory: partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    // Return 429 consistently
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

// Ensure database is created and apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    if (app.Environment.IsDevelopment())
    {
        // In development, use EnsureCreated for clean schema (no migrations needed)
        db.Database.EnsureCreated();
        
        // Seed demo data in development for testing
        var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
        await seeder.SeedDemoDataAsync();
    }
    else
    {
        // In production, use migrations and seed demo data
        db.Database.Migrate();
        var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
        await seeder.SeedDemoDataAsync();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseRequestLocalization();

// Health check endpoint for Docker/K8s
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

    Log.Information("Application started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
