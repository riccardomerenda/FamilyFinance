namespace FamilyFinance.Models;

public record Totals(
    decimal Liquidity,
    decimal InvestmentsValue,      // Valore corrente di mercato
    decimal InvestmentsCost,       // Costo di carico totale
    decimal InvestmentsGainLoss,   // Guadagno/Perdita totale
    decimal CreditsOpen,
    decimal PensionInsuranceValue,     // Valore corrente previdenza
    decimal PensionInsuranceContrib,   // Contributi versati previdenza
    decimal PensionInsuranceGainLoss,  // Guadagno/Perdita previdenza
    decimal CurrentTotal,
    decimal ProjectedTotal,
    decimal GrandTotal,
    decimal InterestLiquidity
)
{
    public decimal InvestmentsGainLossPercent => InvestmentsCost > 0 ? (InvestmentsGainLoss / InvestmentsCost) * 100 : 0;
    public decimal PensionInsuranceGainLossPercent => PensionInsuranceContrib > 0 ? (PensionInsuranceGainLoss / PensionInsuranceContrib) * 100 : 0;
};

