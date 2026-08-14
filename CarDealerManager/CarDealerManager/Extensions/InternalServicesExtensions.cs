using CarDealerManager.Application.IRepository;
using CarDealerManager.Application.IService;
using CarDealerManager.Application.Services;
using CarDealerManager.Domain.Financial;
using CarDealerManager.Infrastructure.Persistence;
using CarDealerManager.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace CarDealerManager.WebAPI.Extensions
{
    public static class InternalServicesExtensions
    {
        public static void ConfigureDbContextService(this IServiceCollection services)
        {
            services.AddDbContext<CdmDbContext>((serviceProvider, options) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("MainDatabase")
                    ?? throw new InvalidOperationException(
                        "Connection string 'MainDatabase' is not configured.");

                options.UseNpgsql(
                    connectionString,
                    npgsql => npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            });
        }

        public static void ConfigureApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton<VehicleFinancialCalculator>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IVehicleService, VehicleService>();
        }
    }
}
