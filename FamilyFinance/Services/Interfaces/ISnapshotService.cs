using FamilyFinance.Models;

namespace FamilyFinance.Services.Interfaces;

public interface ISnapshotService
{
    Task<List<Snapshot>> GetAllAsync(int familyId);
    Task<Snapshot?> GetByIdAsync(int id);
    Task<Snapshot?> GetLatestAsync(int familyId);
    Task<int> SaveAsync(int familyId, int? snapshotId, DateOnly date, 
        List<(int AccountId, decimal Amount, decimal ContributionBasis)> accountAmounts,
        List<(string Name, decimal CostBasis, decimal Value, int? PortfolioId)> investments,
        List<(string Description, decimal Amount, ReceivableStatus Status, DateOnly? ExpectedDate)> receivables);
    Task DeleteAsync(int id);
    Task<Totals> CalculateTotalsAsync(Snapshot snapshot);
}

