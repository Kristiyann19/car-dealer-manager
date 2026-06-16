using CarDealerManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CarDealerManager.Domain
{
    public class CdmDbContext : DbContext
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<Expense> Expenses { get; set; }

        public CdmDbContext(DbContextOptions<CdmDbContext> options)
            : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ApplyConfigurations(modelBuilder);
            DisableCascadeDelete(modelBuilder);
            ConfigurePgSqlNameMappings(modelBuilder);
        }

        protected void ApplyConfigurations(ModelBuilder modelBuilder)
        {
            var typesToRegister = Assembly.GetExecutingAssembly().GetTypes()
                   .Where(t => t.GetInterfaces().Any(gi =>
                       gi.IsGenericType
                       && gi.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
                   .ToList();

            foreach (var type in typesToRegister)
            {
                dynamic configurationInstance = Activator.CreateInstance(type);
                modelBuilder.ApplyConfiguration(configurationInstance);
            }
        }

        protected void DisableCascadeDelete(ModelBuilder modelBuilder)
        {
            modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership
                    && fk.DeleteBehavior == DeleteBehavior.Cascade)
                .ToList()
                .ForEach(e => e.DeleteBehavior = DeleteBehavior.Restrict);
        }

        protected void ConfigurePgSqlNameMappings(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Configure pgsql table names convention.
                entity.SetTableName(entity.ClrType.Name.ToLower());

                // Configure pgsql column names convention.
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.Name.ToLower());
                }
            }
        }
    }
}
