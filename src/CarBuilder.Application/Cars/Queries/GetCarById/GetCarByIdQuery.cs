using CarBuilder.Application.Cars.DTOs;
using MediatR;

namespace CarBuilder.Application.Cars.Queries.GetCarById;

public record GetCarByIdQuery(Guid Id) : IRequest<CarDto?>;
