using FamilyFinance.Services.Interfaces;
using FamilyFinance.Models;

namespace FamilyFinance.Services;

public class InsightService : IInsightService
{
    private readonly ISnapshotService _snapshotService;
    private readonly IBudgetService _budgetService;
    private readonly IGoalService _goalService;

    public InsightService(
        ISnapshotService snapshotService,
        IBudgetService budgetService,
        IGoalService goalService)
    {
        _snapshotService = snapshotService;
        _budgetService = budgetService;
        _goalService = goalService;
    }

    public async Task<List<Insight>> GetInsightsAsync(int familyId, decimal? currentLiveTotal = null)
    {
        var insights = new List<Insight>();
        
        try
        {
            // Get snapshots for comparison
            var snapshots = await _snapshotService.GetAllWithTotalsAsync(familyId);
            if (snapshots.Count == 0) return insights;
            
            var latest = snapshots.OrderByDescending(s => s.Date).FirstOrDefault();
            
            if (latest == null) return insights;

            // 1. Wealth comparison
            // If we have a LIVE total, we compare: LIVE vs LATEST SNAPSHOT
            // If we assume "latest" is a closed month, then Live represents "Month to Date" progress
            
            decimal change = 0;
            decimal changePercent = 0;
            bool hasComparison = false;

            if (currentLiveTotal.HasValue)
            {
                // Live comparison: Today vs Last Closed Snapshot
                change = currentLiveTotal.Value - latest.GrandTotal;
                changePercent = latest.GrandTotal != 0 
                    ? Math.Round((change / latest.GrandTotal) * 100, 1) 
                    : 0;
                hasComparison = true;
            }
            else
            {
                // Fallback: Latest Snapshot vs Previous Snapshot
                var previous = snapshots.OrderByDescending(s => s.Date).Skip(1).FirstOrDefault();
                if (previous != null)
                {
                    change = latest.CurrentTotal - previous.CurrentTotal;
                    changePercent = previous.CurrentTotal != 0 
                        ? Math.Round((change / previous.CurrentTotal) * 100, 1) 
                        : 0;
                    hasComparison = true;
                }
            }
            
            if (hasComparison)
            {
                if (change > 0)
                {
                    insights.Add(new Insight(
                        InsightType.Celebration,
                        "InsightWealthUp",
                        "InsightWealthUpMsg",
                        new object[] { changePercent }
                    ));
                }
                else if (change < 0 && Math.Abs(changePercent) > 5)
                {
                    insights.Add(new Insight(
                        InsightType.Warning,
                        "InsightWealthDown",
                        "InsightWealthDownMsg",
                        new object[] { Math.Abs(changePercent) }
                    ));
                }
            }

            // 2. Budget alerts
            var latestSnapshot = await _snapshotService.GetByIdAsync(latest.Id);
            if (latestSnapshot != null)
            {
                var budgetSummary = await _budgetService.GetSummaryAsync(latest.Id, familyId);
                var dayOfMonth = DateTime.Now.Day;
                var daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                var monthProgress = (decimal)dayOfMonth / daysInMonth * 100;

                // Check for overspending categories
                foreach (var cat in budgetSummary.Categories.Where(c => c.PercentUsed > 80 && c.Budget > 0))
                {
                    if (cat.PercentUsed > 100)
                    {
                        insights.Add(new Insight(
                            InsightType.Warning,
                            "InsightBudgetExceeded",
                            "InsightBudgetExceededMsg",
                            new object[] { cat.Name, Math.Round(cat.PercentUsed - 100, 0) },
                            "/budget"
                        ));
                    }
                    else if (cat.PercentUsed > monthProgress + 15) // Spending faster than time
                    {
                        insights.Add(new Insight(
                            InsightType.Warning,
                            "InsightBudgetWarning",
                            "InsightBudgetWarningMsg",
                            new object[] { cat.Name, Math.Round(cat.PercentUsed, 0), dayOfMonth },
                            "/budget"
                        ));
                    }
                }

                // Overall budget health
                if (budgetSummary.PercentUsed < 50 && dayOfMonth > 20)
                {
                    insights.Add(new Insight(
                        InsightType.Celebration,
                        "InsightBudgetHealthy",
                        "InsightBudgetHealthyMsg",
                        new object[] { Math.Round(100 - budgetSummary.PercentUsed, 0) }
                    ));
                }
            }

            // 3. Goal progress
            var goals = await _goalService.GetAllAsync(familyId);
            foreach (var goal in goals.Where(g => !g.IsCompleted).OrderBy(g => g.Deadline))
            {
                if (goal.ProgressPercent >= 90)
                {
                    insights.Add(new Insight(
                        InsightType.Goal,
                        "InsightGoalAlmostDone",
                        "InsightGoalAlmostDoneMsg",
                        new object[] { goal.Name, Math.Round(goal.Target - goal.AllocatedAmount, 0) },
                        "/goals"
                    ));
                }
                else if (goal.Deadline.HasValue && goal.Deadline.Value <= DateOnly.FromDateTime(DateTime.Now.AddMonths(1)))
                {
                    var remaining = goal.Target - goal.AllocatedAmount;
                    insights.Add(new Insight(
                        InsightType.Warning,
                        "InsightGoalDeadline",
                        "InsightGoalDeadlineMsg",
                        new object[] { goal.Name, remaining },
                        "/goals"
                    ));
                }
            }

            // Achievement celebration
            var achievedRecently = goals.Where(g => g.IsCompleted).OrderByDescending(g => g.Id).Take(1).FirstOrDefault();
            if (achievedRecently != null)
            {
                insights.Add(new Insight(
                    InsightType.Celebration,
                    "InsightGoalAchieved",
                    "InsightGoalAchievedMsg",
                    new object[] { achievedRecently.Name },
                    "/goals"
                ));
            }

            // 4. Tip - accounts with 0% interest
            var accounts = await GetAccountsWithZeroInterest(latestSnapshot);
            if (accounts > 0)
            {
                insights.Add(new Insight(
                    InsightType.Tip,
                    "InsightZeroInterest",
                    "InsightZeroInterestMsg",
                    new object[] { accounts },
                    "/accounts"
                ));
            }
        }
        catch (Exception)
        {
            // Log or ignore
        }

        return insights.Take(4).ToList(); // Limit to 4 insights
    }

    private Task<int> GetAccountsWithZeroInterest(Snapshot? snapshot)
    {
        if (snapshot == null) return Task.FromResult(0);
        
        var count = snapshot.Lines?
            .Where(a => a.Account != null && 
                       a.Account.Category == AccountCategory.Liquidity && 
                       a.Amount > 1000 &&
                       !a.Account.IsInterest)
            .Count() ?? 0;
            
        return Task.FromResult(count);
    }
}
