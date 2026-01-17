using FamilyFinance.Models.Import;

namespace FamilyFinance.Services;

/// <summary>
/// Scoped service to hold investment import data when transitioning from Wizard to Snapshot page.
/// </summary>
public class InvestmentImportStateService
{
    public List<DirectaAssetRow>? ImportedAssets { get; private set; }
    public Dictionary<string, int> PortfolioAssignments { get; private set; } = new();
    public DateTime? ExtractionDate { get; private set; }
    
    public bool HasData => ImportedAssets != null && ImportedAssets.Any();

    public void SetData(List<DirectaAssetRow> assets, Dictionary<string, int> assignments, DateTime? extractionDate)
    {
        ImportedAssets = assets;
        PortfolioAssignments = assignments;
        ExtractionDate = extractionDate;
    }

    public void Clear()
    {
        ImportedAssets = null;
        ExtractionDate = null;
    }
}
