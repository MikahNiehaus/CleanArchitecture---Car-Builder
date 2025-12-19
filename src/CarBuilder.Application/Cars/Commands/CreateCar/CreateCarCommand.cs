using MediatR;

namespace CarBuilder.Application.Cars.Commands.CreateCar;

public record CreateCarCommand(
    string Make,
    string Model,
    int Year,
    decimal Price,
    string? Description
) : IRequest<Guid>;
