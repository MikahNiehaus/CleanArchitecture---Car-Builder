using AutoMapper;
using CarBuilder.Application.Cars.DTOs;
using CarBuilder.Application.Common.Interfaces;
using CarBuilder.Domain.Entities;
using MediatR;

namespace CarBuilder.Application.Cars.Queries.GetCars;

public class GetCarsQueryHandler : IRequestHandler<GetCarsQuery, List<CarDto>>
{
    private readonly IRepository<Car> _repository;
    private readonly IMapper _mapper;

    public GetCarsQueryHandler(IRepository<Car> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<CarDto>> Handle(GetCarsQuery request, CancellationToken cancellationToken)
    {
        var cars = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<List<CarDto>>(cars);
    }
}
