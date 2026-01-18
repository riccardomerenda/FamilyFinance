using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamilyFinance.Services;

/// <summary>
/// Service for managing live pension/insurance holdings.
/// </summary>
public class PensionHoldingService : IPensionHoldingService
{
    private readonly AppDbContext _db;
    private readonly ISnapshotService _snapshotService;
    private readonly ILogger<PensionHoldingService> _logger;

    public PensionHoldingService(
        AppDbContext db,
        ISnapshotService snapshotService,
        ILogger<PensionHoldingService> logger)
    {
        _db = db;
        _snapshotService = snapshotService;
        _logger = logger;
    }

    public async Task<List<PensionHolding>> GetAllAsync(int familyId)
    {
        var holdings = await _db.PensionHoldings
            .Include(p => p.Account)
            .Where(p => p.FamilyId == familyId && p.Account.IsActive && !p.Account.IsDeleted)
            .OrderBy(p => p.Account.Category)
            .ThenBy(p => p.Account.Name)
            .ToListAsync();
        
        // Sync CurrentValue with Account.CurrentBalance (source of truth for live value)
        var needsSave = false;
        foreach (var holding in holdings)
        {
            if (holding.Account != null && holding.CurrentValue != holding.Account.CurrentBalance)
            {
                _logger.LogDebug("Syncing PensionHolding {Id} CurrentValue from {Old} to {New}", 
                    holding.Id, holding.CurrentValue, holding.Account.CurrentBalance);
                holding.CurrentValue = holding.Account.CurrentBalance;
                holding.LastUpdated = DateTime.UtcNow;
                needsSave = true;
            }
        }
        
        if (needsSave)
        {
            await _db.SaveChangesAsync();
        }
        
        return holdings;
    }

    public async Task<PensionHolding?> GetByAccountAsync(int accountId)
    {
        return await _db.PensionHoldings
            .Include(p => p.Account)
            .FirstOrDefaultAsync(p => p.AccountId == accountId);
    }

    public async Task<ServiceResult> AddContributionAsync(int accountId, decimal amount)
    {
        try
        {
            var holding = await GetByAccountAsync(accountId);
            if (holding == null)
                return ServiceResult.Fail("Pension holding not found");
            
            if (amount <= 0)
                return ServiceResult.Fail("Contribution amount must be positive");
            
            holding.ContributionBasis += amount;
            holding.LastUpdated = DateTime.UtcNow;
            
            await _db.SaveChangesAsync();
            _logger.LogInformation("Added contribution of {Amount} to pension account {Id} ({Name})", 
                amount, accountId, holding.Account?.Name);
            
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding contribution to pension account {Id}", accountId);
            return ServiceResult.Fail($"Error adding contribution: {ex.Message}");
        }
    }

    public async Task<ServiceResult> UpdateValueAsync(int accountId, decimal newValue)
    {
        try
        {
            var holding = await EnsureHoldingExistsAsync(0, accountId);
            
            holding.CurrentValue = newValue;
            holding.LastUpdated = DateTime.UtcNow;
            
            // Also update the Account.CurrentBalance for consistency
            if (holding.Account != null)
            {
                holding.Account.CurrentBalance = newValue;
            }
            
            await _db.SaveChangesAsync();
            _logger.LogDebug("Updated pension account {Id} value to {Value}", accountId, newValue);
            
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating pension account {Id} value", accountId);
            return ServiceResult.Fail($"Error updating value: {ex.Message}");
        }
    }

    public async Task<PensionHolding> EnsureHoldingExistsAsync(int familyId, int accountId)
    {
        var existing = await GetByAccountAsync(accountId);
        if (existing != null)
            return existing;

        // Get the account to find family ID and initial value
        var account = await _db.Accounts.FindAsync(accountId);
        if (account == null)
            throw new InvalidOperationException($"Account {accountId} not found");

        var newHolding = new PensionHolding
        {
            FamilyId = account.FamilyId,
            AccountId = accountId,
            CurrentValue = account.CurrentBalance,
            ContributionBasis = 0, // Will be seeded or set manually
            LastUpdated = DateTime.UtcNow
        };

        _db.PensionHoldings.Add(newHolding);
        await _db.SaveChangesAsync();
        
        // Reload with Account included
        return (await GetByAccountAsync(accountId))!;
    }

    public async Task<int> SeedFromLatestSnapshotAsync(int familyId)
    {
        _logger.LogInformation("Seeding pension holdings from latest snapshot for family {FamilyId}", familyId);

        // Get all pension/insurance accounts for this family
        var pensionInsuranceAccounts = await _db.Accounts
            .Where(a => a.FamilyId == familyId 
                     && !a.IsDeleted 
                     && (a.Category == AccountCategory.Pension || a.Category == AccountCategory.Insurance))
            .ToListAsync();

        if (!pensionInsuranceAccounts.Any())
        {
            _logger.LogInformation("No pension/insurance accounts found for family {FamilyId}", familyId);
            return 0;
        }

        // Check existing holdings
        var existingAccountIds = await _db.PensionHoldings
            .Where(p => p.FamilyId == familyId)
            .Select(p => p.AccountId)
            .ToListAsync();

        // Get latest snapshot for contribution basis data
        var latestSnapshot = await _snapshotService.GetLatestAsync(familyId);
        var snapshotLines = latestSnapshot?.Lines ?? new List<SnapshotLine>();

        var seededCount = 0;
        foreach (var account in pensionInsuranceAccounts)
        {
            if (existingAccountIds.Contains(account.Id))
            {
                _logger.LogDebug("Pension holding already exists for account {Id}, skipping", account.Id);
                continue;
            }

            var snapshotLine = snapshotLines.FirstOrDefault(l => l.AccountId == account.Id);
            
            var holding = new PensionHolding
            {
                FamilyId = familyId,
                AccountId = account.Id,
                CurrentValue = account.CurrentBalance,
                ContributionBasis = snapshotLine?.ContributionBasis ?? 0,
                LastUpdated = DateTime.UtcNow
            };

            _db.PensionHoldings.Add(holding);
            seededCount++;
            _logger.LogDebug("Created pension holding for account {Name}: Value={Value}, Contrib={Contrib}", 
                account.Name, holding.CurrentValue, holding.ContributionBasis);
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} pension holdings from snapshot for family {FamilyId}", 
            seededCount, familyId);

        return seededCount;
    }
}
