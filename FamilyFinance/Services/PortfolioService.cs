using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using FamilyFinance.Services.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamilyFinance.Services;

public class PortfolioService : IPortfolioService
{
    private readonly AppDbContext _db;
    private readonly ILogger<PortfolioService> _logger;

    public PortfolioService(AppDbContext db, ILogger<PortfolioService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<Portfolio>> GetAllAsync(int familyId)
    {
        _logger.LogDebug("Fetching all portfolios for family {FamilyId}", familyId);
        return await _db.Portfolios
            .Where(p => p.FamilyId == familyId && p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Portfolio?> GetByIdAsync(int id)
    {
        _logger.LogDebug("Fetching portfolio {PortfolioId}", id);
        return await _db.Portfolios.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<ServiceResult> SaveAsync(Portfolio portfolio, string? userId = null)
    {
        // Validate
        var validation = portfolio.Validate();
        if (!validation.Success)
        {
            _logger.LogWarning("Portfolio validation failed: {Errors}", string.Join(", ", validation.Errors));
            return validation;
        }

        if (portfolio.Id == 0)
        {
            portfolio.CreatedAt = DateTime.UtcNow;
            portfolio.CreatedBy = userId;
            _db.Portfolios.Add(portfolio);
            _logger.LogInformation("Creating new portfolio '{PortfolioName}' for family {FamilyId}", portfolio.Name, portfolio.FamilyId);
        }
        else
        {
            var existing = await _db.Portfolios.FindAsync(portfolio.Id);
            if (existing == null || existing.IsDeleted)
            {
                _logger.LogWarning("Portfolio {PortfolioId} not found for update", portfolio.Id);
                return ServiceResult.Fail("Portafoglio non trovato");
            }

            existing.Name = portfolio.Name;
            existing.Description = portfolio.Description;
            existing.TimeHorizonYears = portfolio.TimeHorizonYears;
            existing.TargetYear = portfolio.TargetYear;
            existing.Color = portfolio.Color;
            existing.Icon = portfolio.Icon;
            existing.IsActive = portfolio.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = userId;
            
            _logger.LogInformation("Updating portfolio {PortfolioId} '{PortfolioName}'", portfolio.Id, portfolio.Name);
        }
        
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // Legacy method for backward compatibility
    public async Task SaveAsync(Portfolio portfolio) => await SaveAsync(portfolio, null);

    public async Task<ServiceResult> DeleteAsync(int id, string? userId = null)
    {
        var portfolio = await _db.Portfolios.FirstOrDefaultAsync(p => p.Id == id);
        if (portfolio == null)
        {
            _logger.LogWarning("Portfolio {PortfolioId} not found for deletion", id);
            return ServiceResult.Fail("Portafoglio non trovato");
        }

        // Soft delete
        portfolio.IsDeleted = true;
        portfolio.IsActive = false;
        portfolio.DeletedAt = DateTime.UtcNow;
        portfolio.DeletedBy = userId;
        
        await _db.SaveChangesAsync();
        _logger.LogInformation("Soft-deleted portfolio {PortfolioId} '{PortfolioName}'", id, portfolio.Name);
        
        return ServiceResult.Ok();
    }

    // Legacy method for backward compatibility
    public async Task DeleteAsync(int id) => await DeleteAsync(id, null);
}

