using CarDealerManager.Domain.Enums;

namespace CarDealerManager.Domain.Financial;

public sealed record CostEntryCalculation(
    int CostEntryId,
    VehicleCostCategory Category,
    string Description,
    decimal FixedAmount,
    decimal PurchasePricePercentage,
    decimal TaxPercentage,
    decimal RiskPercentage,
    decimal PreTaxAmount,
    decimal TaxAmount,
    decimal TaxAdjustedAmount,
    decimal RiskAmount,
    decimal RiskAdjustedAmount);

public sealed record EstimatedCostSummary(
    decimal PurchasePrice,
    decimal PreTaxTotal,
    decimal TaxTotal,
    decimal RawEstimatedTotal,
    decimal RiskContingencyTotal,
    decimal RiskAdjustedTotal,
    IReadOnlyList<CostEntryCalculation> Entries);

public sealed record SaleScenarioMetrics(
    decimal? SalePrice,
    decimal? TotalEstimatedInvestment,
    decimal? Profit,
    decimal? RoiPercentage,
    decimal? MarginPercentage);

public sealed record MaxBidCalculation(
    MaxBidStatus Status,
    string? Reason,
    decimal? SalePrice,
    decimal? MinimumProfitAmount,
    decimal? MinimumRoiPercentage,
    decimal BidIncrement,
    decimal? MaxBidByMinimumProfit,
    decimal? MaxBidByMinimumRoi,
    decimal? MathematicalMaxBid,
    decimal? FinalMaxBid,
    MaxBidBindingConstraint BindingConstraint,
    EstimatedCostSummary? EstimatedCostsAtFinalBid,
    decimal? TotalInvestmentAtFinalBid,
    decimal? ProfitAtFinalBid,
    decimal? RoiPercentageAtFinalBid,
    decimal? MarginPercentageAtFinalBid,
    bool? MinimumProfitSatisfied,
    bool? MinimumRoiSatisfied,
    bool? AllConstraintsSatisfied);

public sealed record FinancialCalculationResult(
    string ReportingCurrency,
    decimal? AnalysisPurchasePrice,
    EstimatedCostSummary? EstimatedCostsAtAnalysisPrice,
    SaleScenarioMetrics ExpectedScenario,
    SaleScenarioMetrics ConservativeScenario,
    MaxBidCalculation ExpectedMaxBid,
    MaxBidCalculation ConservativeMaxBid);
