using AutoMapper;
using CarBuilder.Application.Cars.DTOs;
using CarBuilder.Application.Common.Interfaces;
using CarBuilder.Domain.Entities;
using MediatR;

namespace CarBuilder.Application.Cars.Queries.GetCarById;

public class GetCarByIdQueryHandler : IRequestHandler<GetCarByIdQuery, CarDto?>
{
    private readonly IRepository<Car> _repository;
    private readonly IMapper _mapper;

    public GetCarByIdQueryHandler(IRepository<Car> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CarDto?> Handle(GetCarByIdQuery request, CancellationToken cancellationToken)
    {
        var car = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return car == null ? null : _mapper.Map<CarDto>(car);
    }
}
