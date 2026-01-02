using System.Globalization;
using FamilyFinance.Models.Import;
using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using System.Text.RegularExpressions;

namespace FamilyFinance.Services;

public class CsvImportService : ICsvImportService
{
    private readonly IBudgetService _budgetService;
    private readonly ICategoryRuleService _categoryRuleService;
    
    private readonly Dictionary<string, string[]> _keywordRules = new()
    {
        { "Spesa", new[] { "supermarket", "carrefour", "coop", "conad", "esselunga", "lidl", "eurospin", "iper", "food" } },
        { "Ristoranti", new[] { "ristorante", "restaurant", "mcdonald", "burger", "pizza", "poke", "sushi", "trattoria", "osteria", "bar ", "caffè", "just eat", "glovo", "uber eats", "deliveroo" } },
        { "Trasporti", new[] { "benzina", "eni station", "q8", "esso", "ip matic", "tamoil", "autostrade", "telepass", "trenitalia", "italo", "uber ", "taxi", "atm ", "atac " } },
        { "Shopping", new[] { "amazon", "paypal", "zalando", "hm ", "zara", "nike", "decathlon", "mediaworld", "unieuro", "apple" } },
        { "Utenze", new[] { "enel", "a2a", "iren", "hera", "telecom", "tim ", "vodafone", "wind", "iliad", "fastweb", "sorgenia" } },
        { "Casa", new[] { "leroy merlin", "ikea", "mondo convenienza", "tecnocasa", "affitto", "condominio" } },
        { "Abbonamenti", new[] { "netflix", "spotify", "youtube", "google", "apple", "prime", "disney", "dazn", "sky" } },
        { "Salute", new[] { "farmacia", "medico", "ospedale", "cup ", "dentista" } }
    };

    public CsvImportService(IBudgetService budgetService, ICategoryRuleService categoryRuleService)
    {
        _budgetService = budgetService;
        _categoryRuleService = categoryRuleService;
    }

    public Task<List<string[]>> PreviewCsvAsync(Stream content, int linesToRead = 5)
    {
        var preview = new List<string[]>();
        content.Position = 0;
        
        using var reader = new StreamReader(content, leaveOpen: true);
        
        // Detect delimiter from first line
        string? firstLine = reader.ReadLine();
        if (firstLine == null) return Task.FromResult(preview);
        
        char delimiter = DetectDelimiter(firstLine);
        
        // Reset and read
        content.Position = 0;
        using var reader2 = new StreamReader(content, leaveOpen: true); // New reader for reset stream
        
        int count = 0;
        while (!reader2.EndOfStream && count < linesToRead)
        {
            var line = reader2.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
            {
                var fields = ParseCsvLine(line, delimiter);
                preview.Add(fields);
                count++;
            }
        }
        
        return Task.FromResult(preview);
    }

    public Task<List<ImportedTransaction>> ParseCsvAsync(Stream content, CsvColumnMapping mapping)
    {
        var transactions = new List<ImportedTransaction>();
        content.Position = 0;
        
        using var reader = new StreamReader(content, leaveOpen: true);
        
        // Detect delimiter again or reuse logic? Let's detect.
        string? firstLine = reader.ReadLine();
        if (firstLine == null) return Task.FromResult(transactions);
        char delimiter = DetectDelimiter(firstLine);
        
        // Skip header if needed
        content.Position = 0;
        using var reader2 = new StreamReader(content, leaveOpen: true);
        
        if (mapping.HasHeaderRow && !reader2.EndOfStream)
        {
            reader2.ReadLine();
        }

        var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        culture.NumberFormat.NumberDecimalSeparator = mapping.DecimalSeparator;

        while (!reader2.EndOfStream)
        {
            try
            {
                string? line = reader2.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                string[] fields = ParseCsvLine(line, delimiter);
                
                // Validate indices
                if (mapping.DateIndex >= fields.Length || 
                    mapping.AmountIndex >= fields.Length || 
                    mapping.DescriptionIndex >= fields.Length) continue;

                var tx = new ImportedTransaction();
                
                // Parse Date - try strict format first, then loose
                string dateStr = fields[mapping.DateIndex].Trim('"').Trim(); // Remove loose quotes
                if (DateOnly.TryParseExact(dateStr, mapping.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    tx.Date = date;
                }
                else if (DateOnly.TryParse(dateStr, out var dateAuto))
                {
                    tx.Date = dateAuto;
                }
                else continue;

                // Parse Amount
                string amountStr = fields[mapping.AmountIndex].Replace("€", "").Replace("$", "").Trim('"').Trim();
                
                // If DecimalSep is ',', then '.' might be thousands or ignored.
                if (mapping.DecimalSeparator == ",") amountStr = amountStr.Replace(".", ""); 
                else amountStr = amountStr.Replace(",", ""); 
                
                if (decimal.TryParse(amountStr, NumberStyles.Any, culture, out var amount))
                {
                    tx.Amount = amount; 
                }
                else continue;

                // Parse Description
                tx.Description = fields[mapping.DescriptionIndex].Trim('"').Trim();

                // Optional Category
                if (mapping.CategoryIndex >= 0 && mapping.CategoryIndex < fields.Length)
                {
                    tx.RawCategory = fields[mapping.CategoryIndex].Trim('"').Trim();
                }

                transactions.Add(tx);
            }
            catch
            {
                // Skip broken lines
            }
        }

        return Task.FromResult(transactions);
    }

    public async Task AutoCategorizeAsync(List<ImportedTransaction> transactions, int familyId)
    {
        var categories = await _budgetService.GetCategoriesAsync(familyId);
        
        foreach (var tx in transactions)
        {
            if (tx.SuggestedCategoryId.HasValue) continue;

            string descLower = tx.Description.ToLowerInvariant();
            
            // 1. Try match by RawCategory from bank CSV
            if (!string.IsNullOrEmpty(tx.RawCategory))
            {
                // Filter categories by direction (Income vs Expense)
                var candidates = tx.Amount > 0 
                    ? categories.Where(c => c.Type == CategoryType.Income) 
                    : categories.Where(c => c.Type == CategoryType.Expense);

                var cat = candidates.FirstOrDefault(c => c.Name.Equals(tx.RawCategory, StringComparison.OrdinalIgnoreCase));
                if (cat != null)
                {
                    tx.SuggestedCategoryId = cat.Id;
                    tx.SuggestedCategoryName = cat.Name;
                    tx.ConfidenceScore = 100;
                    continue;
                }
            }

            // 2. Try LEARNED RULES (user's previous categorizations) - NEW!
            var learnedMatch = await _categoryRuleService.FindMatchingCategoryAsync(familyId, tx.Description);
            if (learnedMatch.HasValue && learnedMatch.Value.CategoryId.HasValue)
            {
                var cat = categories.FirstOrDefault(c => c.Id == learnedMatch.Value.CategoryId.Value);
                if (cat != null)
                {
                    tx.SuggestedCategoryId = cat.Id;
                    tx.SuggestedCategoryName = cat.Name;
                    tx.ConfidenceScore = learnedMatch.Value.Confidence;
                    tx.IsLearnedRule = true; // Mark as learned
                    continue;
                }
            }

            // 3. Fallback: Hardcoded keyword heuristic
            // Note: _keywordRules are mostly for Expenses. Should we enable Income keywords?
            // For now, only run keywords if explicit match found, but check category type.
            
            // Only try keyword matching for Expenses (negative amounts) for now, unless we add Income keywords
            if (tx.Amount < 0)
            {
                foreach (var rule in _keywordRules)
                {
                    if (rule.Value.Any(k => descLower.Contains(k)))
                    {
                        var cat = categories.FirstOrDefault(c => c.Name.Equals(rule.Key, StringComparison.OrdinalIgnoreCase) && c.Type == CategoryType.Expense);
                        if (cat != null)
                        {
                            tx.SuggestedCategoryId = cat.Id;
                            tx.SuggestedCategoryName = cat.Name;
                            tx.ConfidenceScore = 80;
                            break;
                        }
                    }
                }
            }
            else if (tx.Amount > 0)
            {
                // Simple Income heuristics
                if (descLower.Contains("stipendio") || descLower.Contains("salary") || descLower.Contains("emorumenti"))
                {
                    var cat = categories.FirstOrDefault(c => c.Type == CategoryType.Income && (c.Name.Contains("Stipendio") || c.Name.Contains("Salary")));
                    if (cat != null) { tx.SuggestedCategoryId = cat.Id; tx.SuggestedCategoryName = cat.Name; tx.ConfidenceScore = 90; }
                }
                else if (descLower.Contains("bonifico") || descLower.Contains("transfer"))
                {
                    // Generic transfer? hard to say without specific category
                    // Try to find "Altro" income
                     var cat = categories.FirstOrDefault(c => c.Type == CategoryType.Income && c.Name == "Altro");
                     if (cat != null) { tx.SuggestedCategoryId = cat.Id; tx.SuggestedCategoryName = cat.Name; tx.ConfidenceScore = 50; }
                }
            }
        }
    }
    
    // Helpers
    private char DetectDelimiter(string line)
    {
        if (line.Count(c => c == ';') > line.Count(c => c == ',')) return ';';
        if (line.Count(c => c == '\t') > line.Count(c => c == ',')) return '\t';
        return ',';
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
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"') // Escaped quote ""
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
