using CarDealerManager.Application.IRepository;
using CarDealerManager.Domain.Entities;
using CarDealerManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarDealerManager.Infrastructure.Repository;

public sealed class VehicleRepository : IVehicleRepository
{
    private readonly CdmDbContext dbContext;

    public VehicleRepository(CdmDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Vehicle>> GetAllAsync(
        bool includeArchived,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Vehicles.AsNoTracking();
        if (!includeArchived)
        {
            query = query.Where(vehicle => vehicle.ArchivedAtUtc == null);
        }

        return await query.OrderByDescending(vehicle => vehicle.Id).ToListAsync(cancellationToken);
    }

    public async Task<Vehicle?> GetByIdAsync(
        int id,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Vehicles.Include(vehicle => vehicle.CostEntries).AsQueryable();
        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(vehicle => vehicle.Id == id, cancellationToken);
    }

    public async Task<VehicleCostEntry?> GetCostEntryAsync(
        int vehicleId,
        int costEntryId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        var query = dbContext.VehicleCostEntries.AsQueryable();
        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            entry => entry.VehicleId == vehicleId && entry.Id == costEntryId,
            cancellationToken);
    }

    public Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken)
        => dbContext.Vehicles.AddAsync(vehicle, cancellationToken).AsTask();

    public Task AddCostEntryAsync(
        VehicleCostEntry costEntry,
        CancellationToken cancellationToken)
        => dbContext.VehicleCostEntries.AddAsync(costEntry, cancellationToken).AsTask();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
