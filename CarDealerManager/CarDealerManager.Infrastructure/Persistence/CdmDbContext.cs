using CarDealerManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarDealerManager.Infrastructure.Persistence;

public sealed class CdmDbContext : DbContext
{
    public CdmDbContext(DbContextOptions<CdmDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleCostEntry> VehicleCostEntries => Set<VehicleCostEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CdmDbContext).Assembly);
    }
}
