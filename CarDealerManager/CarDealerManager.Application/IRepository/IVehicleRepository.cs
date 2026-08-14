using CarDealerManager.Domain.Entities;

namespace CarDealerManager.Application.IRepository;

public interface IVehicleRepository
{
    Task<IReadOnlyList<Vehicle>> GetAllAsync(bool includeArchived, CancellationToken cancellationToken);
    Task<Vehicle?> GetByIdAsync(int id, bool trackChanges, CancellationToken cancellationToken);
    Task<VehicleCostEntry?> GetCostEntryAsync(
        int vehicleId,
        int costEntryId,
        bool trackChanges,
        CancellationToken cancellationToken);
    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken);
    Task AddCostEntryAsync(VehicleCostEntry costEntry, CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
