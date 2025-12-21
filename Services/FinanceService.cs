using FamilyFinance.Data;
using FamilyFinance.Models;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Services;

public class FinanceService
{
    private readonly AppDbContext _db;
    private readonly LogService _log;
    
    public FinanceService(AppDbContext db, LogService log)
    {
        _db = db;
        _log = log;
    }

    // Snapshots
    public async Task<List<Snapshot>> GetSnapshotsAsync()
        => await _db.Snapshots.OrderByDescending(s => s.SnapshotDate).ToListAsync();

    public async Task<Snapshot?> GetSnapshotAsync(int id)
        => await _db.Snapshots
            .Include(s => s.Lines).ThenInclude(l => l.Account)
            .Include(s => s.Investments).ThenInclude(i => i.Portfolio)
            .Include(s => s.Receivables)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Snapshot?> GetLatestSnapshotAsync()
        => await _db.Snapshots
            .Include(s => s.Lines).ThenInclude(l => l.Account)
            .Include(s => s.Investments).ThenInclude(i => i.Portfolio)
            .Include(s => s.Receivables)
            .OrderByDescending(s => s.SnapshotDate)
            .FirstOrDefaultAsync();

    public async Task<List<Account>> GetAccountsAsync() => await _db.Accounts.OrderBy(a => a.Category).ThenBy(a => a.Name).ToListAsync();
    
    public async Task<List<Account>> GetActiveAccountsAsync()
        => await _db.Accounts.Where(a => a.IsActive).OrderBy(a => a.Category).ThenBy(a => a.Name).ToListAsync();

    public Task<Totals> CalculateTotalsAsync(Snapshot snapshot)
    {
        var lines = snapshot.Lines;
        var liquidity = lines.Where(l => l.Account.Category == AccountCategory.Liquidity).Sum(l => l.Amount);
        var interestLiquidity = lines.Where(l => l.Account.Category == AccountCategory.Liquidity && l.Account.IsInterest).Sum(l => l.Amount);
        
        // Pension & Insurance with performance tracking
        var pensionLines = lines.Where(l => l.Account.Category is AccountCategory.Pension or AccountCategory.Insurance);
        var pensionInsuranceValue = pensionLines.Sum(l => l.Amount);
        var pensionInsuranceContrib = pensionLines.Sum(l => l.ContributionBasis);
        var pensionInsuranceGainLoss = pensionInsuranceValue - pensionInsuranceContrib;
        
        // Investments
        var investmentsValue = snapshot.Investments.Sum(i => i.Value);
        var investmentsCost = snapshot.Investments.Sum(i => i.CostBasis);
        var investmentsGainLoss = investmentsValue - investmentsCost;
        
        var creditsOpen = snapshot.Receivables.Where(r => r.Status == ReceivableStatus.Open).Sum(r => r.Amount);

        var currentTotal = liquidity + investmentsValue;
        return Task.FromResult(new Totals(
            liquidity, investmentsValue, investmentsCost, investmentsGainLoss, 
            creditsOpen, pensionInsuranceValue, pensionInsuranceContrib, pensionInsuranceGainLoss,
            currentTotal, currentTotal + creditsOpen, currentTotal + creditsOpen + pensionInsuranceValue, 
            interestLiquidity));
    }

    public async Task<int> SaveSnapshotAsync(int? snapshotId, DateOnly date, List<(int AccountId, decimal Amount, decimal ContributionBasis)> accountAmounts,
        List<(string Name, decimal CostBasis, decimal Value, int? PortfolioId)> investments,
        List<(string Description, decimal Amount, ReceivableStatus Status, DateOnly? ExpectedDate)> receivables)
    {
        Snapshot snapshot;
        if (snapshotId is null) {
            snapshot = new Snapshot { SnapshotDate = date };
            _db.Snapshots.Add(snapshot);
            await _db.SaveChangesAsync(); // Save to get ID
        } else {
            snapshot = await _db.Snapshots.Include(s => s.Lines).Include(s => s.Investments).Include(s => s.Receivables).FirstAsync(s => s.Id == snapshotId.Value);
            snapshot.SnapshotDate = date;
        }

        // Upsert Lines
        foreach (var (accountId, amount, contribBasis) in accountAmounts) {
            var existing = snapshot.Lines.FirstOrDefault(l => l.AccountId == accountId);
            if (existing is null) snapshot.Lines.Add(new SnapshotLine { SnapshotId = snapshot.Id, AccountId = accountId, Amount = amount, ContributionBasis = contribBasis });
            else { existing.Amount = amount; existing.ContributionBasis = contribBasis; }
        }

        // Replace Investments & Receivables
        _db.InvestmentAssets.RemoveRange(snapshot.Investments);
        snapshot.Investments = investments.Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => new InvestmentAsset { 
                SnapshotId = snapshot.Id, 
                Broker = "Directa", 
                Name = x.Name.Trim(), 
                CostBasis = x.CostBasis, 
                Value = x.Value,
                PortfolioId = x.PortfolioId
            }).ToList();

        _db.Receivables.RemoveRange(snapshot.Receivables);
        snapshot.Receivables = receivables.Where(r => !string.IsNullOrWhiteSpace(r.Description))
            .Select(r => new Receivable { SnapshotId = snapshot.Id, Description = r.Description.Trim(), Amount = r.Amount, Status = r.Status, ExpectedDate = r.ExpectedDate }).ToList();

        await _db.SaveChangesAsync();
        return snapshot.Id;
    }

    public async Task DeleteSnapshotAsync(int id) {
        var s = await _db.Snapshots.Include(x=>x.Lines).Include(x=>x.Investments).Include(x=>x.Receivables).FirstOrDefaultAsync(x=>x.Id == id);
        if(s != null) { _db.Snapshots.Remove(s); await _db.SaveChangesAsync(); }
    }

    // Portfolios
    public async Task<List<Portfolio>> GetPortfoliosAsync()
        => await _db.Portfolios.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();

    public async Task<Portfolio?> GetPortfolioAsync(int id)
        => await _db.Portfolios.FirstOrDefaultAsync(p => p.Id == id);

    public async Task SavePortfolioAsync(Portfolio portfolio)
    {
        if (portfolio.Id == 0)
        {
            portfolio.CreatedAt = DateTime.UtcNow;
            _db.Portfolios.Add(portfolio);
        }
        else
        {
            _db.Portfolios.Update(portfolio);
        }
        await _db.SaveChangesAsync();
    }

    public async Task DeletePortfolioAsync(int id)
    {
        var p = await _db.Portfolios.FirstOrDefaultAsync(x => x.Id == id);
        if (p != null)
        {
            p.IsActive = false; // Soft delete
            await _db.SaveChangesAsync();
        }
    }

    // Goals
    public async Task<List<Goal>> GetGoalsAsync() => await _db.Goals.OrderByDescending(g => g.Id).ToListAsync();
    public async Task SaveGoalAsync(Goal goal) {
        if (goal.Id == 0) { 
            goal.Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); 
            _db.Goals.Add(goal); 
        }
        else {
            var existing = await _db.Goals.FindAsync(goal.Id);
            if (existing != null) {
                existing.Name = goal.Name;
                existing.Target = goal.Target;
                existing.AllocatedAmount = goal.AllocatedAmount;
                existing.Deadline = goal.Deadline;
                existing.Priority = goal.Priority;
                existing.Category = goal.Category;
                existing.ShowMonthlyTarget = goal.ShowMonthlyTarget;
            }
        }
        await _db.SaveChangesAsync();
    }
    public async Task DeleteGoalAsync(long id) {
        var g = await _db.Goals.FirstOrDefaultAsync(x=>x.Id==id); if(g!=null){_db.Goals.Remove(g); await _db.SaveChangesAsync();}
    }

    // Import functionality
    public async Task<ImportPreview> PreviewImportAsync(BackupDto backup)
    {
        var preview = new ImportPreview
        {
            IsValid = true,
            ExportDate = backup.ExportDate,
            Version = backup.Version,
            Data = backup
        };

        try
        {
            // Get existing data for comparison
            var existingAccounts = await _db.Accounts.ToListAsync();
            var existingPortfolios = await _db.Portfolios.ToListAsync();
            var existingGoals = await _db.Goals.ToListAsync();
            var existingSnapshots = await _db.Snapshots.ToListAsync();

            // Accounts - match by name (case-insensitive)
            preview.TotalAccounts = backup.Accounts.Count;
            foreach (var acc in backup.Accounts)
            {
                if (existingAccounts.Any(e => e.Name.Equals(acc.Name, StringComparison.OrdinalIgnoreCase)))
                    preview.ExistingAccounts++;
                else
                    preview.NewAccounts++;
            }

            // Portfolios - match by name
            preview.TotalPortfolios = backup.Portfolios.Count;
            foreach (var p in backup.Portfolios)
            {
                if (existingPortfolios.Any(e => e.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                    preview.ExistingPortfolios++;
                else
                    preview.NewPortfolios++;
            }

            // Goals - match by name
            preview.TotalGoals = backup.Goals.Count;
            foreach (var g in backup.Goals)
            {
                if (existingGoals.Any(e => e.Name.Equals(g.Name, StringComparison.OrdinalIgnoreCase)))
                    preview.ExistingGoals++;
                else
                    preview.NewGoals++;
            }

            // Snapshots - match by date
            preview.TotalSnapshots = backup.Snapshots.Count;
            foreach (var s in backup.Snapshots)
            {
                if (DateOnly.TryParse(s.Date, out var date) && existingSnapshots.Any(e => e.SnapshotDate == date))
                    preview.ExistingSnapshots++;
                else
                    preview.NewSnapshots++;
            }

            // Budget Categories - match by name
            var existingBudgetCategories = await _db.BudgetCategories.ToListAsync();
            preview.TotalBudgetCategories = backup.BudgetCategories?.Count ?? 0;
            if (backup.BudgetCategories != null)
            {
                foreach (var bc in backup.BudgetCategories)
                {
                    if (existingBudgetCategories.Any(e => e.Name.Equals(bc.Name, StringComparison.OrdinalIgnoreCase)))
                        preview.ExistingBudgetCategories++;
                    else
                        preview.NewBudgetCategories++;
                }
            }
        }
        catch (Exception ex)
        {
            preview.IsValid = false;
            preview.Error = ex.Message;
        }

        return preview;
    }

    public async Task<ImportResult> ImportDataAsync(BackupDto backup, bool replaceExisting, int familyId)
    {
        var result = new ImportResult { Success = true };
        
        _log.LogImport($"=== Starting Import === Replace existing: {replaceExisting}, FamilyId: {familyId}");
        _log.LogImport($"Backup contains: {backup.Accounts.Count} accounts, {backup.Portfolios.Count} portfolios, {backup.Goals.Count} goals, {backup.Snapshots.Count} snapshots");

        try
        {
            // Build mapping dictionaries for IDs (old ID -> new ID)
            var accountIdMap = new Dictionary<int, int>();
            var portfolioIdMap = new Dictionary<int, int>();

            // 1. Import Accounts
            _log.LogImport("Step 1: Importing Accounts...");
            var existingAccounts = await _db.Accounts.ToListAsync();
            foreach (var accDto in backup.Accounts)
            {
                try
                {
                    _log.LogImport($"  Processing account: {accDto.Name} (Id={accDto.Id}, Category={accDto.Category})");
                    
                    var existing = existingAccounts.FirstOrDefault(a => a.Name.Equals(accDto.Name, StringComparison.OrdinalIgnoreCase));
                    
                    if (existing != null)
                    {
                        _log.LogImport($"    Found existing account with Id={existing.Id}");
                        accountIdMap[accDto.Id] = existing.Id;
                        
                        if (replaceExisting)
                        {
                            existing.Owner = accDto.Owner ?? "";
                            existing.IsInterest = accDto.IsInterest;
                            existing.IsActive = accDto.IsActive;
                            if (Enum.TryParse<AccountCategory>(accDto.Category, out var cat))
                                existing.Category = cat;
                            result.AccountsImported++;
                        }
                    }
                    else
                    {
                        _log.LogImport($"    Creating new account...");
                        var newAccount = new Account
                        {
                            Name = accDto.Name ?? "Unknown",
                            Owner = accDto.Owner ?? "",
                            IsInterest = accDto.IsInterest,
                            IsActive = accDto.IsActive,
                            Category = AccountCategory.Liquidity, // Default
                            FamilyId = familyId
                        };
                        if (Enum.TryParse<AccountCategory>(accDto.Category, out var cat))
                            newAccount.Category = cat;
                        
                        _db.Accounts.Add(newAccount);
                        await _db.SaveChangesAsync();
                        
                        accountIdMap[accDto.Id] = newAccount.Id;
                        result.AccountsImported++;
                        _log.LogImport($"    Created with new Id={newAccount.Id}");
                    }
                }
                catch (Exception ex)
                {
                    var innerMsg = ex.InnerException?.Message ?? ex.Message;
                    _log.LogError($"  Failed to import account '{accDto.Name}': {innerMsg}", ex, "Import");
                    throw; // Re-throw to stop the import
                }
            }

            _log.LogImport($"  Accounts imported: {result.AccountsImported}");

            // 2. Import Portfolios
            _log.LogImport("Step 2: Importing Portfolios...");
            var existingPortfolios = await _db.Portfolios.ToListAsync();
            foreach (var pDto in backup.Portfolios)
            {
                var existing = existingPortfolios.FirstOrDefault(p => p.Name.Equals(pDto.Name, StringComparison.OrdinalIgnoreCase));
                
                if (existing != null)
                {
                    portfolioIdMap[pDto.Id] = existing.Id;
                    
                    if (replaceExisting)
                    {
                        existing.Description = pDto.Description;
                        existing.TimeHorizonYears = pDto.TimeHorizonYears;
                        existing.TargetYear = pDto.TargetYear;
                        existing.Color = pDto.Color;
                        existing.IsActive = pDto.IsActive;
                        result.PortfoliosImported++;
                    }
                }
                else
                {
                    var newPortfolio = new Portfolio
                    {
                        Name = pDto.Name,
                        Description = pDto.Description,
                        TimeHorizonYears = pDto.TimeHorizonYears,
                        TargetYear = pDto.TargetYear,
                        Color = pDto.Color,
                        IsActive = pDto.IsActive,
                        CreatedAt = DateTime.UtcNow,
                        FamilyId = familyId
                    };
                    
                    _db.Portfolios.Add(newPortfolio);
                    await _db.SaveChangesAsync();
                    
                    portfolioIdMap[pDto.Id] = newPortfolio.Id;
                    result.PortfoliosImported++;
                }
            }

            _log.LogImport($"  Portfolios imported: {result.PortfoliosImported}");

            // 3. Import Goals
            _log.LogImport("Step 3: Importing Goals...");
            var existingGoals = await _db.Goals.ToListAsync();
            foreach (var gDto in backup.Goals)
            {
                var existing = existingGoals.FirstOrDefault(g => g.Name.Equals(gDto.Name, StringComparison.OrdinalIgnoreCase));
                
                if (existing != null)
                {
                    if (replaceExisting)
                    {
                        existing.Target = gDto.Target;
                        existing.AllocatedAmount = gDto.AllocatedAmount;
                        existing.Deadline = gDto.Deadline;
                        existing.ShowMonthlyTarget = gDto.ShowMonthlyTarget;
                        if (Enum.TryParse<GoalPriority>(gDto.Priority, out var pri))
                            existing.Priority = pri;
                        if (Enum.TryParse<GoalCategory>(gDto.Category, out var cat))
                            existing.Category = cat;
                        result.GoalsImported++;
                    }
                }
                else
                {
                    var newGoal = new Goal
                    {
                        Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + result.GoalsImported, // Unique ID
                        Name = gDto.Name,
                        Target = gDto.Target,
                        AllocatedAmount = gDto.AllocatedAmount,
                        Deadline = gDto.Deadline,
                        ShowMonthlyTarget = gDto.ShowMonthlyTarget,
                        FamilyId = familyId
                    };
                    if (Enum.TryParse<GoalPriority>(gDto.Priority, out var pri))
                        newGoal.Priority = pri;
                    if (Enum.TryParse<GoalCategory>(gDto.Category, out var cat))
                        newGoal.Category = cat;
                    
                    _db.Goals.Add(newGoal);
                    result.GoalsImported++;
                }
            }

            await _db.SaveChangesAsync();
            _log.LogImport($"  Goals imported: {result.GoalsImported}");

            // 4. Import Budget Categories
            var budgetCategoryIdMap = new Dictionary<int, int>();
            if (backup.BudgetCategories != null && backup.BudgetCategories.Count > 0)
            {
                _log.LogImport("Step 4: Importing Budget Categories...");
                var existingBudgetCategories = await _db.BudgetCategories.Where(c => c.FamilyId == familyId).ToListAsync();
                foreach (var bcDto in backup.BudgetCategories)
                {
                    var existing = existingBudgetCategories.FirstOrDefault(bc => bc.Name.Equals(bcDto.Name, StringComparison.OrdinalIgnoreCase));
                    
                    if (existing != null)
                    {
                        budgetCategoryIdMap[bcDto.Id] = existing.Id;
                        
                        if (replaceExisting)
                        {
                            existing.Icon = bcDto.Icon;
                            existing.Color = bcDto.Color;
                            existing.MonthlyBudget = bcDto.MonthlyBudget;
                            existing.IsActive = bcDto.IsActive;
                            result.BudgetCategoriesImported++;
                        }
                    }
                    else
                    {
                        var newCategory = new BudgetCategory
                        {
                            Name = bcDto.Name,
                            Icon = bcDto.Icon,
                            Color = bcDto.Color,
                            MonthlyBudget = bcDto.MonthlyBudget,
                            IsActive = bcDto.IsActive,
                            FamilyId = familyId
                        };
                        
                        _db.BudgetCategories.Add(newCategory);
                        await _db.SaveChangesAsync();
                        
                        budgetCategoryIdMap[bcDto.Id] = newCategory.Id;
                        result.BudgetCategoriesImported++;
                    }
                }
                await _db.SaveChangesAsync();
                _log.LogImport($"  Budget Categories imported: {result.BudgetCategoriesImported}");
            }

            // 5. Import Snapshots
            _log.LogImport("Step 5: Importing Snapshots...");
            var existingSnapshots = await _db.Snapshots.ToListAsync();
            foreach (var sDto in backup.Snapshots)
            {
                if (!DateOnly.TryParse(sDto.Date, out var snapshotDate))
                    continue;

                var existing = existingSnapshots.FirstOrDefault(s => s.SnapshotDate == snapshotDate);
                
                if (existing != null && !replaceExisting)
                    continue; // Skip existing snapshots if not replacing

                if (existing != null && replaceExisting)
                {
                    // Delete existing snapshot data
                    await DeleteSnapshotAsync(existing.Id);
                }

                // Create new snapshot
                var newSnapshot = new Snapshot { SnapshotDate = snapshotDate, FamilyId = familyId };
                _db.Snapshots.Add(newSnapshot);
                await _db.SaveChangesAsync();

                // Add lines
                foreach (var lineDto in sDto.Lines)
                {
                    var mappedAccountId = accountIdMap.GetValueOrDefault(lineDto.AccountId, lineDto.AccountId);
                    
                    // Try to find account by mapped ID or by name
                    var account = await _db.Accounts.FindAsync(mappedAccountId);
                    if (account == null && !string.IsNullOrEmpty(lineDto.AccountName))
                    {
                        account = await _db.Accounts.FirstOrDefaultAsync(a => a.Name == lineDto.AccountName);
                    }
                    
                    if (account != null)
                    {
                        newSnapshot.Lines.Add(new SnapshotLine
                        {
                            SnapshotId = newSnapshot.Id,
                            AccountId = account.Id,
                            Amount = lineDto.Amount,
                            ContributionBasis = lineDto.ContributionBasis
                        });
                    }
                }

                // Add investments
                foreach (var invDto in sDto.Investments)
                {
                    int? mappedPortfolioId = null;
                    if (invDto.PortfolioId.HasValue)
                    {
                        mappedPortfolioId = portfolioIdMap.GetValueOrDefault(invDto.PortfolioId.Value, invDto.PortfolioId.Value);
                        // Verify portfolio exists
                        if (!await _db.Portfolios.AnyAsync(p => p.Id == mappedPortfolioId))
                            mappedPortfolioId = null;
                    }

                    newSnapshot.Investments.Add(new InvestmentAsset
                    {
                        SnapshotId = newSnapshot.Id,
                        Name = invDto.Name,
                        CostBasis = invDto.CostBasis,
                        Value = invDto.Value,
                        PortfolioId = mappedPortfolioId,
                        Broker = "Directa"
                    });
                }

                // Add receivables
                foreach (var recDto in sDto.Receivables)
                {
                    var receivable = new Receivable
                    {
                        SnapshotId = newSnapshot.Id,
                        Description = recDto.Description,
                        Amount = recDto.Amount
                    };
                    if (Enum.TryParse<ReceivableStatus>(recDto.Status, out var status))
                        receivable.Status = status;
                    
                    newSnapshot.Receivables.Add(receivable);
                }

                // Add monthly expenses
                if (sDto.MonthlyExpenses != null)
                {
                    foreach (var expDto in sDto.MonthlyExpenses)
                    {
                        var mappedCategoryId = budgetCategoryIdMap.GetValueOrDefault(expDto.CategoryId, expDto.CategoryId);
                        
                        // Verify category exists
                        var category = await _db.BudgetCategories.FindAsync(mappedCategoryId);
                        if (category == null && !string.IsNullOrEmpty(expDto.CategoryName))
                        {
                            category = await _db.BudgetCategories.FirstOrDefaultAsync(c => c.Name == expDto.CategoryName && c.FamilyId == familyId);
                        }
                        
                        if (category != null)
                        {
                            newSnapshot.Expenses.Add(new MonthlyExpense
                            {
                                SnapshotId = newSnapshot.Id,
                                CategoryId = category.Id,
                                Amount = expDto.Amount
                            });
                        }
                    }
                }

                await _db.SaveChangesAsync();
                result.SnapshotsImported++;
            }
            
            _log.LogImport($"  Snapshots imported: {result.SnapshotsImported}");
            _log.LogImport("=== Import completed successfully ===");
        }
        catch (Exception ex)
        {
            result.Success = false;
            
            // Get the innermost exception for better error details
            var innerEx = ex;
            while (innerEx.InnerException != null)
                innerEx = innerEx.InnerException;
            
            result.Error = $"{ex.Message} - Inner: {innerEx.Message}";
            _log.LogError($"Import failed: {ex.Message}", ex, "Import");
            _log.LogError($"Inner exception: {innerEx.Message}", innerEx, "Import");
        }

        return result;
    }

    public async Task ClearAllDataAsync()
    {
        // Delete all data in correct order (respect FK constraints)
        _db.MonthlyExpenses.RemoveRange(_db.MonthlyExpenses);
        _db.BudgetCategories.RemoveRange(_db.BudgetCategories);
        _db.Receivables.RemoveRange(_db.Receivables);
        _db.InvestmentAssets.RemoveRange(_db.InvestmentAssets);
        _db.SnapshotLines.RemoveRange(_db.SnapshotLines);
        _db.Snapshots.RemoveRange(_db.Snapshots);
        _db.Goals.RemoveRange(_db.Goals);
        _db.Portfolios.RemoveRange(_db.Portfolios);
        _db.Accounts.RemoveRange(_db.Accounts);
        await _db.SaveChangesAsync();
    }

    // ========== Budget Categories ==========
    
    public async Task<List<BudgetCategory>> GetBudgetCategoriesAsync(int familyId)
        => await _db.BudgetCategories
            .Where(c => c.FamilyId == familyId && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

    public async Task<BudgetCategory?> GetBudgetCategoryAsync(int id)
        => await _db.BudgetCategories.FirstOrDefaultAsync(c => c.Id == id);

    public async Task SaveBudgetCategoryAsync(BudgetCategory category)
    {
        if (category.Id == 0)
        {
            // Set sort order to last
            var maxOrder = await _db.BudgetCategories
                .Where(c => c.FamilyId == category.FamilyId)
                .MaxAsync(c => (int?)c.SortOrder) ?? 0;
            category.SortOrder = maxOrder + 1;
            _db.BudgetCategories.Add(category);
        }
        else
        {
            var existing = await _db.BudgetCategories.FindAsync(category.Id);
            if (existing != null)
            {
                existing.Name = category.Name;
                existing.Icon = category.Icon;
                existing.Color = category.Color;
                existing.MonthlyBudget = category.MonthlyBudget;
                existing.SortOrder = category.SortOrder;
                existing.IsActive = category.IsActive;
            }
        }
        await _db.SaveChangesAsync();
    }

    public async Task DeleteBudgetCategoryAsync(int id)
    {
        var category = await _db.BudgetCategories.FirstOrDefaultAsync(c => c.Id == id);
        if (category != null)
        {
            category.IsActive = false; // Soft delete
            await _db.SaveChangesAsync();
        }
    }

    public List<BudgetCategory> GetDefaultBudgetCategories()
    {
        // Return a list of common budget categories for initial setup
        return new List<BudgetCategory>
        {
            new() { Name = "Casa/Affitto", Icon = "🏠", Color = "#6366f1", MonthlyBudget = 0 },
            new() { Name = "Alimentari", Icon = "🛒", Color = "#10b981", MonthlyBudget = 0 },
            new() { Name = "Trasporti", Icon = "🚗", Color = "#f59e0b", MonthlyBudget = 0 },
            new() { Name = "Utenze", Icon = "⚡", Color = "#ef4444", MonthlyBudget = 0 },
            new() { Name = "Svago", Icon = "🎭", Color = "#8b5cf6", MonthlyBudget = 0 },
            new() { Name = "Salute", Icon = "🏥", Color = "#ec4899", MonthlyBudget = 0 },
            new() { Name = "Abbigliamento", Icon = "👕", Color = "#06b6d4", MonthlyBudget = 0 },
            new() { Name = "Ristoranti", Icon = "🍕", Color = "#f97316", MonthlyBudget = 0 },
            new() { Name = "Abbonamenti", Icon = "📱", Color = "#14b8a6", MonthlyBudget = 0 },
            new() { Name = "Altro", Icon = "💰", Color = "#6b7280", MonthlyBudget = 0 }
        };
    }

    public async Task InitializeBudgetCategoriesAsync(int familyId)
    {
        // Check if family already has categories
        var existingCount = await _db.BudgetCategories.CountAsync(c => c.FamilyId == familyId);
        if (existingCount > 0) return;

        var defaults = GetDefaultBudgetCategories();
        var order = 1;
        foreach (var cat in defaults)
        {
            cat.FamilyId = familyId;
            cat.SortOrder = order++;
            _db.BudgetCategories.Add(cat);
        }
        await _db.SaveChangesAsync();
    }

    // ========== Monthly Expenses ==========

    public async Task<List<MonthlyExpense>> GetMonthlyExpensesAsync(int snapshotId)
        => await _db.MonthlyExpenses
            .Include(e => e.Category)
            .Where(e => e.SnapshotId == snapshotId)
            .OrderBy(e => e.Category!.SortOrder)
            .ToListAsync();

    public async Task SaveMonthlyExpensesAsync(int snapshotId, List<(int CategoryId, decimal Amount, string? Notes)> expenses)
    {
        // Get existing expenses for this snapshot
        var existing = await _db.MonthlyExpenses
            .Where(e => e.SnapshotId == snapshotId)
            .ToListAsync();

        foreach (var (categoryId, amount, notes) in expenses)
        {
            var expense = existing.FirstOrDefault(e => e.CategoryId == categoryId);
            if (expense != null)
            {
                expense.Amount = amount;
                expense.Notes = notes;
            }
            else if (amount > 0) // Only add if there's an amount
            {
                _db.MonthlyExpenses.Add(new MonthlyExpense
                {
                    SnapshotId = snapshotId,
                    CategoryId = categoryId,
                    Amount = amount,
                    Notes = notes
                });
            }
        }

        // Remove expenses with 0 amount
        var toRemove = existing.Where(e => expenses.Any(x => x.CategoryId == e.CategoryId && x.Amount == 0));
        _db.MonthlyExpenses.RemoveRange(toRemove);

        await _db.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalExpensesForSnapshotAsync(int snapshotId)
        => await _db.MonthlyExpenses
            .Where(e => e.SnapshotId == snapshotId)
            .SumAsync(e => e.Amount);

    public async Task<decimal> GetTotalBudgetAsync(int familyId)
        => await _db.BudgetCategories
            .Where(c => c.FamilyId == familyId && c.IsActive)
            .SumAsync(c => c.MonthlyBudget);

    public record BudgetSummary(
        decimal TotalBudget,
        decimal TotalSpent,
        decimal Difference,
        decimal PercentUsed,
        List<CategorySummary> Categories
    );

    public record CategorySummary(
        int Id,
        string Name,
        string Icon,
        string Color,
        decimal Budget,
        decimal Spent,
        decimal Difference,
        decimal PercentUsed,
        bool IsOverBudget
    );

    public async Task<BudgetSummary> GetBudgetSummaryAsync(int snapshotId, int familyId)
    {
        var categories = await _db.BudgetCategories
            .Where(c => c.FamilyId == familyId && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        var expenses = await _db.MonthlyExpenses
            .Where(e => e.SnapshotId == snapshotId)
            .ToListAsync();

        var categorySummaries = categories.Select(c =>
        {
            var spent = expenses.FirstOrDefault(e => e.CategoryId == c.Id)?.Amount ?? 0;
            var diff = c.MonthlyBudget - spent;
            var pct = c.MonthlyBudget > 0 ? (spent / c.MonthlyBudget) * 100 : 0;
            return new CategorySummary(
                c.Id, c.Name, c.Icon, c.Color, c.MonthlyBudget, spent, diff, pct, spent > c.MonthlyBudget && c.MonthlyBudget > 0
            );
        }).ToList();

        var totalBudget = categories.Sum(c => c.MonthlyBudget);
        var totalSpent = expenses.Sum(e => e.Amount);
        var totalDiff = totalBudget - totalSpent;
        var totalPct = totalBudget > 0 ? (totalSpent / totalBudget) * 100 : 0;

        return new BudgetSummary(totalBudget, totalSpent, totalDiff, totalPct, categorySummaries);
    }
}
