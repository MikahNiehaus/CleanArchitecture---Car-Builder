using MediatR;

namespace CarBuilder.Application.Cars.Commands.UpdateCar;

public record UpdateCarCommand(
    Guid Id,
    string Make,
    string Model,
    int Year,
    decimal Price,
    string? Description
) : IRequest<Unit>;
