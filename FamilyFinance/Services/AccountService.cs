using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Services;

public class AccountService : IAccountService
{
    private readonly AppDbContext _db;

    public AccountService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Account>> GetAllAsync(int familyId)
        => await _db.Accounts
            .Where(a => a.FamilyId == familyId)
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Name)
            .ToListAsync();

    public async Task<List<Account>> GetActiveAsync(int familyId)
        => await _db.Accounts
            .Where(a => a.FamilyId == familyId && a.IsActive)
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Name)
            .ToListAsync();

    public async Task<Account?> GetByIdAsync(int id)
        => await _db.Accounts.FirstOrDefaultAsync(a => a.Id == id);

    public async Task SaveAsync(Account account)
    {
        if (account.Id == 0)
        {
            _db.Accounts.Add(account);
        }
        else
        {
            var existing = await _db.Accounts.FindAsync(account.Id);
            if (existing != null)
            {
                existing.Name = account.Name;
                existing.Category = account.Category;
                existing.Owner = account.Owner;
                existing.IsActive = account.IsActive;
                existing.IsInterest = account.IsInterest;
            }
        }
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == id);
        if (account != null)
        {
            account.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }
}

