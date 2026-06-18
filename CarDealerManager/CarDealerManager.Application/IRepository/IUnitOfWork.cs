using CarDealerManager.Common.Enums;
using CarDealerManager.Domain;
using Microsoft.EntityFrameworkCore.Storage;

namespace CarDealerManager.Application.IRepository
{
    public interface IUnitOfWork
    {
        CdmDbContext Context { get; }

        Task<int> SaveChangesAsync(DbRepositoryType dbRepositoryType = DbRepositoryType.Main);
        IDbContextTransaction BeginTransaction(DbRepositoryType dbRepositoryType = DbRepositoryType.Main);
    }
}
