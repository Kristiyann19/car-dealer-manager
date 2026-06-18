using CarDealerManager.Application.Dtos;
using CarDealerManager.Application.IService;
using Microsoft.AspNetCore.Mvc;

namespace CarDealerManager.WebAPI.Controllers
{
    public class CarController : Controller
    {
        [ApiController]
        [Route("api/[controller]")]
        public class CarsController : ControllerBase
        {
            private readonly ICarService carService;

            public CarsController(ICarService carService)
            {
                this.carService = carService;
            }

            [HttpGet]
            public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
            {
                var cars = await carService.GetAll(cancellationToken);

                return Ok(cars);
            }

            [HttpGet("{id:int}")]
            public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
            {
                var car = await carService.GetById(id, cancellationToken);

                if (car == null)
                {
                    return NotFound();
                }

                return Ok(car);
            }

            [HttpPost]
            public async Task<IActionResult> Create([FromBody] CarDto dto, CancellationToken cancellationToken)
            {
                var createdCar = await carService.Create(dto, cancellationToken);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdCar.Id },
                    createdCar);
            }

            [HttpPut("{id:int}")]
            public async Task<IActionResult> Update(int id, [FromBody] CarDto dto, CancellationToken cancellationToken)
            {
                var result = await carService.Update(id, dto, cancellationToken);

                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }

            [HttpDelete("{id:int}")]
            public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
            {
                var result = await carService.Delete(id, cancellationToken);

                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }

            [HttpPost("{id:int}/expenses")]
            public async Task<IActionResult> AddExpense(int id, [FromBody] ExpenseDto dto, CancellationToken cancellationToken)
            {
                var result = await carService.AddExpense(id, dto, cancellationToken);

                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }

        }
    }
}
