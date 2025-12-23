using FamilyFinance.Models.Import;

namespace FamilyFinance.Services.Interfaces;

public interface ICsvImportService
{
    /// <summary>
    /// Reads the first few lines of a CSV to help user map columns
    /// </summary>
    Task<List<string[]>> PreviewCsvAsync(Stream content, int linesToRead = 5);
    
    /// <summary>
    /// Parses the full CSV using the provided mapping
    /// </summary>
    Task<List<ImportedTransaction>> ParseCsvAsync(Stream content, CsvColumnMapping mapping);
    
    /// <summary>
    /// Tries to guess categories for transactions based on description keywords
    /// </summary>
    Task AutoCategorizeAsync(List<ImportedTransaction> transactions, int familyId);
}
