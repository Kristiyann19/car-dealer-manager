using CarDealerManager.Application.IRepository;
using CarDealerManager.Application.IService;
using CarDealerManager.Common.AppSettings;
using CarDealerManager.Domain;
using CarDealerManager.Infrastructure.Repository;
using CarDealerManager.Infrastructure.Service.CarDealerManager.Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

namespace CarDealerManager.WebAPI.Extensions
{
    public static class InternalServicesExtensions
    {
        public static void ConfigureDbContextService(this IServiceCollection services)
        {
            services
                .AddDbContext<CdmDbContext>(o =>
                {
                    o.UseNpgsql(AppSettingsProvider.MainDbConnectionString,
                        e => e.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                });
        }

        public static void ConfigureRepositories(this IServiceCollection services)
        {
            services
                .AddScoped<IUnitOfWork, UnitOfWork>()
                .AddScoped<ICarRepository, CarRepository>();

        }

        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddHttpContextAccessor();

            services.AddScoped<ICarService, CarService>();
        }

     }
}
