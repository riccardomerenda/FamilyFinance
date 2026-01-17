using System.Globalization;
using FamilyFinance.Models.Import;

namespace FamilyFinance.Services;

/// <summary>
/// Service to parse Directa SIM portfolio CSV exports.
/// </summary>
public class DirectaImportService : IDirectaImportService
{
    /// <summary>
    /// Parses a Directa portfolio CSV stream.
    /// Expected format:
    /// - Lines 1-7: Header (portfolio info, account, date, empty lines)
    /// - Line 8: Column headers
    /// - Lines 9+: Data rows
    /// - Last row: Totals (empty first column)
    /// </summary>
    public Task<DirectaImportResult> ParsePortfolioCsvAsync(Stream content)
    {
        var result = new DirectaImportResult();
        
        try
        {
            content.Position = 0;
            using var reader = new StreamReader(content, leaveOpen: true);
            
            var culture = new CultureInfo("it-IT"); // Italian format for numbers
            
            // Parse header lines
            // Line 1: "Portafoglio : TOTALE,,,..."
            var line1 = reader.ReadLine();
            
            // Line 2: "Conto : L7999 MERENDA RICCARDO,,,..."
            var line2 = reader.ReadLine();
            if (line2 != null && line2.StartsWith("Conto :"))
            {
                result.AccountInfo = line2.Split(',')[0].Replace("Conto :", "").Trim();
            }
            
            // Line 3: "Data estrazione : 2026/01/16 17:37:41,,,..."
            var line3 = reader.ReadLine();
            if (line3 != null && line3.StartsWith("Data estrazione :"))
            {
                var dateStr = line3.Split(',')[0].Replace("Data estrazione :", "").Trim();
                if (DateTime.TryParse(dateStr, out var extractDate))
                {
                    result.ExtractionDate = extractDate;
                }
            }
            
            // Skip lines 4-7 (empty + total value + more empty)
            for (int i = 0; i < 4 && !reader.EndOfStream; i++)
            {
                var skipLine = reader.ReadLine();
                // Try to extract total value from line like: "Valore portafoglio : 25332,28€"
                if (skipLine != null && skipLine.Contains("Valore portafoglio"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(skipLine, @"([\d\.,]+)€");
                    if (match.Success)
                    {
                        var valueStr = match.Groups[1].Value.Replace(".", ""); // Remove thousands sep
                        if (decimal.TryParse(valueStr, NumberStyles.Any, culture, out var totalVal))
                        {
                            result.TotalValue = totalVal;
                        }
                    }
                }
            }
            
            // Line 8: Column headers
            // "Strumento,Ticker,Isin,Prezzo,Trend %,Quantita,Valore di carico,Valore attuale,..."
            var headerLine = reader.ReadLine();
            if (headerLine == null)
            {
                result.ErrorMessage = "CSV file appears to be empty or invalid format";
                return Task.FromResult(result);
            }
            
            // Parse column indices from header
            var headers = ParseCsvLine(headerLine, ',');
            var tickerIdx = Array.FindIndex(headers, h => h.Equals("Ticker", StringComparison.OrdinalIgnoreCase));
            var isinIdx = Array.FindIndex(headers, h => h.Equals("Isin", StringComparison.OrdinalIgnoreCase));
            var nameIdx = Array.FindIndex(headers, h => h.Equals("Strumento", StringComparison.OrdinalIgnoreCase));
            var priceIdx = Array.FindIndex(headers, h => h.Equals("Prezzo", StringComparison.OrdinalIgnoreCase));
            var qtyIdx = Array.FindIndex(headers, h => h.Equals("Quantita", StringComparison.OrdinalIgnoreCase));
            var costIdx = Array.FindIndex(headers, h => h.Contains("carico", StringComparison.OrdinalIgnoreCase));
            var valueIdx = Array.FindIndex(headers, h => h.Contains("attuale", StringComparison.OrdinalIgnoreCase));
            
            if (tickerIdx < 0 || costIdx < 0 || valueIdx < 0)
            {
                result.ErrorMessage = "Required columns not found: Ticker, Valore di carico, Valore attuale";
                return Task.FromResult(result);
            }
            
            // Parse data rows
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                var fields = ParseCsvLine(line, ',');
                
                // Skip footer row (first field is empty, contains totals)
                if (fields.Length == 0 || string.IsNullOrWhiteSpace(fields[0]))
                {
                    // This is likely the totals row - extract totals
                    if (fields.Length > costIdx && fields.Length > valueIdx)
                    {
                        TryParseDecimal(fields[costIdx], culture, out var totalCost);
                        result.TotalCostBasis = totalCost;
                    }
                    continue;
                }
                
                var asset = new DirectaAssetRow();
                
                // Ticker
                if (tickerIdx < fields.Length)
                    asset.Ticker = fields[tickerIdx].Trim();
                
                // ISIN
                if (isinIdx >= 0 && isinIdx < fields.Length)
                    asset.ISIN = fields[isinIdx].Trim();
                
                // Full name
                if (nameIdx >= 0 && nameIdx < fields.Length)
                    asset.FullName = fields[nameIdx].Trim();
                
                // Price
                if (priceIdx >= 0 && priceIdx < fields.Length)
                {
                    if (TryParseDecimal(fields[priceIdx], culture, out var price))
                        asset.Price = price;
                }
                
                // Quantity
                if (qtyIdx >= 0 && qtyIdx < fields.Length)
                {
                    if (TryParseDecimal(fields[qtyIdx], culture, out var qty))
                        asset.Quantity = qty;
                }
                
                // Cost Basis
                if (costIdx < fields.Length)
                {
                    if (TryParseDecimal(fields[costIdx], culture, out var cost))
                        asset.CostBasis = cost;
                }
                
                // Current Value
                if (valueIdx < fields.Length)
                {
                    if (TryParseDecimal(fields[valueIdx], culture, out var val))
                        asset.CurrentValue = val;
                }
                
                // Only add valid assets (with ticker)
                if (!string.IsNullOrEmpty(asset.Ticker))
                {
                    result.Assets.Add(asset);
                }
            }
            
            result.Success = result.Assets.Count > 0;
            if (!result.Success && string.IsNullOrEmpty(result.ErrorMessage))
            {
                result.ErrorMessage = "No valid assets found in CSV";
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Error parsing CSV: {ex.Message}";
        }
        
        return Task.FromResult(result);
    }
    
    private bool TryParseDecimal(string value, CultureInfo culture, out decimal result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(value)) return false;
        
        // Clean the value: remove currency symbols
        var cleaned = value.Replace("€", "").Replace("$", "").Trim();
        
        // Directa CSV uses US format: dot as decimal separator (e.g., 5527.21)
        // Use InvariantCulture for parsing
        return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }
    
    private string[] ParseCsvLine(string line, char delimiter)
    {
        var result = new List<string>();
        bool inQuotes = false;
        string currentField = "";
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentField += '"';
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == delimiter && !inQuotes)
            {
                result.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        result.Add(currentField);
        
        return result.ToArray();
    }
}
