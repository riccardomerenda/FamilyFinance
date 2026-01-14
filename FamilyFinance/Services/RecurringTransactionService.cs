using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamilyFinance.Services;

/// <summary>
/// Service for managing recurring transactions (subscriptions, salary, etc.)
/// </summary>
public class RecurringTransactionService : IRecurringTransactionService
{
    private readonly AppDbContext _db;
    private readonly ILogger<RecurringTransactionService> _logger;
    
    private readonly ITransactionService _transactionService;
    private readonly IAccountService _accountService;
    private readonly Microsoft.Extensions.Localization.IStringLocalizer<SharedResource> _L;

    public RecurringTransactionService(
        AppDbContext db, 
        ILogger<RecurringTransactionService> logger,
        ITransactionService transactionService,
        IAccountService accountService,
        Microsoft.Extensions.Localization.IStringLocalizer<SharedResource> L)
    {
        _db = db;
        _logger = logger;
        _transactionService = transactionService;
        _accountService = accountService;
        _L = L;
    }
    
    public async Task<List<RecurringTransaction>> GetAllAsync(int familyId, bool activeOnly = true)
    {
        var query = _db.RecurringTransactions
            .Include(r => r.Category)
            .Include(r => r.Account)
            .Where(r => r.FamilyId == familyId && !r.IsDeleted);
        
        if (activeOnly)
            query = query.Where(r => r.IsActive);
        
        return await query
            .OrderBy(r => r.Name)
            .ToListAsync();
    }
    
    public async Task<RecurringTransaction?> GetByIdAsync(int id)
    {
        return await _db.RecurringTransactions
            .Include(r => r.Category)
            .Include(r => r.Account)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }
    
    public async Task<ServiceResult> SaveAsync(RecurringTransaction recurring)
    {
        try
        {
            if (recurring.Id == 0)
            {
                recurring.CreatedAt = DateTime.UtcNow;
                _db.RecurringTransactions.Add(recurring);
                _logger.LogInformation("Creating new recurring transaction: {Name} - {Amount} {Frequency}", 
                    recurring.Name, recurring.Amount, recurring.Frequency);
            }
            else
            {
                var existing = await _db.RecurringTransactions.FindAsync(recurring.Id);
                if (existing == null)
                    return ServiceResult.Fail("Recurring transaction not found");
                
                existing.Name = recurring.Name;
                existing.Amount = recurring.Amount;
                existing.Type = recurring.Type;
                existing.Frequency = recurring.Frequency;
                existing.DayOfMonth = recurring.DayOfMonth;
                existing.DayOfWeek = recurring.DayOfWeek;
                existing.StartDate = recurring.StartDate;
                existing.EndDate = recurring.EndDate;
                existing.IsActive = recurring.IsActive;
                existing.CategoryId = recurring.CategoryId;
                existing.AccountId = recurring.AccountId;
                existing.NotifyBeforeDue = recurring.NotifyBeforeDue;
                existing.NotifyDaysBefore = recurring.NotifyDaysBefore;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = recurring.UpdatedBy;
                
                _logger.LogInformation("Updating recurring transaction {Id}: {Name}", 
                    recurring.Id, recurring.Name);
            }
            
            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving recurring transaction");
            return ServiceResult.Fail($"Error saving recurring transaction: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var recurring = await _db.RecurringTransactions.FindAsync(id);
            if (recurring == null)
                return ServiceResult.Fail("Recurring transaction not found");
            
            recurring.IsDeleted = true;
            recurring.DeletedAt = DateTime.UtcNow;
            recurring.IsActive = false;
            
            await _db.SaveChangesAsync();
            _logger.LogInformation("Soft-deleted recurring transaction {Id}", id);
            
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting recurring transaction {Id}", id);
            return ServiceResult.Fail($"Error deleting recurring transaction: {ex.Message}");
        }
    }
    
    public async Task<List<UpcomingTransaction>> GetUpcomingAsync(int familyId, int daysAhead = 30)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var endDate = today.AddDays(daysAhead);
        
        var recurring = await GetAllAsync(familyId, activeOnly: true);
        var upcoming = new List<UpcomingTransaction>();
        
        foreach (var r in recurring)
        {
            var nextDate = r.GetNextOccurrence(today);
            if (nextDate.HasValue && nextDate.Value <= endDate)
            {
                upcoming.Add(new UpcomingTransaction
                {
                    Recurring = r,
                    NextDate = nextDate.Value,
                    DaysUntil = nextDate.Value.DayNumber - today.DayNumber
                });
            }
        }
        
        return upcoming.OrderBy(u => u.NextDate).ToList();
    }
    
    public async Task<decimal> GetMonthlyTotalAsync(int familyId, TransactionType type)
    {
        var recurring = await _db.RecurringTransactions
            .Where(r => r.FamilyId == familyId && 
                        r.IsActive && 
                        !r.IsDeleted && 
                        r.Type == type)
            .ToListAsync();
        
        decimal total = 0;
        foreach (var r in recurring)
        {
            total += r.Frequency switch
            {
                RecurrenceFrequency.Daily => r.Amount * 30, // Approximate
                RecurrenceFrequency.Weekly => r.Amount * 4.33m, // Approximate
                RecurrenceFrequency.Monthly => r.Amount,
                RecurrenceFrequency.Yearly => r.Amount / 12,
                _ => 0
            };
        }
        
        return total;
    }
    
    public async Task<List<Transaction>> GenerateTransactionsForMonthAsync(int familyId, int year, int month)
    {
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        
        var recurring = await GetAllAsync(familyId, activeOnly: true);
        var transactions = new List<Transaction>();
        
        foreach (var r in recurring)
        {
            // Check if this recurring generates for this month
            if (r.StartDate > endDate || (r.EndDate.HasValue && r.EndDate.Value < startDate))
                continue;
            
            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                var nextDate = r.GetNextOccurrence(currentDate.AddDays(-1));
                if (!nextDate.HasValue || nextDate.Value > endDate)
                    break;
                
                if (nextDate.Value >= startDate && nextDate.Value <= endDate)
                {
                    transactions.Add(new Transaction
                    {
                        Date = nextDate.Value,
                        Amount = r.Amount,
                        Type = r.Type,
                        Description = r.Name,
                        CategoryId = r.CategoryId,
                        AccountId = r.AccountId,
                        FamilyId = familyId,
                        ImportSource = "recurring",
                        CreatedAt = DateTime.UtcNow
                    });
                }
                
                currentDate = nextDate.Value.AddDays(1);
            }
        }
        
        return transactions;
    }

    public async Task<ServiceResult<int>> ProcessRecurringForMonthAsync(int familyId, int year, int month)
    {
        try
        {
            var start = new DateOnly(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            // 1. Check if we already have recurring transactions for this month
            var existingCount = await _db.Transactions
                .Where(t => t.FamilyId == familyId && 
                            t.Date >= start && 
                            t.Date <= end &&
                            t.ImportSource == "recurring" &&
                            !t.IsDeleted)
                .CountAsync();

            if (existingCount > 0)
            {
                return ServiceResult<int>.Fail("Transactions for this period have already been generated.");
            }

            // 2. Generate transactions
            var transactions = await GenerateTransactionsForMonthAsync(familyId, year, month);

            if (transactions.Count == 0)
            {
                 // Success but 0 created
                 return ServiceResult<int>.Ok(0);
            }

            // 3. Save transactions (bulk)
            var saveResult = await _transactionService.CreateManyAsync(transactions);
            if (!saveResult.Success)
            {
                return ServiceResult<int>.Fail("Failed to save transactions: " + saveResult.Error);
            }

            // 4. Update Account Balances (Live Balance)
            foreach (var t in transactions)
            {
                if (t.AccountId.HasValue)
                {
                    // For expense, we subtract amount (amount is positive in model but logic handles it)
                    // Wait, TransactionService usually handles sign logic? 
                    // Let's check AccountService.UpdateBalanceAsync behavior.
                    
                    // Standard convention: 
                    // Income: +Amount
                    // Expense: -Amount
                    
                    decimal delta = t.Type == TransactionType.Income ? t.Amount : -t.Amount;
                    await _accountService.UpdateBalanceAsync(t.AccountId.Value, delta);
                }
            }

            return ServiceResult<int>.Ok(transactions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing recurring transactions for {Month}/{Year}", month, year);
            return ServiceResult<int>.Fail("An error occurred while generating transactions.");
        }
    }
}
