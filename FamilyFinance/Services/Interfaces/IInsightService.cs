namespace FamilyFinance.Services.Interfaces;

public enum InsightType
{
    Celebration,  // 🎉 Positive achievements
    Warning,      // ⚠️ Budget alerts
    Goal,         // 🎯 Goal progress
    Tip           // 💡 Helpful suggestions
}

public record Insight(
    InsightType Type,
    string TitleKey,      // Localization key for title
    string MessageKey,    // Localization key for message template
    object[]? MessageArgs, // Arguments for string.Format
    string? ActionUrl = null
);

public interface IInsightService
{
    Task<List<Insight>> GetInsightsAsync(int familyId, decimal? currentLiveTotal = null);
}
