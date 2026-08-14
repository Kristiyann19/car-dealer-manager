using CarDealerManager.Domain.Entities;
using CarDealerManager.Domain.Enums;
using CarDealerManager.Domain.Financial;
using Xunit;

namespace CarDealerManager.Domain.Tests;

public sealed class VehicleFinancialCalculatorTests
{
    private readonly VehicleFinancialCalculator calculator = new();

    [Fact]
    public void Calculate_ReproducesWorkedProfitAndRoiAnalysis()
    {
        var vehicle = CreateWorkedExampleVehicle();

        var result = calculator.Calculate(vehicle);

        Assert.NotNull(result.EstimatedCostsAtAnalysisPrice);
        AssertClose(2_660m, result.EstimatedCostsAtAnalysisPrice.RiskAdjustedTotal);
        AssertClose(12_660m, result.ExpectedScenario.TotalEstimatedInvestment);
        AssertClose(3_340m, result.ExpectedScenario.Profit);
        AssertClose(26.3823m, result.ExpectedScenario.RoiPercentage, 0.0001m);
        AssertClose(20.875m, result.ExpectedScenario.MarginPercentage, 0.0001m);
        AssertClose(1_840m, result.ConservativeScenario.Profit);
        AssertClose(14.5339m, result.ConservativeScenario.RoiPercentage, 0.0001m);
        AssertClose(12.6896m, result.ConservativeScenario.MarginPercentage, 0.0001m);
    }

    [Fact]
    public void Calculate_ReproducesWorkedExpectedAndConservativeMaxBids()
    {
        var vehicle = CreateWorkedExampleVehicle();

        var result = calculator.Calculate(vehicle);

        Assert.Equal(MaxBidStatus.Calculated, result.ExpectedMaxBid.Status);
        AssertClose(10_810.8108m, result.ExpectedMaxBid.MaxBidByMinimumProfit, 0.0001m);
        AssertClose(10_649.9356m, result.ExpectedMaxBid.MaxBidByMinimumRoi, 0.0001m);
        Assert.Equal(10_649.93m, result.ExpectedMaxBid.FinalMaxBid);
        Assert.Equal(MaxBidBindingConstraint.MinimumRoi, result.ExpectedMaxBid.BindingConstraint);
        Assert.True(result.ExpectedMaxBid.AllConstraintsSatisfied);

        Assert.Equal(MaxBidStatus.Calculated, result.ConservativeMaxBid.Status);
        AssertClose(9_362.9343m, result.ConservativeMaxBid.MaxBidByMinimumProfit, 0.0001m);
        AssertClose(9_443.3719m, result.ConservativeMaxBid.MaxBidByMinimumRoi, 0.0001m);
        Assert.Equal(9_362.93m, result.ConservativeMaxBid.FinalMaxBid);
        Assert.Equal(
            MaxBidBindingConstraint.MinimumProfit,
            result.ConservativeMaxBid.BindingConstraint);
        Assert.True(result.ConservativeMaxBid.AllConstraintsSatisfied);
    }

    [Fact]
    public void Calculate_FloorsMaxBidToConfiguredIncrementAndRevalidatesConstraints()
    {
        var vehicle = CreateWorkedExampleVehicle();
        vehicle.BidIncrement = 100m;

        var result = calculator.Calculate(vehicle);

        Assert.Equal(10_600m, result.ExpectedMaxBid.FinalMaxBid);
        Assert.True(result.ExpectedMaxBid.MinimumProfitSatisfied);
        Assert.True(result.ExpectedMaxBid.MinimumRoiSatisfied);
        Assert.True(result.ExpectedMaxBid.AllConstraintsSatisfied);
    }

    [Fact]
    public void Calculate_ExcludesArchivedAndNonIncludedEstimates()
    {
        var vehicle = CreateWorkedExampleVehicle();
        vehicle.CostEntries.Add(FixedEstimate(99, VehicleCostCategory.Miscellaneous, 5_000m, included: false));
        vehicle.CostEntries.Add(FixedEstimate(
            100,
            VehicleCostCategory.Miscellaneous,
            5_000m,
            archivedAtUtc: DateTimeOffset.UtcNow));

        var result = calculator.Calculate(vehicle);

        Assert.Equal(4, result.EstimatedCostsAtAnalysisPrice!.Entries.Count);
        AssertClose(2_660m, result.EstimatedCostsAtAnalysisPrice.RiskAdjustedTotal);
    }

    [Fact]
    public void Calculate_AppliesTaxBeforeRiskAndReturnsDetailedBreakdown()
    {
        var vehicle = new Vehicle
        {
            AnalysisPurchasePrice = 10_000m,
            ExpectedSalePrice = 20_000m,
            MinimumProfitAmount = 1_000m
        };
        vehicle.CostEntries.Add(new VehicleCostEntry
        {
            Id = 1,
            Kind = VehicleCostKind.Estimated,
            Category = VehicleCostCategory.AuctionFee,
            Description = "Fee",
            Amount = 250m,
            PurchasePricePercentage = 3m,
            TaxPercentage = 20m,
            RiskPercentage = 10m,
            IncludedInAnalysis = true
        });

        var result = calculator.Calculate(vehicle);
        var fee = Assert.Single(result.EstimatedCostsAtAnalysisPrice!.Entries);

        AssertClose(550m, fee.PreTaxAmount);
        AssertClose(110m, fee.TaxAmount);
        AssertClose(660m, fee.TaxAdjustedAmount);
        AssertClose(66m, fee.RiskAmount);
        AssertClose(726m, fee.RiskAdjustedAmount);
    }

    [Fact]
    public void Calculate_ReturnsNotFinanciallyViableWhenTargetsAllowNoPositiveBid()
    {
        var vehicle = new Vehicle
        {
            ExpectedSalePrice = 1_000m,
            ConservativeSalePrice = 900m,
            MinimumProfitAmount = 2_000m,
            MinimumRoiPercentage = 20m,
            BidIncrement = 1m
        };

        var result = calculator.Calculate(vehicle);

        Assert.Equal(MaxBidStatus.NotFinanciallyViable, result.ExpectedMaxBid.Status);
        Assert.Null(result.ExpectedMaxBid.FinalMaxBid);
        Assert.False(result.ExpectedMaxBid.AllConstraintsSatisfied);
    }

    [Fact]
    public void Calculate_ReturnsNotFinanciallyViableWhenViableBidIsBelowIncrement()
    {
        var vehicle = new Vehicle
        {
            ExpectedSalePrice = 100m,
            MinimumProfitAmount = 99.50m,
            BidIncrement = 1m
        };

        var result = calculator.Calculate(vehicle);

        AssertClose(0.50m, result.ExpectedMaxBid.MathematicalMaxBid);
        Assert.Equal(MaxBidStatus.NotFinanciallyViable, result.ExpectedMaxBid.Status);
        Assert.Null(result.ExpectedMaxBid.FinalMaxBid);
    }

    [Fact]
    public void Calculate_ReturnsInsufficientDataForMissingTargetsOrSalePrice()
    {
        var missingTargets = calculator.Calculate(new Vehicle { ExpectedSalePrice = 10_000m });
        var missingSalePrice = calculator.Calculate(new Vehicle { MinimumProfitAmount = 1_000m });

        Assert.Equal(MaxBidStatus.InsufficientData, missingTargets.ExpectedMaxBid.Status);
        Assert.Equal(MaxBidStatus.InsufficientData, missingSalePrice.ExpectedMaxBid.Status);
        Assert.Null(missingTargets.ExpectedMaxBid.FinalMaxBid);
        Assert.Null(missingSalePrice.ExpectedMaxBid.FinalMaxBid);
    }

    [Fact]
    public void Calculate_ReportsBothWhenProfitAndRoiConstraintsAreEqual()
    {
        var vehicle = new Vehicle
        {
            ExpectedSalePrice = 10_000m,
            MinimumProfitAmount = 2_000m,
            MinimumRoiPercentage = 25m,
            BidIncrement = 1m
        };

        var result = calculator.Calculate(vehicle);

        Assert.Equal(8_000m, result.ExpectedMaxBid.FinalMaxBid);
        Assert.Equal(MaxBidBindingConstraint.Both, result.ExpectedMaxBid.BindingConstraint);
    }

    [Fact]
    public void Calculate_DoesNotUseMinimumAcceptableSalePriceForMaxBid()
    {
        var vehicle = CreateWorkedExampleVehicle();
        vehicle.MinimumAcceptableSalePrice = 1m;

        var result = calculator.Calculate(vehicle);

        Assert.Equal(10_649.93m, result.ExpectedMaxBid.FinalMaxBid);
        Assert.Equal(9_362.93m, result.ConservativeMaxBid.FinalMaxBid);
    }

    [Fact]
    public void Calculate_SupportsOneConstraintAndUsesDefaultOneEuroIncrement()
    {
        var vehicle = new Vehicle
        {
            ExpectedSalePrice = 10_000m,
            MinimumProfitAmount = 2_345.25m
        };

        var result = calculator.Calculate(vehicle);

        Assert.Equal(1m, result.ExpectedMaxBid.BidIncrement);
        AssertClose(7_654.75m, result.ExpectedMaxBid.MaxBidByMinimumProfit);
        Assert.Null(result.ExpectedMaxBid.MaxBidByMinimumRoi);
        Assert.Equal(7_654m, result.ExpectedMaxBid.FinalMaxBid);
        Assert.Equal(
            MaxBidBindingConstraint.MinimumProfit,
            result.ExpectedMaxBid.BindingConstraint);
        Assert.True(result.ExpectedMaxBid.AllConstraintsSatisfied);
    }

    [Fact]
    public void Calculate_ReturnsNullMetricsWhenAnalysisPurchasePriceIsMissing()
    {
        var vehicle = new Vehicle
        {
            ExpectedSalePrice = 10_000m,
            ConservativeSalePrice = 9_000m,
            MinimumProfitAmount = 2_000m
        };

        var result = calculator.Calculate(vehicle);

        Assert.Null(result.EstimatedCostsAtAnalysisPrice);
        Assert.Null(result.ExpectedScenario.TotalEstimatedInvestment);
        Assert.Null(result.ExpectedScenario.Profit);
        Assert.Null(result.ExpectedScenario.RoiPercentage);
        Assert.Null(result.ExpectedScenario.MarginPercentage);
        Assert.Null(result.ConservativeScenario.Profit);
        Assert.Equal(MaxBidStatus.Calculated, result.ExpectedMaxBid.Status);
    }

    private static Vehicle CreateWorkedExampleVehicle()
    {
        var vehicle = new Vehicle
        {
            AnalysisPurchasePrice = 10_000m,
            ExpectedSalePrice = 16_000m,
            ConservativeSalePrice = 14_500m,
            MinimumProfitAmount = 2_500m,
            MinimumRoiPercentage = 20m,
            BidIncrement = 0.01m
        };

        vehicle.CostEntries.Add(FixedEstimate(1, VehicleCostCategory.Transport, 660m));
        vehicle.CostEntries.Add(FixedEstimate(2, VehicleCostCategory.Repair, 1_000m, risk: 15m));
        vehicle.CostEntries.Add(FixedEstimate(3, VehicleCostCategory.RegistrationDocuments, 190m));
        vehicle.CostEntries.Add(new VehicleCostEntry
        {
            Id = 4,
            Kind = VehicleCostKind.Estimated,
            Category = VehicleCostCategory.AuctionFee,
            Description = "Auction fee",
            Amount = 250m,
            PurchasePricePercentage = 3m,
            TaxPercentage = 20m,
            IncludedInAnalysis = true
        });

        return vehicle;
    }

    private static VehicleCostEntry FixedEstimate(
        int id,
        VehicleCostCategory category,
        decimal amount,
        decimal? risk = null,
        bool included = true,
        DateTimeOffset? archivedAtUtc = null)
        => new()
        {
            Id = id,
            Kind = VehicleCostKind.Estimated,
            Category = category,
            Description = category.ToString(),
            Amount = amount,
            RiskPercentage = risk,
            IncludedInAnalysis = included,
            ArchivedAtUtc = archivedAtUtc
        };

    private static void AssertClose(
        decimal expected,
        decimal? actual,
        decimal tolerance = 0.000001m)
    {
        Assert.NotNull(actual);
        Assert.InRange(actual.Value, expected - tolerance, expected + tolerance);
    }
}
