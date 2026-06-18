using CarDealerManager.Application.Dtos;

namespace CarDealerManager.Application.IService
{
    public interface ICarService
    {
        Task<List<CarDto>> GetAll(CancellationToken cancellationToken);
        Task<CarDto> GetById(int id, CancellationToken cancellationToken);
        Task<CarDto> Create(CarDto dto, CancellationToken cancellationToken);
        Task<bool> Update(int id, CarDto dto, CancellationToken cancellationToken);
        Task<bool> Delete(int id, CancellationToken cancellationToken);

        Task<bool> AddExpense(int carId, ExpenseDto dto, CancellationToken cancellationToken);
        Task<bool> SellCar(int carId, decimal salePrice, DateTime saleDate, CancellationToken cancellationToken);
    }
}