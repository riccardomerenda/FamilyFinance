namespace FamilyFinance.Models;

public record Totals(
    decimal Liquidity,
    decimal Investments,
    decimal CreditsOpen,
    decimal PensionInsurance,
    decimal CurrentTotal,
    decimal ProjectedTotal,
    decimal GrandTotal,
    decimal InterestLiquidity
);

