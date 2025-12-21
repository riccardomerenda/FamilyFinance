using FamilyFinance.Models;

namespace FamilyFinance.Services.Interfaces;

public interface IImportExportService
{
    Task<ImportPreview> PreviewImportAsync(BackupDto backup, int familyId);
    Task<ImportResult> ImportDataAsync(BackupDto backup, bool replaceExisting, int familyId);
    Task ClearAllDataAsync(int familyId);
}

