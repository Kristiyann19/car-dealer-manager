using CarDealerManager.Domain.Entities.Base;
using CarDealerManager.Domain.Enums;

namespace CarDealerManager.Domain.Entities;

public sealed class VehicleCostEntry : Entity
{
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public VehicleCostKind Kind { get; set; }
    public VehicleCostCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public decimal? PurchasePricePercentage { get; set; }
    public decimal? TaxPercentage { get; set; }
    public decimal? RiskPercentage { get; set; }
    public DateOnly? EntryDate { get; set; }

    public int? RelatedEstimateId { get; set; }
    public VehicleCostEntry? RelatedEstimate { get; set; }
    public ICollection<VehicleCostEntry> RelatedActualCosts { get; set; } = new List<VehicleCostEntry>();

    public bool IncludedInAnalysis { get; set; } = true;
    public string? Notes { get; set; }
}
