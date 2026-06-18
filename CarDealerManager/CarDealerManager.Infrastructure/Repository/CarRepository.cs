using CarDealerManager.Application.FilterDtos;
using CarDealerManager.Application.IRepository;
using CarDealerManager.Common.Enums.BaseRepositories;
using CarDealerManager.Domain;
using CarDealerManager.Domain.Entities;
using CarDealerManager.Infastructure.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarDealerManager.Infrastructure.Repository
{
    public class CarRepository : BaseRepository<Car, CarFilterDto, CdmDbContext>, ICarRepository
    {
        public CarRepository(CdmDbContext context)
            : base(context)
        {
        }

        public async Task<Car?> GetTrackedById(int id, CancellationToken cancellationToken)
        {
            return await context.Cars
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Car?> GetByIdWithExpenses(int id, CancellationToken cancellationToken)
        {
            return await context.Cars
                .AsNoTracking()
                .Include(c => c.Expenses)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task AddExpense(Expense expense, CancellationToken cancellationToken)
        {
            await context.Expenses.AddAsync(expense, cancellationToken);
        }

        public override Func<IQueryable<Car>, IQueryable<Car>> ConstructInclude(IncludeType includeType = IncludeType.None)
        {
            return includeType switch
            {
                IncludeType.All => query => query.Include(c => c.Expenses),
                IncludeType.Collections => query => query.Include(c => c.Expenses),
                _ => query => query
            };
        }
    }
}
