namespace CarDealerManager.Infrastructure.Service
{
    using global::CarDealerManager.Application.Dtos;
    using global::CarDealerManager.Application.FilterDtos;
    using global::CarDealerManager.Application.IRepository;
    using global::CarDealerManager.Application.IService;
    using global::CarDealerManager.Common.Enums.BaseRepositories;
    using global::CarDealerManager.Domain.Entities;

    namespace CarDealerManager.Infrastructure.Service
    {
        public class CarService : ICarService
        {
            private readonly ICarRepository carRepository;
            private readonly IUnitOfWork unitOfWork;

            public CarService(
                ICarRepository carRepository,
                IUnitOfWork unitOfWork)
            {
                this.carRepository = carRepository;
                this.unitOfWork = unitOfWork;
            }

            public async Task<List<CarDto>> GetAll(CancellationToken cancellationToken)
            {
                var filter = new CarFilterDto
                {
                    GetAllData = true
                };

                var cars = await carRepository.GetList(
                    filter,
                    cancellationToken,
                    carRepository.ConstructInclude(IncludeType.Collections),
                    q => q.OrderByDescending(c => c.Id));

                return cars.Select(MapToDto).ToList();
            }

            public async Task<CarDto?> GetById(int id, CancellationToken cancellationToken)
            {
                var car = await carRepository.GetByIdWithExpenses(id, cancellationToken);

                if (car == null)
                {
                    return null;
                }

                return MapToDto(car);
            }

            public async Task<CarDto> Create(CarDto dto, CancellationToken cancellationToken)
            {
                var car = new Car
                {
                    Brand = dto.Brand,
                    Model = dto.Model,
                    Year = dto.Year,
                    Vin = dto.Vin,
                    PurchasePrice = dto.PurchasePrice,
                    PurchaseDate = dto.PurchaseDate,
                    SalePrice = dto.SalePrice,
                    SaleDate = dto.SaleDate,
                    Status = string.IsNullOrWhiteSpace(dto.Status)
                        ? "Purchased"
                        : dto.Status
                };

                await carRepository.Create(car);
                await unitOfWork.SaveChangesAsync();

                return MapToDto(car);
            }

            public async Task<bool> Update(int id, CarDto dto, CancellationToken cancellationToken)
            {
                var car = await carRepository.GetTrackedById(id, cancellationToken);

                if (car == null)
                {
                    return false;
                }

                car.Brand = dto.Brand;
                car.Model = dto.Model;
                car.Year = dto.Year;
                car.Vin = dto.Vin;
                car.PurchasePrice = dto.PurchasePrice;
                car.PurchaseDate = dto.PurchaseDate;
                car.SalePrice = dto.SalePrice;
                car.SaleDate = dto.SaleDate;
                car.Status = dto.Status;

                await unitOfWork.SaveChangesAsync();

                return true;
            }

            public async Task<bool> Delete(int id, CancellationToken cancellationToken)
            {
                var car = await carRepository.GetTrackedById(id, cancellationToken);

                if (car == null)
                {
                    return false;
                }

                carRepository.Delete(car);
                await unitOfWork.SaveChangesAsync();

                return true;
            }

            public async Task<bool> AddExpense(int carId, ExpenseDto dto, CancellationToken cancellationToken)
            {
                var carExists = await carRepository.AnyEntity(
                    c => c.Id == carId,
                    cancellationToken);

                if (!carExists)
                {
                    return false;
                }

                var expense = new Expense
                {
                    CarId = carId,
                    Type = dto.Type,
                    Description = dto.Description,
                    Amount = dto.Amount,
                    Date = dto.Date
                };

                await carRepository.AddExpense(expense, cancellationToken);
                await unitOfWork.SaveChangesAsync();

                return true;
            }

            public async Task<bool> SellCar(int carId, decimal salePrice, DateTime saleDate, CancellationToken cancellationToken)
            {
                var car = await carRepository.GetTrackedById(carId, cancellationToken);

                if (car == null)
                {
                    return false;
                }

                car.SalePrice = salePrice;
                car.SaleDate = saleDate;
                car.Status = "Sold";

                await unitOfWork.SaveChangesAsync();

                return true;
            }

            private static CarDto MapToDto(Car car)
            {
                //ADD AUTO MAPPER LATER
                return new CarDto
                {
                    Id = car.Id,
                    Brand = car.Brand,
                    Model = car.Model,
                    Year = car.Year,
                    Vin = car.Vin,
                    PurchasePrice = car.PurchasePrice,
                    PurchaseDate = car.PurchaseDate,
                    SalePrice = car.SalePrice,
                    SaleDate = car.SaleDate,
                    Status = car.Status,
                    Expenses = car.Expenses.Select(e => new ExpenseDto
                    {
                        Id = e.Id,
                        CarId = e.CarId,
                        Type = e.Type,
                        Description = e.Description,
                        Amount = e.Amount,
                        Date = e.Date
                    }).ToList()
                };
            }
        }
    }
}
