using System.ComponentModel.DataAnnotations;
using CarDealerManager.Domain.Enums;

namespace CarDealerManager.Application.Models.Vehicles;

public sealed class VehicleUpsertRequest
{
    public VehicleStatus Status { get; init; } = VehicleStatus.Candidate;

    [Required, MaxLength(100)]
    public string Brand { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string Model { get; init; } = string.Empty;

    [Range(1886, 2200)]
    public int Year { get; init; }

    [MaxLength(50)]
    public string? Vin { get; init; }

    [Range(0, int.MaxValue)]
    public int? Mileage { get; init; }

    [MaxLength(100)]
    public string? Engine { get; init; }
    public FuelType? FuelType { get; init; }
    public TransmissionType? TransmissionType { get; init; }

    [MaxLength(100)]
    public string? SourcePlatform { get; init; }

    [MaxLength(2048)]
    public string? SourceUrl { get; init; }

    [MaxLength(100)]
    public string? SourceCountry { get; init; }

    [MaxLength(200)]
    public string? PhysicalLocation { get; init; }

    public decimal? CurrentBid { get; init; }
    public decimal? MyBid { get; init; }
    public decimal? AnalysisPurchasePrice { get; init; }
    public decimal BidIncrement { get; init; } = 1m;

    public decimal? ExpectedSalePrice { get; init; }
    public decimal? ConservativeSalePrice { get; init; }
    public decimal? PlannedListingPrice { get; init; }
    public decimal? MinimumAcceptableSalePrice { get; init; }

    public decimal? MinimumProfitAmount { get; init; }
    public decimal? MinimumRoiPercentage { get; init; }
}

public sealed class EstimatedCostEntryUpsertRequest
{
    public VehicleCostCategory Category { get; init; }

    [Required, MaxLength(500)]
    public string Description { get; init; } = string.Empty;

    public decimal Amount { get; init; }
    public decimal? PurchasePricePercentage { get; init; }
    public decimal? TaxPercentage { get; init; }
    public decimal? RiskPercentage { get; init; }
    public DateOnly? EntryDate { get; init; }
    public bool IncludedInAnalysis { get; init; } = true;

    [MaxLength(2000)]
    public string? Notes { get; init; }
}
