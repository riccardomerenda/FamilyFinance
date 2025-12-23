using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamilyFinance.Services;

public class BudgetService : IBudgetService
{
    private readonly AppDbContext _db;
    private readonly ILogger<BudgetService> _logger;
    private readonly IValidator<BudgetCategory> _validator;

    public BudgetService(AppDbContext db, ILogger<BudgetService> logger, IValidator<BudgetCategory> validator)
    {
        _db = db;
        _logger = logger;
        _validator = validator;
    }

    // Categories
    public async Task<List<BudgetCategory>> GetCategoriesAsync(int familyId)
    {
        _logger.LogDebug("Fetching budget categories for family {FamilyId}", familyId);
        return await _db.BudgetCategories
            .Where(c => c.FamilyId == familyId && c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<BudgetCategory?> GetCategoryByIdAsync(int id)
    {
        _logger.LogDebug("Fetching budget category {CategoryId}", id);
        return await _db.BudgetCategories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<ServiceResult> SaveCategoryAsync(BudgetCategory category, string? userId = null)
    {
        // Validate
        var validationResult = await _validator.ValidateAsync(category);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Budget category validation failed: {Errors}", string.Join(", ", errors));
            return ServiceResult.Fail(errors);
        }

        if (category.Id == 0)
        {
            var maxOrder = await _db.BudgetCategories
                .Where(c => c.FamilyId == category.FamilyId)
                .MaxAsync(c => (int?)c.SortOrder) ?? 0;
            category.SortOrder = maxOrder + 1;
            category.CreatedAt = DateTime.UtcNow;
            category.CreatedBy = userId;
            _db.BudgetCategories.Add(category);
            _logger.LogInformation("Creating new budget category '{CategoryName}' for family {FamilyId}", category.Name, category.FamilyId);
        }
        else
        {
            var existing = await _db.BudgetCategories.FindAsync(category.Id);
            if (existing == null || existing.IsDeleted)
            {
                _logger.LogWarning("Budget category {CategoryId} not found for update", category.Id);
                return ServiceResult.Fail("Categoria non trovata");
            }

            existing.Name = category.Name;
            existing.Icon = category.Icon;
            existing.Color = category.Color;
            existing.MonthlyBudget = category.MonthlyBudget;
            existing.SortOrder = category.SortOrder;
            existing.IsActive = category.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = userId;
            
            _logger.LogInformation("Updating budget category {CategoryId} '{CategoryName}'", category.Id, category.Name);
        }
        
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // Legacy method for backward compatibility
    public async Task SaveCategoryAsync(BudgetCategory category) => await SaveCategoryAsync(category, null);

    public async Task<ServiceResult> DeleteCategoryAsync(int id, string? userId = null)
    {
        var category = await _db.BudgetCategories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            _logger.LogWarning("Budget category {CategoryId} not found for deletion", id);
            return ServiceResult.Fail("Categoria non trovata");
        }

        // Soft delete
        category.IsDeleted = true;
        category.IsActive = false;
        category.DeletedAt = DateTime.UtcNow;
        category.DeletedBy = userId;
        
        await _db.SaveChangesAsync();
        _logger.LogInformation("Soft-deleted budget category {CategoryId} '{CategoryName}'", id, category.Name);
        
        return ServiceResult.Ok();
    }

    // Legacy method for backward compatibility
    public async Task DeleteCategoryAsync(int id) => await DeleteCategoryAsync(id, null);

    public List<BudgetCategory> GetDefaultCategories() => new()
    {
        new() { Name = "Casa/Affitto", Icon = "🏠", Color = "#6366f1", MonthlyBudget = 0 },
        new() { Name = "Alimentari", Icon = "🛒", Color = "#10b981", MonthlyBudget = 0 },
        new() { Name = "Trasporti", Icon = "🚗", Color = "#f59e0b", MonthlyBudget = 0 },
        new() { Name = "Utenze", Icon = "⚡", Color = "#ef4444", MonthlyBudget = 0 },
        new() { Name = "Svago", Icon = "🎭", Color = "#8b5cf6", MonthlyBudget = 0 },
        new() { Name = "Salute", Icon = "🏥", Color = "#ec4899", MonthlyBudget = 0 },
        new() { Name = "Abbigliamento", Icon = "👕", Color = "#06b6d4", MonthlyBudget = 0 },
        new() { Name = "Ristoranti", Icon = "🍕", Color = "#f97316", MonthlyBudget = 0 },
        new() { Name = "Abbonamenti", Icon = "📱", Color = "#14b8a6", MonthlyBudget = 0 },
        new() { Name = "Altro", Icon = "💰", Color = "#6b7280", MonthlyBudget = 0 }
    };

    public async Task InitializeCategoriesAsync(int familyId)
    {
        var existingCount = await _db.BudgetCategories.CountAsync(c => c.FamilyId == familyId);
        if (existingCount > 0) return;

        var defaults = GetDefaultCategories();
        var order = 1;
        foreach (var cat in defaults)
        {
            cat.FamilyId = familyId;
            cat.SortOrder = order++;
            _db.BudgetCategories.Add(cat);
        }
        await _db.SaveChangesAsync();
    }

    // Expenses
    public async Task<List<MonthlyExpense>> GetExpensesAsync(int snapshotId)
        => await _db.MonthlyExpenses
            .Include(e => e.Category)
            .Where(e => e.SnapshotId == snapshotId)
            .OrderBy(e => e.Category!.SortOrder)
            .ToListAsync();

    public async Task SaveExpensesAsync(int snapshotId, List<(int CategoryId, decimal Amount, string? Notes)> expenses)
    {
        var existing = await _db.MonthlyExpenses
            .Where(e => e.SnapshotId == snapshotId)
            .ToListAsync();

        foreach (var (categoryId, amount, notes) in expenses)
        {
            var expense = existing.FirstOrDefault(e => e.CategoryId == categoryId);
            if (expense != null)
            {
                expense.Amount = amount;
                expense.Notes = notes;
            }
            else if (amount > 0)
            {
                _db.MonthlyExpenses.Add(new MonthlyExpense
                {
                    SnapshotId = snapshotId,
                    CategoryId = categoryId,
                    Amount = amount,
                    Notes = notes
                });
            }
        }

        var toRemove = existing.Where(e => expenses.Any(x => x.CategoryId == e.CategoryId && x.Amount == 0));
        _db.MonthlyExpenses.RemoveRange(toRemove);

        await _db.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalExpensesAsync(int snapshotId)
        => await _db.MonthlyExpenses
            .Where(e => e.SnapshotId == snapshotId)
            .SumAsync(e => e.Amount);

    public async Task<decimal> GetTotalBudgetAsync(int familyId)
        => await _db.BudgetCategories
            .Where(c => c.FamilyId == familyId && c.IsActive)
            .SumAsync(c => c.MonthlyBudget);

    public async Task<BudgetSummary> GetSummaryAsync(int snapshotId, int familyId)
    {
        var categories = await _db.BudgetCategories
            .Where(c => c.FamilyId == familyId && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        var expenses = await _db.MonthlyExpenses
            .Where(e => e.SnapshotId == snapshotId)
            .ToListAsync();

        var categorySummaries = categories.Select(c =>
        {
            var spent = expenses.FirstOrDefault(e => e.CategoryId == c.Id)?.Amount ?? 0;
            var diff = c.MonthlyBudget - spent;
            var pct = c.MonthlyBudget > 0 ? (spent / c.MonthlyBudget) * 100 : 0;
            return new CategorySummary(
                c.Id, c.Name, c.Icon, c.Color, c.MonthlyBudget, spent, diff, pct,
                spent > c.MonthlyBudget && c.MonthlyBudget > 0
            );
        }).ToList();

        var totalBudget = categories.Sum(c => c.MonthlyBudget);
        var totalSpent = expenses.Sum(e => e.Amount);
        var totalDiff = totalBudget - totalSpent;
        var totalPct = totalBudget > 0 ? (totalSpent / totalBudget) * 100 : 0;

        return new BudgetSummary(totalBudget, totalSpent, totalDiff, totalPct, categorySummaries);
    }
}

