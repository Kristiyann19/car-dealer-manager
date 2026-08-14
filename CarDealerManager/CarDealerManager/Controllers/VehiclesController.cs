using CarDealerManager.Application.IService;
using CarDealerManager.Application.Models.Vehicles;
using Microsoft.AspNetCore.Mvc;

namespace CarDealerManager.WebAPI.Controllers;

[ApiController]
[Route("api/vehicles")]
public sealed class VehiclesController : ControllerBase
{
    private readonly IVehicleService vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        this.vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VehicleListItemResponse>>> GetAll(
        [FromQuery] bool includeArchived,
        CancellationToken cancellationToken)
        => Ok(await vehicleService.GetAllAsync(includeArchived, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehicleDetailResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var vehicle = await vehicleService.GetByIdAsync(id, cancellationToken);
        return vehicle is null ? NotFound() : Ok(vehicle);
    }

    [HttpPost]
    public async Task<ActionResult<VehicleDetailResponse>> Create(
        [FromBody] VehicleUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var vehicle = await vehicleService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VehicleDetailResponse>> Update(
        int id,
        [FromBody] VehicleUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var vehicle = await vehicleService.UpdateAsync(id, request, cancellationToken);
        return vehicle is null ? NotFound() : Ok(vehicle);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Archive(int id, CancellationToken cancellationToken)
        => await vehicleService.ArchiveAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken)
        => await vehicleService.RestoreAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpPost("{vehicleId:int}/costs")]
    public async Task<ActionResult<VehicleCostEntryResponse>> AddEstimatedCost(
        int vehicleId,
        [FromBody] EstimatedCostEntryUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var costEntry = await vehicleService.AddEstimatedCostAsync(
            vehicleId,
            request,
            cancellationToken);

        return costEntry is null
            ? NotFound()
            : CreatedAtAction(nameof(GetById), new { id = vehicleId }, costEntry);
    }

    [HttpPut("{vehicleId:int}/costs/{costEntryId:int}")]
    public async Task<ActionResult<VehicleCostEntryResponse>> UpdateEstimatedCost(
        int vehicleId,
        int costEntryId,
        [FromBody] EstimatedCostEntryUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var costEntry = await vehicleService.UpdateEstimatedCostAsync(
            vehicleId,
            costEntryId,
            request,
            cancellationToken);

        return costEntry is null ? NotFound() : Ok(costEntry);
    }

    [HttpDelete("{vehicleId:int}/costs/{costEntryId:int}")]
    public async Task<IActionResult> ArchiveEstimatedCost(
        int vehicleId,
        int costEntryId,
        CancellationToken cancellationToken)
        => await vehicleService.ArchiveEstimatedCostAsync(
            vehicleId,
            costEntryId,
            cancellationToken)
            ? NoContent()
            : NotFound();
}
