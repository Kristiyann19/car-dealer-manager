using CarDealerManager.Domain.Entities.Base;
using CarDealerManager.Domain.Enums;

namespace CarDealerManager.Domain.Entities;

public sealed class Vehicle : Entity
{
    public VehicleStatus Status { get; set; } = VehicleStatus.Candidate;

    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Vin { get; set; }
    public int? Mileage { get; set; }
    public string? Engine { get; set; }
    public FuelType? FuelType { get; set; }
    public TransmissionType? TransmissionType { get; set; }

    public string? SourcePlatform { get; set; }
    public string? SourceUrl { get; set; }
    public string? SourceCountry { get; set; }
    public string? PhysicalLocation { get; set; }

    public decimal? CurrentBid { get; set; }
    public decimal? MyBid { get; set; }
    public decimal? AnalysisPurchasePrice { get; set; }
    public decimal BidIncrement { get; set; } = 1m;
    public decimal? FinalPurchasePrice { get; set; }
    public DateOnly? PurchaseDate { get; set; }

    public decimal? ExpectedSalePrice { get; set; }
    public decimal? ConservativeSalePrice { get; set; }
    public decimal? PlannedListingPrice { get; set; }
    public decimal? MinimumAcceptableSalePrice { get; set; }
    public decimal? ActualSalePrice { get; set; }
    public DateOnly? ListingDate { get; set; }
    public DateOnly? SaleDate { get; set; }

    public decimal? MinimumProfitAmount { get; set; }
    public decimal? MinimumRoiPercentage { get; set; }

    public ICollection<VehicleCostEntry> CostEntries { get; set; } = new List<VehicleCostEntry>();
}
