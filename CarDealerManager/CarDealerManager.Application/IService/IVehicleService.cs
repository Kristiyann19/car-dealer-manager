using CarDealerManager.Application.Models.Vehicles;

namespace CarDealerManager.Application.IService;

public interface IVehicleService
{
    Task<IReadOnlyList<VehicleListItemResponse>> GetAllAsync(
        bool includeArchived,
        CancellationToken cancellationToken);
    Task<VehicleDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<VehicleDetailResponse> CreateAsync(
        VehicleUpsertRequest request,
        CancellationToken cancellationToken);
    Task<VehicleDetailResponse?> UpdateAsync(
        int id,
        VehicleUpsertRequest request,
        CancellationToken cancellationToken);
    Task<bool> ArchiveAsync(int id, CancellationToken cancellationToken);
    Task<bool> RestoreAsync(int id, CancellationToken cancellationToken);
    Task<VehicleCostEntryResponse?> AddEstimatedCostAsync(
        int vehicleId,
        EstimatedCostEntryUpsertRequest request,
        CancellationToken cancellationToken);
    Task<VehicleCostEntryResponse?> UpdateEstimatedCostAsync(
        int vehicleId,
        int costEntryId,
        EstimatedCostEntryUpsertRequest request,
        CancellationToken cancellationToken);
    Task<bool> ArchiveEstimatedCostAsync(
        int vehicleId,
        int costEntryId,
        CancellationToken cancellationToken);
}
