using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace FamilyFinance.Services;

public class CategoryRuleService : ICategoryRuleService
{
    private readonly AppDbContext _db;
    private readonly ILogger<CategoryRuleService> _logger;
    
    // Common words to ignore when extracting keywords
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "srl", "spa", "ltd", "inc", "gmbh", "sarl", "sa", "ag",
        "pagamento", "payment", "bonifico", "transfer", "pos",
        "carta", "card", "visa", "mastercard", "maestro",
        "del", "di", "da", "in", "con", "per", "the", "of", "to", "at",
        "eu", "it", "com", "www", "http", "https"
    };

    public CategoryRuleService(AppDbContext db, ILogger<CategoryRuleService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<CategoryRule>> GetRulesAsync(int familyId)
    {
        return await _db.CategoryRules
            .Where(r => r.FamilyId == familyId && !r.IsDeleted)
            .OrderByDescending(r => r.UsageCount)
            .ToListAsync();
    }

    public async Task LearnFromCategorizationAsync(int familyId, string description, int categoryId)
    {
        var keywords = ExtractKeywords(description);
        
        foreach (var keyword in keywords)
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 3) continue;
            
            var existingRule = await _db.CategoryRules
                .FirstOrDefaultAsync(r => r.FamilyId == familyId && 
                                          r.Keyword == keyword && 
                                          !r.IsDeleted);
            
            if (existingRule != null)
            {
                // Update existing rule - if same category, increment usage
                if (existingRule.CategoryId == categoryId)
                {
                    existingRule.UsageCount++;
                    existingRule.UpdatedAt = DateTime.UtcNow;
                }
                else if (existingRule.UsageCount <= 1)
                {
                    // If different category and low usage, update the category
                    existingRule.CategoryId = categoryId;
                    existingRule.UsageCount = 1;
                    existingRule.UpdatedAt = DateTime.UtcNow;
                }
                // If high usage with different category, don't override
            }
            else
            {
                // Create new rule
                var rule = new CategoryRule
                {
                    FamilyId = familyId,
                    CategoryId = categoryId,
                    Keyword = keyword,
                    UsageCount = 1,
                    CreatedAt = DateTime.UtcNow
                };
                _db.CategoryRules.Add(rule);
            }
        }
        
        await _db.SaveChangesAsync();
        _logger.LogDebug("Learned {Count} keyword(s) from '{Description}' → Category {CategoryId}", 
            keywords.Count, description, categoryId);
    }

    public async Task<(int? CategoryId, int Confidence)?> FindMatchingCategoryAsync(int familyId, string description)
    {
        var keywords = ExtractKeywords(description);
        if (!keywords.Any()) return null;
        
        // Find rules that match any of the extracted keywords
        var matchingRules = await _db.CategoryRules
            .Where(r => r.FamilyId == familyId && 
                        !r.IsDeleted &&
                        keywords.Contains(r.Keyword))
            .OrderByDescending(r => r.UsageCount)
            .ToListAsync();
        
        if (!matchingRules.Any()) return null;
        
        // Return the best match (highest usage count)
        var bestMatch = matchingRules.First();
        
        // Confidence based on usage count: 1 use = 85%, 2+ uses = 90%, 5+ uses = 95%
        int confidence = bestMatch.UsageCount switch
        {
            >= 5 => 95,
            >= 2 => 90,
            _ => 85
        };
        
        return (bestMatch.CategoryId, confidence);
    }

    /// <summary>
    /// Extract meaningful keywords from a transaction description.
    /// Returns lowercase keywords that are likely to identify the merchant/payee.
    /// </summary>
    private List<string> ExtractKeywords(string description)
    {
        if (string.IsNullOrWhiteSpace(description)) return new List<string>();
        
        // Clean and normalize
        var cleaned = description.ToLowerInvariant();
        
        // Remove common patterns: dates, amounts, card numbers
        cleaned = Regex.Replace(cleaned, @"\d{2}/\d{2}/\d{2,4}", ""); // dates
        cleaned = Regex.Replace(cleaned, @"\d+[,\.]\d{2}", "");       // amounts
        cleaned = Regex.Replace(cleaned, @"\*{4}\d{4}", "");          // card suffixes
        cleaned = Regex.Replace(cleaned, @"[^\w\s]", " ");            // punctuation
        
        // Split and filter
        var tokens = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length >= 3)
            .Where(t => !StopWords.Contains(t))
            .Where(t => !Regex.IsMatch(t, @"^\d+$")) // no pure numbers
            .Distinct()
            .Take(3) // Max 3 keywords per description
            .ToList();
        
        return tokens;
    }
}
