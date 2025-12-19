using MediatR;

namespace CarBuilder.Application.Cars.Commands.DeleteCar;

public record DeleteCarCommand(Guid Id) : IRequest<Unit>;
