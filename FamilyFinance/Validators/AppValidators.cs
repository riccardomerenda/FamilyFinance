using FluentValidation;
using FamilyFinance.Models;
using FamilyFinance.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Validators;

public class AccountValidator : AbstractValidator<Account>
{
    public AccountValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.FamilyId)
            .GreaterThan(0).WithMessage("Invalid Family ID");
    }
}

public class GoalValidator : AbstractValidator<Goal>
{
    public GoalValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Target)
            .GreaterThan(0).WithMessage("Target amount must be greater than zero")
            .LessThan(100_000_000).WithMessage("Target amount is too high");

        RuleFor(x => x.Deadline)
            .Must(d => !d.HasValue || d.Value >= DateOnly.FromDateTime(DateTime.Today))
            .When(x => x.Id == 0) // Only validate future date for new goals
            .WithMessage("Deadline cannot be in the past");
    }
}

public class PortfolioValidator : AbstractValidator<Portfolio>
{
    public PortfolioValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");

        RuleFor(x => x.TimeHorizonYears)
            .InclusiveBetween(0, 50).WithMessage("Time horizon must be between 0 and 50 years");
    }
}

public class BudgetCategoryValidator : AbstractValidator<BudgetCategory>
{
    public BudgetCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");

        RuleFor(x => x.MonthlyBudget)
            .GreaterThanOrEqualTo(0).WithMessage("Monthly budget must be non-negative")
            .LessThan(1_000_000).WithMessage("Monthly budget is too high");
    }
}

public class SnapshotValidator : AbstractValidator<Snapshot>
{
    private readonly AppDbContext _context;

    public SnapshotValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.SnapshotDate)
            .MustAsync(async (snapshot, date, cancellation) =>
            {
                // Verify uniqueness of date for this family
                if (snapshot.Id == 0) // New snapshot
                {
                    return !await _context.Snapshots
                        .AnyAsync(s => s.FamilyId == snapshot.FamilyId && s.SnapshotDate == date, cancellation);
                }
                return true;
            })
            .WithMessage("A snapshot for this date already exists");

        RuleFor(x => x.SnapshotDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today.AddMonths(1)))
            .WithMessage("La data dello snapshot non può essere troppo nel futuro")
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today.AddYears(-20)))
            .WithMessage("La data dello snapshot è troppo nel passato");
    }
}

public class InvestmentAssetValidator : AbstractValidator<InvestmentAsset>
{
    public InvestmentAssetValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0).WithMessage("Value cannot be negative")
            .LessThan(100_000_000).WithMessage("Value is too high");

        RuleFor(x => x.CostBasis)
            .GreaterThanOrEqualTo(0).WithMessage("Cost basis cannot be negative");
    }
}

public class ReceivableValidator : AbstractValidator<Receivable>
{
    public ReceivableValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(200).WithMessage("Description must not exceed 200 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero")
            .LessThan(10_000_000).WithMessage("Amount is too high");
    }
}
