using CarDealerManager.Domain.Entities;
using CarDealerManager.Domain.Enums;

namespace CarDealerManager.Domain.Financial;

public sealed class VehicleFinancialCalculator
{
    private const decimal ConstraintComparisonTolerance = 0.000001m;

    public FinancialCalculationResult Calculate(Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        var estimates = vehicle.CostEntries
            .Where(entry => entry.Kind == VehicleCostKind.Estimated
                && entry.IncludedInAnalysis
                && entry.ArchivedAtUtc == null)
            .ToList();

        var analysisCosts = vehicle.AnalysisPurchasePrice is > 0m
            ? CalculateEstimatedCosts(vehicle.AnalysisPurchasePrice.Value, estimates)
            : null;
        var totalEstimatedInvestment = analysisCosts is null
            ? null
            : vehicle.AnalysisPurchasePrice + analysisCosts.RiskAdjustedTotal;

        return new FinancialCalculationResult(
            "EUR",
            vehicle.AnalysisPurchasePrice,
            analysisCosts,
            CalculateScenario(vehicle.ExpectedSalePrice, totalEstimatedInvestment),
            CalculateScenario(vehicle.ConservativeSalePrice, totalEstimatedInvestment),
            CalculateMaxBid(vehicle.ExpectedSalePrice, vehicle, estimates),
            CalculateMaxBid(vehicle.ConservativeSalePrice, vehicle, estimates));
    }

    private static EstimatedCostSummary CalculateEstimatedCosts(
        decimal purchasePrice,
        IReadOnlyCollection<VehicleCostEntry> estimates)
    {
        var entries = estimates.Select(estimate =>
        {
            var purchasePriceRate = (estimate.PurchasePricePercentage ?? 0m) / 100m;
            var taxRate = (estimate.TaxPercentage ?? 0m) / 100m;
            var riskRate = (estimate.RiskPercentage ?? 0m) / 100m;
            var preTaxAmount = estimate.Amount + purchasePrice * purchasePriceRate;
            var taxAmount = preTaxAmount * taxRate;
            var taxAdjustedAmount = preTaxAmount + taxAmount;
            var riskAmount = taxAdjustedAmount * riskRate;

            return new CostEntryCalculation(
                estimate.Id,
                estimate.Category,
                estimate.Description,
                estimate.Amount,
                estimate.PurchasePricePercentage ?? 0m,
                estimate.TaxPercentage ?? 0m,
                estimate.RiskPercentage ?? 0m,
                preTaxAmount,
                taxAmount,
                taxAdjustedAmount,
                riskAmount,
                taxAdjustedAmount + riskAmount);
        }).ToList();

        return new EstimatedCostSummary(
            purchasePrice,
            entries.Sum(entry => entry.PreTaxAmount),
            entries.Sum(entry => entry.TaxAmount),
            entries.Sum(entry => entry.TaxAdjustedAmount),
            entries.Sum(entry => entry.RiskAmount),
            entries.Sum(entry => entry.RiskAdjustedAmount),
            entries);
    }

    private static SaleScenarioMetrics CalculateScenario(
        decimal? salePrice,
        decimal? totalEstimatedInvestment)
    {
        if (salePrice is null || totalEstimatedInvestment is null)
        {
            return new SaleScenarioMetrics(salePrice, totalEstimatedInvestment, null, null, null);
        }

        var profit = salePrice.Value - totalEstimatedInvestment.Value;
        decimal? roi = totalEstimatedInvestment > 0m
            ? profit / totalEstimatedInvestment.Value * 100m
            : null;
        decimal? margin = salePrice > 0m
            ? profit / salePrice.Value * 100m
            : null;

        return new SaleScenarioMetrics(salePrice, totalEstimatedInvestment, profit, roi, margin);
    }

    private static MaxBidCalculation CalculateMaxBid(
        decimal? salePrice,
        Vehicle vehicle,
        IReadOnlyCollection<VehicleCostEntry> estimates)
    {
        var bidIncrement = vehicle.BidIncrement > 0m ? vehicle.BidIncrement : 1m;

        if (salePrice is null or <= 0m)
        {
            return IncompleteMaxBid("A positive sale price is required.", salePrice, vehicle, bidIncrement);
        }

        if (vehicle.MinimumProfitAmount is null && vehicle.MinimumRoiPercentage is null)
        {
            return IncompleteMaxBid("At least one financial target is required.", salePrice, vehicle, bidIncrement);
        }

        var fixedCoefficient = 0m;
        var purchasePriceCoefficient = 0m;

        foreach (var estimate in estimates)
        {
            var taxFactor = 1m + (estimate.TaxPercentage ?? 0m) / 100m;
            var riskFactor = 1m + (estimate.RiskPercentage ?? 0m) / 100m;
            fixedCoefficient += estimate.Amount * taxFactor * riskFactor;
            purchasePriceCoefficient += (estimate.PurchasePricePercentage ?? 0m)
                / 100m * taxFactor * riskFactor;
        }

        var investmentCoefficient = 1m + purchasePriceCoefficient;
        decimal? maxBidByProfit = vehicle.MinimumProfitAmount is null
            ? null
            : (salePrice.Value - vehicle.MinimumProfitAmount.Value - fixedCoefficient)
                / investmentCoefficient;
        var minimumRoiRate = vehicle.MinimumRoiPercentage / 100m;
        decimal? maxBidByRoi = minimumRoiRate is null
            ? null
            : (salePrice.Value / (1m + minimumRoiRate.Value) - fixedCoefficient)
                / investmentCoefficient;

        var mathematicalMaxBid = MinPresent(maxBidByProfit, maxBidByRoi);
        var bindingConstraint = DetermineBindingConstraint(maxBidByProfit, maxBidByRoi);

        if (mathematicalMaxBid is null || mathematicalMaxBid <= 0m)
        {
            return NotViableMaxBid(
                "No positive purchase price can satisfy the configured targets.",
                salePrice.Value,
                vehicle,
                bidIncrement,
                maxBidByProfit,
                maxBidByRoi,
                mathematicalMaxBid,
                bindingConstraint);
        }

        var finalMaxBid = FloorToIncrement(mathematicalMaxBid.Value, bidIncrement);
        if (finalMaxBid <= 0m)
        {
            return NotViableMaxBid(
                "The viable mathematical bid is below the configured bid increment.",
                salePrice.Value,
                vehicle,
                bidIncrement,
                maxBidByProfit,
                maxBidByRoi,
                mathematicalMaxBid,
                bindingConstraint);
        }

        var costs = CalculateEstimatedCosts(finalMaxBid, estimates);
        var totalInvestment = finalMaxBid + costs.RiskAdjustedTotal;
        var profit = salePrice.Value - totalInvestment;
        decimal? roi = totalInvestment > 0m ? profit / totalInvestment * 100m : null;
        var margin = profit / salePrice.Value * 100m;
        bool? profitSatisfied = vehicle.MinimumProfitAmount is null
            ? null
            : profit + ConstraintComparisonTolerance >= vehicle.MinimumProfitAmount.Value;
        bool? roiSatisfied = vehicle.MinimumRoiPercentage is null
            ? null
            : roi!.Value + ConstraintComparisonTolerance >= vehicle.MinimumRoiPercentage.Value;
        var allSatisfied = profitSatisfied is not false && roiSatisfied is not false;

        if (!allSatisfied)
        {
            return NotViableMaxBid(
                "The rounded bid did not satisfy all configured constraints.",
                salePrice.Value,
                vehicle,
                bidIncrement,
                maxBidByProfit,
                maxBidByRoi,
                mathematicalMaxBid,
                bindingConstraint);
        }

        return new MaxBidCalculation(
            MaxBidStatus.Calculated,
            null,
            salePrice,
            vehicle.MinimumProfitAmount,
            vehicle.MinimumRoiPercentage,
            bidIncrement,
            maxBidByProfit,
            maxBidByRoi,
            mathematicalMaxBid,
            finalMaxBid,
            bindingConstraint,
            costs,
            totalInvestment,
            profit,
            roi,
            margin,
            profitSatisfied,
            roiSatisfied,
            true);
    }

    private static MaxBidCalculation IncompleteMaxBid(
        string reason,
        decimal? salePrice,
        Vehicle vehicle,
        decimal bidIncrement)
        => new(
            MaxBidStatus.InsufficientData,
            reason,
            salePrice,
            vehicle.MinimumProfitAmount,
            vehicle.MinimumRoiPercentage,
            bidIncrement,
            null,
            null,
            null,
            null,
            MaxBidBindingConstraint.None,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);

    private static MaxBidCalculation NotViableMaxBid(
        string reason,
        decimal salePrice,
        Vehicle vehicle,
        decimal bidIncrement,
        decimal? maxBidByProfit,
        decimal? maxBidByRoi,
        decimal? mathematicalMaxBid,
        MaxBidBindingConstraint bindingConstraint)
        => new(
            MaxBidStatus.NotFinanciallyViable,
            reason,
            salePrice,
            vehicle.MinimumProfitAmount,
            vehicle.MinimumRoiPercentage,
            bidIncrement,
            maxBidByProfit,
            maxBidByRoi,
            mathematicalMaxBid,
            null,
            bindingConstraint,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            false);

    private static decimal? MinPresent(decimal? left, decimal? right)
        => (left, right) switch
        {
            (not null, not null) => Math.Min(left.Value, right.Value),
            (not null, null) => left,
            (null, not null) => right,
            _ => null
        };

    private static MaxBidBindingConstraint DetermineBindingConstraint(
        decimal? maxBidByProfit,
        decimal? maxBidByRoi)
    {
        if (maxBidByProfit is not null && maxBidByRoi is not null)
        {
            if (Math.Abs(maxBidByProfit.Value - maxBidByRoi.Value)
                <= ConstraintComparisonTolerance)
            {
                return MaxBidBindingConstraint.Both;
            }

            return maxBidByProfit < maxBidByRoi
                ? MaxBidBindingConstraint.MinimumProfit
                : MaxBidBindingConstraint.MinimumRoi;
        }

        return maxBidByProfit is not null
            ? MaxBidBindingConstraint.MinimumProfit
            : MaxBidBindingConstraint.MinimumRoi;
    }

    private static decimal FloorToIncrement(decimal value, decimal increment)
        => Math.Floor(value / increment) * increment;
}
