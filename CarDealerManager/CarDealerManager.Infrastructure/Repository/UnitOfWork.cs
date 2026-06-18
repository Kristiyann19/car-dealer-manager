using CarDealerManager.Application.IRepository;
using CarDealerManager.Common.Enums;
using CarDealerManager.Domain;
using Microsoft.EntityFrameworkCore.Storage;

namespace CarDealerManager.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public CdmDbContext Context { get; }

        public UnitOfWork(
            CdmDbContext context)
        {
            Context = context;
        }

        public async Task<int> SaveChangesAsync(DbRepositoryType dbRepositoryType = DbRepositoryType.Main)
        {
            return dbRepositoryType switch
            {
                DbRepositoryType.Main => await Context.SaveChangesAsync(),
                _ => await Context.SaveChangesAsync(),
            };
        }

        public IDbContextTransaction BeginTransaction(DbRepositoryType dbRepositoryType = DbRepositoryType.Main)
        {
            return dbRepositoryType switch
            {
                DbRepositoryType.Main => Context.Database.BeginTransaction(),
                DbRepositoryType.Log => Context.Database.BeginTransaction(),
                _ => Context.Database.BeginTransaction(),
            };
        }
    }
}
