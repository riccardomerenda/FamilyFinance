using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Services;

public class PortfolioService : IPortfolioService
{
    private readonly AppDbContext _db;

    public PortfolioService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Portfolio>> GetAllAsync(int familyId)
        => await _db.Portfolios
            .Where(p => p.FamilyId == familyId && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

    public async Task<Portfolio?> GetByIdAsync(int id)
        => await _db.Portfolios.FirstOrDefaultAsync(p => p.Id == id);

    public async Task SaveAsync(Portfolio portfolio)
    {
        if (portfolio.Id == 0)
        {
            portfolio.CreatedAt = DateTime.UtcNow;
            _db.Portfolios.Add(portfolio);
        }
        else
        {
            var existing = await _db.Portfolios.FindAsync(portfolio.Id);
            if (existing != null)
            {
                existing.Name = portfolio.Name;
                existing.Description = portfolio.Description;
                existing.TimeHorizonYears = portfolio.TimeHorizonYears;
                existing.TargetYear = portfolio.TargetYear;
                existing.Color = portfolio.Color;
                existing.IsActive = portfolio.IsActive;
            }
        }
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var portfolio = await _db.Portfolios.FirstOrDefaultAsync(p => p.Id == id);
        if (portfolio != null)
        {
            portfolio.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }
}

