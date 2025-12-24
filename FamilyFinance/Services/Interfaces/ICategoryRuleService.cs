using FamilyFinance.Models;

namespace FamilyFinance.Services.Interfaces;

public interface ICategoryRuleService
{
    /// <summary>
    /// Get all learned rules for a family
    /// </summary>
    Task<List<CategoryRule>> GetRulesAsync(int familyId);
    
    /// <summary>
    /// Learn a new categorization rule from user's manual choice.
    /// Extracts keywords from description and saves/updates rules.
    /// </summary>
    Task LearnFromCategorizationAsync(int familyId, string description, int categoryId);
    
    /// <summary>
    /// Find a matching category based on learned rules.
    /// Returns null if no rule matches.
    /// </summary>
    Task<(int? CategoryId, int Confidence)?> FindMatchingCategoryAsync(int familyId, string description);
}
