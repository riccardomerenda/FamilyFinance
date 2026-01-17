using FamilyFinance.Models.Import;

namespace FamilyFinance.Services;

public interface IDirectaImportService
{
    Task<DirectaImportResult> ParsePortfolioCsvAsync(Stream content);
}
