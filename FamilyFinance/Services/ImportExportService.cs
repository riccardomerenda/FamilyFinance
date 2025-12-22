using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Services;

public class ImportExportService : IImportExportService
{
    private readonly AppDbContext _db;
    private readonly LogService _log;
    private readonly ISnapshotService _snapshotService;

    public ImportExportService(AppDbContext db, LogService log, ISnapshotService snapshotService)
    {
        _db = db;
        _log = log;
        _snapshotService = snapshotService;
    }

    public async Task<ImportPreview> PreviewImportAsync(BackupDto backup, int familyId)
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
            var existingAccounts = await _db.Accounts.Where(a => a.FamilyId == familyId).ToListAsync();
            var existingPortfolios = await _db.Portfolios.Where(p => p.FamilyId == familyId).ToListAsync();
            var existingGoals = await _db.Goals.Where(g => g.FamilyId == familyId).ToListAsync();
            var existingSnapshots = await _db.Snapshots.Where(s => s.FamilyId == familyId).ToListAsync();

            preview.TotalAccounts = backup.Accounts.Count;
            foreach (var acc in backup.Accounts)
            {
                if (existingAccounts.Any(e => e.Name.Equals(acc.Name, StringComparison.OrdinalIgnoreCase)))
                    preview.ExistingAccounts++;
                else
                    preview.NewAccounts++;
            }

            preview.TotalPortfolios = backup.Portfolios.Count;
            foreach (var p in backup.Portfolios)
            {
                if (existingPortfolios.Any(e => e.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                    preview.ExistingPortfolios++;
                else
                    preview.NewPortfolios++;
            }

            preview.TotalGoals = backup.Goals.Count;
            foreach (var g in backup.Goals)
            {
                if (existingGoals.Any(e => e.Name.Equals(g.Name, StringComparison.OrdinalIgnoreCase)))
                    preview.ExistingGoals++;
                else
                    preview.NewGoals++;
            }

            preview.TotalSnapshots = backup.Snapshots.Count;
            foreach (var s in backup.Snapshots)
            {
                if (DateOnly.TryParse(s.Date, out var date) && existingSnapshots.Any(e => e.SnapshotDate == date))
                    preview.ExistingSnapshots++;
                else
                    preview.NewSnapshots++;
            }

            var existingBudgetCategories = await _db.BudgetCategories.Where(c => c.FamilyId == familyId).ToListAsync();
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
            var accountIdMap = new Dictionary<int, int>();
            var portfolioIdMap = new Dictionary<int, int>();

            // 1. Import Accounts
            _log.LogImport("Step 1: Importing Accounts...");
            var existingAccounts = await _db.Accounts.Where(a => a.FamilyId == familyId).ToListAsync();
            foreach (var accDto in backup.Accounts)
            {
                var existing = existingAccounts.FirstOrDefault(a => a.Name.Equals(accDto.Name, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
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
                    var newAccount = new Account
                    {
                        Name = accDto.Name ?? "Unknown",
                        Owner = accDto.Owner ?? "",
                        IsInterest = accDto.IsInterest,
                        IsActive = accDto.IsActive,
                        Category = AccountCategory.Liquidity,
                        FamilyId = familyId
                    };
                    if (Enum.TryParse<AccountCategory>(accDto.Category, out var cat))
                        newAccount.Category = cat;

                    _db.Accounts.Add(newAccount);
                    await _db.SaveChangesAsync();

                    accountIdMap[accDto.Id] = newAccount.Id;
                    result.AccountsImported++;
                }
            }
            _log.LogImport($"  Accounts imported: {result.AccountsImported}");

            // 2. Import Portfolios
            _log.LogImport("Step 2: Importing Portfolios...");
            var existingPortfolios = await _db.Portfolios.Where(p => p.FamilyId == familyId).ToListAsync();
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
            var existingGoals = await _db.Goals.Where(g => g.FamilyId == familyId).ToListAsync();
            foreach (var gDto in backup.Goals)
            {
                var existing = existingGoals.FirstOrDefault(g => g.Name.Equals(gDto.Name, StringComparison.OrdinalIgnoreCase));
                
                // Parse deadline from string (YYYY-MM or YYYY-MM-DD format)
                DateOnly? parsedDeadline = null;
                if (!string.IsNullOrEmpty(gDto.Deadline) && gDto.Deadline.Length >= 7)
                {
                    if (int.TryParse(gDto.Deadline[..4], out var year) && 
                        int.TryParse(gDto.Deadline.Substring(5, 2), out var month))
                    {
                        parsedDeadline = new DateOnly(year, month, 1);
                    }
                }

                if (existing != null)
                {
                    if (replaceExisting)
                    {
                        existing.Target = gDto.Target;
                        existing.AllocatedAmount = gDto.AllocatedAmount;
                        existing.Deadline = parsedDeadline;
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
                        // Id is now auto-generated by database
                        Name = gDto.Name,
                        Target = gDto.Target,
                        AllocatedAmount = gDto.AllocatedAmount,
                        Deadline = parsedDeadline,
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
            var existingSnapshots = await _db.Snapshots.Where(s => s.FamilyId == familyId).ToListAsync();
            foreach (var sDto in backup.Snapshots)
            {
                if (!DateOnly.TryParse(sDto.Date, out var snapshotDate))
                    continue;

                var existing = existingSnapshots.FirstOrDefault(s => s.SnapshotDate == snapshotDate);

                if (existing != null && !replaceExisting)
                    continue;

                if (existing != null && replaceExisting)
                {
                    await _snapshotService.DeleteAsync(existing.Id);
                }

                var newSnapshot = new Snapshot { SnapshotDate = snapshotDate, FamilyId = familyId };
                _db.Snapshots.Add(newSnapshot);
                await _db.SaveChangesAsync();

                foreach (var lineDto in sDto.Lines)
                {
                    var mappedAccountId = accountIdMap.GetValueOrDefault(lineDto.AccountId, lineDto.AccountId);
                    var account = await _db.Accounts.FindAsync(mappedAccountId);
                    if (account == null && !string.IsNullOrEmpty(lineDto.AccountName))
                        account = await _db.Accounts.FirstOrDefaultAsync(a => a.Name == lineDto.AccountName && a.FamilyId == familyId);

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

                foreach (var invDto in sDto.Investments)
                {
                    int? mappedPortfolioId = null;
                    if (invDto.PortfolioId.HasValue)
                    {
                        mappedPortfolioId = portfolioIdMap.GetValueOrDefault(invDto.PortfolioId.Value, invDto.PortfolioId.Value);
                        if (!await _db.Portfolios.AnyAsync(p => p.Id == mappedPortfolioId))
                            mappedPortfolioId = null;
                    }

                    newSnapshot.Investments.Add(new InvestmentAsset
                    {
                        SnapshotId = newSnapshot.Id,
                        Name = invDto.Name,
                        CostBasis = invDto.CostBasis,
                        Value = invDto.Value,
                        PortfolioId = mappedPortfolioId
                    });
                }

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

                if (sDto.MonthlyExpenses != null)
                {
                    foreach (var expDto in sDto.MonthlyExpenses)
                    {
                        var mappedCategoryId = budgetCategoryIdMap.GetValueOrDefault(expDto.CategoryId, expDto.CategoryId);
                        var category = await _db.BudgetCategories.FindAsync(mappedCategoryId);
                        if (category == null && !string.IsNullOrEmpty(expDto.CategoryName))
                            category = await _db.BudgetCategories.FirstOrDefaultAsync(c => c.Name == expDto.CategoryName && c.FamilyId == familyId);

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
            var innerEx = ex;
            while (innerEx.InnerException != null)
                innerEx = innerEx.InnerException;
            result.Error = $"{ex.Message} - Inner: {innerEx.Message}";
            _log.LogError($"Import failed: {ex.Message}", ex, "Import");
        }

        return result;
    }

    public async Task ClearAllDataAsync(int familyId)
    {
        _db.MonthlyExpenses.RemoveRange(_db.MonthlyExpenses.Where(e => e.Snapshot!.FamilyId == familyId));
        _db.Receivables.RemoveRange(_db.Receivables.Where(r => r.Snapshot!.FamilyId == familyId));
        _db.InvestmentAssets.RemoveRange(_db.InvestmentAssets.Where(i => i.Snapshot!.FamilyId == familyId));
        _db.SnapshotLines.RemoveRange(_db.SnapshotLines.Where(l => l.Snapshot!.FamilyId == familyId));
        _db.Snapshots.RemoveRange(_db.Snapshots.Where(s => s.FamilyId == familyId));
        _db.Goals.RemoveRange(_db.Goals.Where(g => g.FamilyId == familyId));
        _db.Portfolios.RemoveRange(_db.Portfolios.Where(p => p.FamilyId == familyId));
        _db.BudgetCategories.RemoveRange(_db.BudgetCategories.Where(b => b.FamilyId == familyId));
        _db.Accounts.RemoveRange(_db.Accounts.Where(a => a.FamilyId == familyId));
        await _db.SaveChangesAsync();
    }
}

