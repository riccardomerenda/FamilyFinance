using FamilyFinance.Models;

namespace FamilyFinance.Services.Interfaces;

/// <summary>
/// Service interface for generating financial reports
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Generate monthly report data
    /// </summary>
    Task<MonthlyReportData> GenerateMonthlyReportAsync(int familyId, int year, int month);
    
    /// <summary>
    /// Generate yearly report data with monthly breakdown
    /// </summary>
    Task<YearlyReportData> GenerateYearlyReportAsync(int familyId, int year);
    
    /// <summary>
    /// Export report data to PDF format
    /// </summary>
    Task<byte[]> ExportToPdfAsync<T>(T reportData, ReportType type) where T : class;
    
    /// <summary>
    /// Export report data to Excel format
    /// </summary>
    Task<byte[]> ExportToExcelAsync<T>(T reportData, ReportType type) where T : class;
}
