using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamilyFinance.Services;

/// <summary>
/// Service for managing individual transactions
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly AppDbContext _db;
    private readonly ILogger<TransactionService> _logger;
    private readonly IAccountService _accountService;
    
    public TransactionService(AppDbContext db, ILogger<TransactionService> logger, IAccountService accountService)
    {
        _db = db;
        _logger = logger;
        _accountService = accountService;
    }
    
    public async Task<List<Transaction>> GetAllAsync(int familyId, TransactionFilter? filter = null)
    {
        var query = _db.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .Where(t => t.FamilyId == familyId && !t.IsDeleted)
            .AsQueryable();
        
        if (filter != null)
        {
            if (filter.FromDate.HasValue)
                query = query.Where(t => t.Date >= filter.FromDate.Value);
            
            if (filter.ToDate.HasValue)
                query = query.Where(t => t.Date <= filter.ToDate.Value);
            
            if (filter.CategoryId.HasValue)
                query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
            
            if (filter.AccountId.HasValue)
                query = query.Where(t => t.AccountId == filter.AccountId.Value);
            
            if (filter.Type.HasValue)
                query = query.Where(t => t.Type == filter.Type.Value);
            
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                query = query.Where(t => 
                    t.Description.Contains(filter.SearchText) || 
                    (t.Payee != null && t.Payee.Contains(filter.SearchText)) ||
                    (t.Notes != null && t.Notes.Contains(filter.SearchText)));
            
            query = query.Skip(filter.Skip).Take(filter.Take);
        }
        else
        {
            query = query.Take(100); // Default limit
        }
        
        return await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<Transaction?> GetByIdAsync(long id)
    {
        return await _db.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }
    
    public async Task<ServiceResult> SaveAsync(Transaction transaction)
    {
        try
        {
            if (transaction.Id == 0)
            {
                transaction.CreatedAt = DateTime.UtcNow;
                _db.Transactions.Add(transaction);
                
                // Update Account Balance (New)
                if (transaction.AccountId.HasValue)
                {
                    var delta = GetSignedAmount(transaction);
                    await _accountService.UpdateBalanceAsync(transaction.AccountId.Value, delta);
                }
                
                _logger.LogInformation("Creating new transaction: {Description} - {Amount}", 
                    transaction.Description, transaction.Amount);
            }
            else
            {
                var existing = await _db.Transactions.FindAsync(transaction.Id);
                if (existing == null)
                    return ServiceResult.Fail("Transaction not found");

                // Calculate Balance Impact
                // 1. Revert Old (subtract old signed amount)
                if (existing.AccountId.HasValue)
                {
                    var oldDelta = GetSignedAmount(existing);
                    await _accountService.UpdateBalanceAsync(existing.AccountId.Value, -oldDelta);
                }
                
                // Update Properties
                existing.Date = transaction.Date;
                existing.Amount = transaction.Amount;
                existing.Type = transaction.Type;
                existing.Description = transaction.Description;
                existing.Notes = transaction.Notes;
                existing.Payee = transaction.Payee;
                existing.CategoryId = transaction.CategoryId;
                existing.AccountId = transaction.AccountId;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = transaction.UpdatedBy;
                
                // 2. Apply New (add new signed amount)
                if (existing.AccountId.HasValue)
                {
                    var newDelta = GetSignedAmount(existing);
                    await _accountService.UpdateBalanceAsync(existing.AccountId.Value, newDelta);
                }
                
                _logger.LogInformation("Updating transaction {Id}: {Description}", 
                    transaction.Id, transaction.Description);
            }
            
            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving transaction");
            return ServiceResult.Fail($"Error saving transaction: {ex.Message}");
        }
    }
    
    public async Task<ServiceResult> DeleteAsync(long id)
    {
        try
        {
            var transaction = await _db.Transactions.FindAsync(id);
            if (transaction == null)
                return ServiceResult.Fail("Transaction not found");
            
            transaction.IsDeleted = true;
            transaction.DeletedAt = DateTime.UtcNow;
            
            // Revert Account Balance
            if (transaction.AccountId.HasValue)
            {
                var delta = GetSignedAmount(transaction);
                await _accountService.UpdateBalanceAsync(transaction.AccountId.Value, -delta);
            }
            
            await _db.SaveChangesAsync();
            _logger.LogInformation("Soft-deleted transaction {Id}", id);
            
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction {Id}", id);
            return ServiceResult.Fail($"Error deleting transaction: {ex.Message}");
        }
    }
    
    private decimal GetSignedAmount(Transaction t)
    {
        if (t.Type == TransactionType.Expense) return -Math.Abs(t.Amount);
        if (t.Type == TransactionType.Income) return Math.Abs(t.Amount);
        return t.Amount; // Transfer (assumed signed)
    }

    public async Task<decimal> GetTotalByCategoryAsync(int familyId, int categoryId, DateOnly from, DateOnly to)
    {
        return await _db.Transactions
            .Where(t => t.FamilyId == familyId && 
                        t.CategoryId == categoryId && 
                        t.Date >= from && 
                        t.Date <= to &&
                        !t.IsDeleted)
            .SumAsync(t => t.Amount);
    }
    
    public async Task<List<ReportCategorySummary>> GetSummaryByCategoryAsync(int familyId, DateOnly from, DateOnly to, TransactionType type)
    {
        // Fetch transactions with categories first, then group client-side
        // This avoids complex GroupBy translation issues with SQLite
        var transactions = await _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.FamilyId == familyId && 
                        t.Date >= from && 
                        t.Date <= to &&
                        t.Type == type &&
                        !t.IsDeleted &&
                        t.CategoryId != null)
            .ToListAsync();
        
        var grouped = transactions
            .GroupBy(t => t.CategoryId!.Value)
            .Select(g => 
            {
                var first = g.First();
                return new ReportCategorySummary
                {
                    CategoryId = g.Key,
                    CategoryName = first.Category?.Name ?? "Unknown",
                    CategoryIcon = first.Category?.Icon ?? "💰",
                    CategoryColor = first.Category?.Color ?? "#6366f1",
                    Amount = g.Sum(t => t.Amount),
                    Budget = first.Category?.MonthlyBudget ?? 0,
                    TransactionCount = g.Count()
                };
            })
            .ToList();
        
        var total = grouped.Sum(c => c.Amount);
        foreach (var category in grouped)
        {
            category.Percentage = total > 0 ? (category.Amount / total) * 100 : 0;
        }
        
        return grouped.OrderByDescending(c => c.Amount).ToList();
    }
    
    public async Task<int> GetCountAsync(int familyId, TransactionFilter? filter = null)
    {
        var query = _db.Transactions
            .Where(t => t.FamilyId == familyId && !t.IsDeleted)
            .AsQueryable();
        
        if (filter != null)
        {
            if (filter.FromDate.HasValue)
                query = query.Where(t => t.Date >= filter.FromDate.Value);
            
            if (filter.ToDate.HasValue)
                query = query.Where(t => t.Date <= filter.ToDate.Value);
            
            if (filter.CategoryId.HasValue)
                query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
            
            if (filter.AccountId.HasValue)
                query = query.Where(t => t.AccountId == filter.AccountId.Value);
            
            if (filter.Type.HasValue)
                query = query.Where(t => t.Type == filter.Type.Value);
        }
        
        return await query.CountAsync();
    }

    public async Task<ServiceResult> CreateManyAsync(List<Transaction> transactions)
    {
        if (transactions == null || !transactions.Any())
            return ServiceResult.Ok();

        try
        {
            await _db.Set<Transaction>().AddRangeAsync(transactions);
            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating batch transactions");
            return ServiceResult.Fail($"Error creating transactions: {ex.Message}");
        }
    }

    public async Task<List<Transaction>> GetByMonthAsync(int familyId, int year, int month)
    {
        try
        {
            var start = new DateOnly(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            return await _db.Set<Transaction>()
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => t.FamilyId == familyId 
                           && !t.IsDeleted
                           && t.Date >= start 
                           && t.Date <= end)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for {Month}/{Year}", month, year);
            return new List<Transaction>();
        }
    }
}
