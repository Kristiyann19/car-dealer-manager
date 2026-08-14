using CarDealerManager.Application.IRepository;
using CarDealerManager.Application.IService;
using CarDealerManager.Application.Models.Vehicles;
using CarDealerManager.Domain.Entities;
using CarDealerManager.Domain.Enums;
using CarDealerManager.Domain.Financial;

namespace CarDealerManager.Application.Services;

public sealed class VehicleService : IVehicleService
{
    private static readonly HashSet<VehicleStatus> CandidateStatuses =
    [
        VehicleStatus.Candidate,
        VehicleStatus.Bidding,
        VehicleStatus.Rejected,
        VehicleStatus.LostAuction
    ];

    private static readonly HashSet<VehicleCostCategory> BidDependentCategories =
    [
        VehicleCostCategory.AuctionFee,
        VehicleCostCategory.PlatformFee,
        VehicleCostCategory.BrokerFee,
        VehicleCostCategory.BankPaymentFee
    ];

    private readonly IVehicleRepository vehicleRepository;
    private readonly VehicleFinancialCalculator financialCalculator;
    private readonly TimeProvider timeProvider;

    public VehicleService(
        IVehicleRepository vehicleRepository,
        VehicleFinancialCalculator financialCalculator,
        TimeProvider timeProvider)
    {
        this.vehicleRepository = vehicleRepository;
        this.financialCalculator = financialCalculator;
        this.timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<VehicleListItemResponse>> GetAllAsync(
        bool includeArchived,
        CancellationToken cancellationToken)
    {
        var vehicles = await vehicleRepository.GetAllAsync(includeArchived, cancellationToken);
        return vehicles.Select(MapListItem).ToList();
    }

    public async Task<VehicleDetailResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(id, false, cancellationToken);
        return vehicle is null ? null : MapDetail(vehicle);
    }

    public async Task<VehicleDetailResponse> CreateAsync(
        VehicleUpsertRequest request,
        CancellationToken cancellationToken)
    {
        ValidateVehicleRequest(request);
        var now = timeProvider.GetUtcNow();
        var vehicle = new Vehicle
        {
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        ApplyRequest(vehicle, request);
        await vehicleRepository.AddAsync(vehicle, cancellationToken);
        await vehicleRepository.SaveChangesAsync(cancellationToken);
        return MapDetail(vehicle);
    }

    public async Task<VehicleDetailResponse?> UpdateAsync(
        int id,
        VehicleUpsertRequest request,
        CancellationToken cancellationToken)
    {
        ValidateVehicleRequest(request);
        var vehicle = await vehicleRepository.GetByIdAsync(id, true, cancellationToken);
        if (vehicle is null)
        {
            return null;
        }

        if (IsAcquisitionDecisionFrozen(vehicle.Status))
        {
            throw new ApplicationValidationException(
                "Candidate financial inputs are frozen after the vehicle is purchased.");
        }

        ApplyRequest(vehicle, request);
        vehicle.UpdatedAtUtc = timeProvider.GetUtcNow();
        await vehicleRepository.SaveChangesAsync(cancellationToken);
        return MapDetail(vehicle);
    }

    public async Task<bool> ArchiveAsync(int id, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(id, true, cancellationToken);
        if (vehicle is null)
        {
            return false;
        }

        var now = timeProvider.GetUtcNow();
        vehicle.ArchivedAtUtc ??= now;
        vehicle.UpdatedAtUtc = now;
        await vehicleRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RestoreAsync(int id, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(id, true, cancellationToken);
        if (vehicle is null)
        {
            return false;
        }

        vehicle.ArchivedAtUtc = null;
        vehicle.UpdatedAtUtc = timeProvider.GetUtcNow();
        await vehicleRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<VehicleCostEntryResponse?> AddEstimatedCostAsync(
        int vehicleId,
        EstimatedCostEntryUpsertRequest request,
        CancellationToken cancellationToken)
    {
        ValidateCostRequest(request);
        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, true, cancellationToken);
        if (vehicle is null)
        {
            return null;
        }

        EnsureEstimatesAreEditable(vehicle);
        var now = timeProvider.GetUtcNow();
        var costEntry = new VehicleCostEntry
        {
            VehicleId = vehicleId,
            Kind = VehicleCostKind.Estimated,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        ApplyRequest(costEntry, request);
        await vehicleRepository.AddCostEntryAsync(costEntry, cancellationToken);
        vehicle.UpdatedAtUtc = now;
        await vehicleRepository.SaveChangesAsync(cancellationToken);
        return MapCostEntry(costEntry);
    }

    public async Task<VehicleCostEntryResponse?> UpdateEstimatedCostAsync(
        int vehicleId,
        int costEntryId,
        EstimatedCostEntryUpsertRequest request,
        CancellationToken cancellationToken)
    {
        ValidateCostRequest(request);
        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, true, cancellationToken);
        if (vehicle is null)
        {
            return null;
        }

        EnsureEstimatesAreEditable(vehicle);
        var costEntry = await vehicleRepository.GetCostEntryAsync(
            vehicleId,
            costEntryId,
            true,
            cancellationToken);

        if (costEntry is null || costEntry.Kind != VehicleCostKind.Estimated)
        {
            return null;
        }

        if (costEntry.ArchivedAtUtc is not null)
        {
            throw new ApplicationValidationException("Archived estimates cannot be edited.");
        }

        ApplyRequest(costEntry, request);
        var now = timeProvider.GetUtcNow();
        costEntry.UpdatedAtUtc = now;
        vehicle.UpdatedAtUtc = now;
        await vehicleRepository.SaveChangesAsync(cancellationToken);
        return MapCostEntry(costEntry);
    }

    public async Task<bool> ArchiveEstimatedCostAsync(
        int vehicleId,
        int costEntryId,
        CancellationToken cancellationToken)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, true, cancellationToken);
        if (vehicle is null)
        {
            return false;
        }

        EnsureEstimatesAreEditable(vehicle);
        var costEntry = await vehicleRepository.GetCostEntryAsync(
            vehicleId,
            costEntryId,
            true,
            cancellationToken);

        if (costEntry is null || costEntry.Kind != VehicleCostKind.Estimated)
        {
            return false;
        }

        var now = timeProvider.GetUtcNow();
        costEntry.ArchivedAtUtc ??= now;
        costEntry.IncludedInAnalysis = false;
        costEntry.UpdatedAtUtc = now;
        vehicle.UpdatedAtUtc = now;
        await vehicleRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private VehicleDetailResponse MapDetail(Vehicle vehicle)
        => new(
            vehicle.Id,
            vehicle.Status,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            vehicle.Vin,
            vehicle.Mileage,
            vehicle.Engine,
            vehicle.FuelType,
            vehicle.TransmissionType,
            vehicle.SourcePlatform,
            vehicle.SourceUrl,
            vehicle.SourceCountry,
            vehicle.PhysicalLocation,
            vehicle.CurrentBid,
            vehicle.MyBid,
            vehicle.AnalysisPurchasePrice,
            vehicle.BidIncrement,
            vehicle.FinalPurchasePrice,
            vehicle.PurchaseDate,
            vehicle.ExpectedSalePrice,
            vehicle.ConservativeSalePrice,
            vehicle.PlannedListingPrice,
            vehicle.MinimumAcceptableSalePrice,
            vehicle.ActualSalePrice,
            vehicle.ListingDate,
            vehicle.SaleDate,
            vehicle.MinimumProfitAmount,
            vehicle.MinimumRoiPercentage,
            vehicle.CreatedAtUtc,
            vehicle.UpdatedAtUtc,
            vehicle.ArchivedAtUtc,
            vehicle.CostEntries.OrderBy(entry => entry.Id).Select(MapCostEntry).ToList(),
            financialCalculator.Calculate(vehicle));

    private static VehicleListItemResponse MapListItem(Vehicle vehicle)
        => new(
            vehicle.Id,
            vehicle.Status,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            vehicle.Vin,
            vehicle.CurrentBid,
            vehicle.MyBid,
            vehicle.AnalysisPurchasePrice,
            vehicle.ExpectedSalePrice,
            vehicle.ConservativeSalePrice,
            vehicle.ArchivedAtUtc);

    private static VehicleCostEntryResponse MapCostEntry(VehicleCostEntry entry)
        => new(
            entry.Id,
            entry.Kind,
            entry.Category,
            entry.Description,
            entry.Amount,
            entry.PurchasePricePercentage,
            entry.TaxPercentage,
            entry.RiskPercentage,
            entry.EntryDate,
            entry.RelatedEstimateId,
            entry.IncludedInAnalysis,
            entry.Notes,
            entry.CreatedAtUtc,
            entry.UpdatedAtUtc,
            entry.ArchivedAtUtc);

    private static void ApplyRequest(Vehicle vehicle, VehicleUpsertRequest request)
    {
        vehicle.Status = request.Status;
        vehicle.Brand = request.Brand.Trim();
        vehicle.Model = request.Model.Trim();
        vehicle.Year = request.Year;
        vehicle.Vin = NormalizeOptional(request.Vin)?.ToUpperInvariant();
        vehicle.Mileage = request.Mileage;
        vehicle.Engine = NormalizeOptional(request.Engine);
        vehicle.FuelType = request.FuelType;
        vehicle.TransmissionType = request.TransmissionType;
        vehicle.SourcePlatform = NormalizeOptional(request.SourcePlatform);
        vehicle.SourceUrl = NormalizeOptional(request.SourceUrl);
        vehicle.SourceCountry = NormalizeOptional(request.SourceCountry);
        vehicle.PhysicalLocation = NormalizeOptional(request.PhysicalLocation);
        vehicle.CurrentBid = request.CurrentBid;
        vehicle.MyBid = request.MyBid;
        vehicle.AnalysisPurchasePrice = request.AnalysisPurchasePrice;
        vehicle.BidIncrement = request.BidIncrement;
        vehicle.ExpectedSalePrice = request.ExpectedSalePrice;
        vehicle.ConservativeSalePrice = request.ConservativeSalePrice;
        vehicle.PlannedListingPrice = request.PlannedListingPrice;
        vehicle.MinimumAcceptableSalePrice = request.MinimumAcceptableSalePrice;
        vehicle.MinimumProfitAmount = request.MinimumProfitAmount;
        vehicle.MinimumRoiPercentage = request.MinimumRoiPercentage;
    }

    private static void ApplyRequest(
        VehicleCostEntry costEntry,
        EstimatedCostEntryUpsertRequest request)
    {
        costEntry.Category = request.Category;
        costEntry.Description = request.Description.Trim();
        costEntry.Amount = request.Amount;
        costEntry.PurchasePricePercentage = request.PurchasePricePercentage;
        costEntry.TaxPercentage = request.TaxPercentage;
        costEntry.RiskPercentage = request.RiskPercentage;
        costEntry.EntryDate = request.EntryDate;
        costEntry.IncludedInAnalysis = request.IncludedInAnalysis;
        costEntry.Notes = NormalizeOptional(request.Notes);
    }

    private static void ValidateVehicleRequest(VehicleUpsertRequest request)
    {
        if (!CandidateStatuses.Contains(request.Status))
        {
            throw new ApplicationValidationException(
                "Candidate editing supports Candidate, Bidding, Rejected, and LostAuction statuses only.");
        }

        if (request.FuelType is not null && !Enum.IsDefined(request.FuelType.Value))
        {
            throw new ApplicationValidationException("FuelType is invalid.");
        }

        if (request.TransmissionType is not null
            && !Enum.IsDefined(request.TransmissionType.Value))
        {
            throw new ApplicationValidationException("TransmissionType is invalid.");
        }

        if (string.IsNullOrWhiteSpace(request.Brand) || string.IsNullOrWhiteSpace(request.Model))
        {
            throw new ApplicationValidationException("Brand and model are required.");
        }

        if (request.Year is < 1886 or > 2200)
        {
            throw new ApplicationValidationException("Year must be between 1886 and 2200.");
        }

        if (request.Mileage < 0)
        {
            throw new ApplicationValidationException("Mileage cannot be negative.");
        }

        ValidateOptionalPositive(request.CurrentBid, nameof(request.CurrentBid));
        ValidateOptionalPositive(request.MyBid, nameof(request.MyBid));
        ValidateOptionalPositive(request.AnalysisPurchasePrice, nameof(request.AnalysisPurchasePrice));
        ValidateOptionalPositive(request.ExpectedSalePrice, nameof(request.ExpectedSalePrice));
        ValidateOptionalPositive(request.ConservativeSalePrice, nameof(request.ConservativeSalePrice));
        ValidateOptionalPositive(request.PlannedListingPrice, nameof(request.PlannedListingPrice));
        ValidateOptionalPositive(
            request.MinimumAcceptableSalePrice,
            nameof(request.MinimumAcceptableSalePrice));

        if (request.BidIncrement <= 0m)
        {
            throw new ApplicationValidationException("BidIncrement must be greater than zero.");
        }

        if (request.MinimumProfitAmount < 0m || request.MinimumRoiPercentage < 0m)
        {
            throw new ApplicationValidationException("Financial targets cannot be negative.");
        }

        if (request.ExpectedSalePrice is not null
            && request.ConservativeSalePrice > request.ExpectedSalePrice)
        {
            throw new ApplicationValidationException(
                "ConservativeSalePrice cannot exceed ExpectedSalePrice.");
        }

        if (!string.IsNullOrWhiteSpace(request.SourceUrl)
            && (!Uri.TryCreate(request.SourceUrl, UriKind.Absolute, out var sourceUri)
                || sourceUri.Scheme is not ("http" or "https")))
        {
            throw new ApplicationValidationException("SourceUrl must be an absolute HTTP or HTTPS URL.");
        }
    }

    private static void ValidateCostRequest(EstimatedCostEntryUpsertRequest request)
    {
        if (!Enum.IsDefined(request.Category))
        {
            throw new ApplicationValidationException("Cost category is invalid.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ApplicationValidationException("Cost description is required.");
        }

        if (request.Amount < 0m
            || request.PurchasePricePercentage < 0m
            || request.TaxPercentage < 0m
            || request.RiskPercentage < 0m)
        {
            throw new ApplicationValidationException("Cost amounts and percentages cannot be negative.");
        }

        if (request.Amount == 0m && (request.PurchasePricePercentage ?? 0m) == 0m)
        {
            throw new ApplicationValidationException(
                "An estimate requires a fixed amount, a purchase price percentage, or both.");
        }

        if (request.PurchasePricePercentage is > 0m
            && !BidDependentCategories.Contains(request.Category))
        {
            throw new ApplicationValidationException(
                "Purchase-price percentages are only supported for acquisition fee categories in V1.");
        }
    }

    private static void EnsureEstimatesAreEditable(Vehicle vehicle)
    {
        if (IsAcquisitionDecisionFrozen(vehicle.Status))
        {
            throw new ApplicationValidationException(
                "Estimates used for the acquisition decision are frozen after purchase.");
        }
    }

    private static bool IsAcquisitionDecisionFrozen(VehicleStatus status)
        => status is VehicleStatus.Purchased
            or VehicleStatus.InTransit
            or VehicleStatus.InPreparation
            or VehicleStatus.Listed
            or VehicleStatus.Sold;

    private static void ValidateOptionalPositive(decimal? value, string fieldName)
    {
        if (value is <= 0m)
        {
            throw new ApplicationValidationException($"{fieldName} must be greater than zero when supplied.");
        }
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
