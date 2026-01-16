using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Models.Import;
using FamilyFinance.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Services;

public interface ITransactionMatchingService
{
    /// <summary>
    /// Analyzes imported transactions and matches them against recurring transactions and receivables
    /// </summary>
    Task MatchTransactionsAsync(List<ImportedTransaction> transactions, int familyId);
    
    /// <summary>
    /// Learns a recurring match from user's manual selection for future auto-matching
    /// </summary>
    Task LearnRecurringMatchAsync(int familyId, string description, int recurringId);
}

public class TransactionMatchingService : ITransactionMatchingService
{
    private readonly AppDbContext _db;
    private const decimal AmountTolerance = 0.1m; // 10% tolerance for amount matching
    
    public TransactionMatchingService(AppDbContext db)
    {
        _db = db;
    }
    
    public async Task MatchTransactionsAsync(List<ImportedTransaction> transactions, int familyId)
    {
        // Load all active recurring transactions for the family
        var recurringList = await _db.RecurringTransactions
            .Where(r => r.FamilyId == familyId && r.IsActive && !r.IsDeleted)
            .ToListAsync();
        
        // Load all open receivables from recent snapshots
        var recentSnapshots = await _db.Snapshots
            .Where(s => s.FamilyId == familyId)
            .OrderByDescending(s => s.SnapshotDate)
            .Take(3) // Last 3 snapshots
            .Select(s => s.Id)
            .ToListAsync();
        
        var openReceivables = await _db.Receivables
            .Where(r => recentSnapshots.Contains(r.SnapshotId) && r.Status == ReceivableStatus.Open)
            .ToListAsync();
        
        // Load learned recurring rules for this family
        var learnedRules = await _db.RecurringMatchRules
            .Where(r => r.FamilyId == familyId)
            .ToListAsync();
        
        foreach (var tx in transactions)
        {
            // Skip if already matched or zero amount
            if (tx.MatchType != TransactionMatchType.None || tx.Amount == 0)
                continue;
            
            var descLower = tx.Description.ToLowerInvariant();
            
            // === 0. FIRST: Check learned rules (highest priority) ===
            var learnedMatch = learnedRules
                .Where(r => descLower.Contains(r.Keyword.ToLowerInvariant()))
                .OrderByDescending(r => r.Keyword.Length) // Prefer longer matches
                .ThenByDescending(r => r.UsageCount)
                .FirstOrDefault();
            
            if (learnedMatch != null)
            {
                var recurring = recurringList.FirstOrDefault(r => r.Id == learnedMatch.RecurringTransactionId);
                if (recurring != null)
                {
                    tx.MatchType = TransactionMatchType.Recurring;
                    tx.MatchedRecurringId = recurring.Id;
                    tx.MatchedRecurringName = recurring.Name;
                    tx.MatchConfidence = 95; // Learned = high confidence
                    tx.IsMatchConfirmed = true; // Auto-confirm learned matches
                    
                    if (recurring.CategoryId.HasValue && !tx.SuggestedCategoryId.HasValue)
                    {
                        tx.SuggestedCategoryId = recurring.CategoryId;
                    }
                    continue;
                }
            }
            
            // === 1. Try matching recurring transactions by keywords ===
            // For expenses (negative amounts), look for expense recurring
            // For income (positive amounts), look for income recurring
            var txType = tx.Amount < 0 ? TransactionType.Expense : TransactionType.Income;
            var txAmountAbs = Math.Abs(tx.Amount);
            
            foreach (var recurring in recurringList.Where(r => r.Type == txType))
            {
                var nameLower = recurring.Name.ToLowerInvariant();
                var amountMatch = IsAmountSimilar(txAmountAbs, recurring.Amount);
                var descMatch = DescriptionContainsKeywords(descLower, nameLower);
                
                if (descMatch && amountMatch)
                {
                    tx.MatchType = TransactionMatchType.Recurring;
                    tx.MatchedRecurringId = recurring.Id;
                    tx.MatchedRecurringName = recurring.Name;
                    tx.MatchConfidence = 90;
                    
                    // Also suggest the recurring's category if available
                    if (recurring.CategoryId.HasValue && !tx.SuggestedCategoryId.HasValue)
                    {
                        tx.SuggestedCategoryId = recurring.CategoryId;
                    }
                    break;
                }
                else if (descMatch)
                {
                    // Partial match on description only
                    tx.MatchType = TransactionMatchType.Recurring;
                    tx.MatchedRecurringId = recurring.Id;
                    tx.MatchedRecurringName = recurring.Name;
                    tx.MatchConfidence = 60;
                    break;
                }
            }
            
            // === 2. Try matching receivables (credit collections) ===
            // Only for positive amounts (incoming money)
            if (tx.MatchType == TransactionMatchType.None && tx.Amount > 0)
            {
                foreach (var receivable in openReceivables)
                {
                    var receivableDescLower = receivable.Description.ToLowerInvariant();
                    var amountMatch = IsAmountSimilar(tx.Amount, receivable.Amount);
                    
                    // Check if description contains keywords from receivable
                    var keywords = ExtractKeywords(receivableDescLower);
                    var descMatch = keywords.Any(k => descLower.Contains(k));
                    
                    if (amountMatch && descMatch)
                    {
                        tx.MatchType = TransactionMatchType.Receivable;
                        tx.MatchedReceivableId = receivable.Id;
                        tx.MatchedReceivableDesc = receivable.Description;
                        tx.MatchConfidence = 85;
                        break;
                    }
                    else if (amountMatch)
                    {
                        // Partial match on amount only
                        tx.MatchType = TransactionMatchType.Receivable;
                        tx.MatchedReceivableId = receivable.Id;
                        tx.MatchedReceivableDesc = receivable.Description;
                        tx.MatchConfidence = 50;
                        break;
                    }
                }
            }
        }
    }
    
    private bool IsAmountSimilar(decimal amount1, decimal amount2)
    {
        if (amount2 == 0) return false;
        var diff = Math.Abs(amount1 - amount2) / amount2;
        return diff <= AmountTolerance;
    }
    
    private bool DescriptionContainsKeywords(string description, string searchName)
    {
        // Split search name into words and check if any appear in description
        var keywords = ExtractKeywords(searchName);
        return keywords.Any(k => description.Contains(k));
    }
    
    private string[] ExtractKeywords(string text)
    {
        // Extract meaningful keywords (3+ chars, not common words)
        var commonWords = new HashSet<string> { "per", "del", "con", "alla", "dalla", "the", "for", "from" };
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length >= 3 && !commonWords.Contains(w))
            .ToArray();
    }
    
    /// <summary>
    /// Learns a recurring match from user's manual selection for future auto-matching
    /// </summary>
    public async Task LearnRecurringMatchAsync(int familyId, string description, int recurringId)
    {
        // Extract a meaningful keyword from the description
        var descLower = description.ToLowerInvariant();
        var keywords = ExtractKeywords(descLower);
        
        // Take the longest keyword as the most specific identifier
        var keyword = keywords.OrderByDescending(k => k.Length).FirstOrDefault();
        if (string.IsNullOrEmpty(keyword)) keyword = descLower;
        
        // Check if rule already exists
        var existingRule = await _db.RecurringMatchRules
            .FirstOrDefaultAsync(r => r.FamilyId == familyId && r.Keyword == keyword);
        
        if (existingRule != null)
        {
            // Update existing rule
            existingRule.RecurringTransactionId = recurringId;
            existingRule.UsageCount++;
        }
        else
        {
            // Create new rule
            _db.RecurringMatchRules.Add(new RecurringMatchRule
            {
                FamilyId = familyId,
                RecurringTransactionId = recurringId,
                Keyword = keyword,
                UsageCount = 1
            });
        }
        
        await _db.SaveChangesAsync();
    }
}

