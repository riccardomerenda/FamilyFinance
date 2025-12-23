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
    
    /// <summary>
    /// Get all snapshots with their totals in a single optimized query (avoids N+1)
    /// </summary>
    Task<List<SnapshotSummary>> GetAllWithTotalsAsync(int familyId);

    Task SaveExpensesAsync(int snapshotId, List<(int CategoryId, decimal Amount, string? Notes)> expenses);
}

/// <summary>
/// Summary record for chart data - avoids loading full entities
/// </summary>
public record SnapshotSummary(
    int Id,
    DateOnly Date,
    decimal Liquidity,
    decimal InvestmentsValue,
    decimal InvestmentsCost,
    decimal CreditsOpen,
    decimal PensionInsuranceValue,
    decimal PensionInsuranceContrib,
    decimal InterestLiquidity
)
{
    public decimal InvestmentsGainLoss => InvestmentsValue - InvestmentsCost;
    public decimal PensionInsuranceGainLoss => PensionInsuranceValue - PensionInsuranceContrib;
    public decimal CurrentTotal => Liquidity + InvestmentsValue;
    public decimal ProjectedTotal => CurrentTotal + CreditsOpen;
    public decimal GrandTotal => ProjectedTotal + PensionInsuranceValue;
}

