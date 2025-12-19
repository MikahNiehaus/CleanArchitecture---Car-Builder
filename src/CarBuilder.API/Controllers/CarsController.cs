using CarBuilder.Application.Cars.Commands.CreateCar;
using CarBuilder.Application.Cars.Commands.DeleteCar;
using CarBuilder.Application.Cars.Commands.UpdateCar;
using CarBuilder.Application.Cars.DTOs;
using CarBuilder.Application.Cars.Queries.GetCarById;
using CarBuilder.Application.Cars.Queries.GetCars;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarBuilder.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CarsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CarsController> _logger;

    public CarsController(IMediator mediator, ILogger<CarsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all cars
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CarDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CarDto>>> GetCars()
    {
        var query = new GetCarsQuery();
        var cars = await _mediator.Send(query);
        return Ok(cars);
    }

    /// <summary>
    /// Get car by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CarDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CarDto>> GetCar(Guid id)
    {
        var query = new GetCarByIdQuery(id);
        var car = await _mediator.Send(query);

        if (car == null)
            return NotFound();

        return Ok(car);
    }

    /// <summary>
    /// Create a new car
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateCar([FromBody] CreateCarCommand command)
    {
        var id = await _mediator.Send(command);

        _logger.LogInformation("Car created: {CarId}", id);

        return CreatedAtAction(nameof(GetCar), new { id }, id);
    }

    /// <summary>
    /// Update an existing car
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateCar(Guid id, [FromBody] UpdateCarCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        await _mediator.Send(command);

        _logger.LogInformation("Car updated: {CarId}", id);

        return NoContent();
    }

    /// <summary>
    /// Delete a car
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteCar(Guid id)
    {
        await _mediator.Send(new DeleteCarCommand(id));

        _logger.LogInformation("Car deleted: {CarId}", id);

        return NoContent();
    }
}
