using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamilyFinance.Services;

public class AccountService : IAccountService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AccountService> _logger;
    private readonly IValidator<Account> _validator;

    public AccountService(AppDbContext db, ILogger<AccountService> logger, IValidator<Account> validator)
    {
        _db = db;
        _logger = logger;
        _validator = validator;
    }

    public async Task<List<Account>> GetAllAsync(int familyId)
    {
        _logger.LogDebug("Fetching all accounts for family {FamilyId}", familyId);
        return await _db.Accounts
            .Where(a => a.FamilyId == familyId && !a.IsDeleted)
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<List<Account>> GetActiveAsync(int familyId)
    {
        _logger.LogDebug("Fetching active accounts for family {FamilyId}", familyId);
        return await _db.Accounts
            .Where(a => a.FamilyId == familyId && a.IsActive && !a.IsDeleted)
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        _logger.LogDebug("Fetching account {AccountId}", id);
        return await _db.Accounts.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
    }

    public async Task<ServiceResult> SaveAsync(Account account, string? userId = null)
    {
        // Validate
        var validationResult = await _validator.ValidateAsync(account);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Account validation failed: {Errors}", string.Join(", ", errors));
            return ServiceResult.Fail(errors);
        }

        if (account.Id == 0)
        {
            account.CreatedAt = DateTime.UtcNow;
            account.CreatedBy = userId;
            _db.Accounts.Add(account);
            _logger.LogInformation("Creating new account '{AccountName}' for family {FamilyId}", account.Name, account.FamilyId);
        }
        else
        {
            var existing = await _db.Accounts.FindAsync(account.Id);
            if (existing == null || existing.IsDeleted)
            {
                _logger.LogWarning("Account {AccountId} not found for update", account.Id);
                return ServiceResult.Fail("Conto non trovato");
            }

            existing.Name = account.Name;
            existing.Category = account.Category;
            existing.Owner = account.Owner;
            existing.IsActive = account.IsActive;
            existing.IsInterest = account.IsInterest;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = userId;
            
            _logger.LogInformation("Updating account {AccountId} '{AccountName}'", account.Id, account.Name);
        }
        
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // Legacy method for backward compatibility
    public async Task SaveAsync(Account account) => await SaveAsync(account, null);

    public async Task<ServiceResult> DeleteAsync(int id, string? userId = null)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == id);
        if (account == null)
        {
            _logger.LogWarning("Account {AccountId} not found for deletion", id);
            return ServiceResult.Fail("Conto non trovato");
        }

        // Soft delete
        account.IsDeleted = true;
        account.IsActive = false;
        account.DeletedAt = DateTime.UtcNow;
        account.DeletedBy = userId;
        
        await _db.SaveChangesAsync();
        _logger.LogInformation("Soft-deleted account {AccountId} '{AccountName}'", id, account.Name);
        
        return ServiceResult.Ok();
    }

    // Legacy method for backward compatibility
    public async Task DeleteAsync(int id) => await DeleteAsync(id, null);
}

