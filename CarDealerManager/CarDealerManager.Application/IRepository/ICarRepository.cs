using CarDealerManager.Application.FilterDtos;
using CarDealerManager.Domain;
using CarDealerManager.Domain.Entities;

namespace CarDealerManager.Application.IRepository
{
    public interface ICarRepository : IBaseRepository<Car, CarFilterDto, CdmDbContext>
    {
        Task<Car?> GetTrackedById(int id, CancellationToken cancellationToken);
        Task<Car?> GetByIdWithExpenses(int id, CancellationToken cancellationToken);
        Task AddExpense(Expense expense, CancellationToken cancellationToken);
    }
}
