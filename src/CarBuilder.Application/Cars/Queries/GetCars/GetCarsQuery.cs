using CarBuilder.Application.Cars.DTOs;
using MediatR;

namespace CarBuilder.Application.Cars.Queries.GetCars;

public record GetCarsQuery : IRequest<List<CarDto>>;
