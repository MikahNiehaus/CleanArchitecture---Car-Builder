using AutoMapper;
using CarBuilder.Application.Cars.DTOs;
using CarBuilder.Domain.Entities;

namespace CarBuilder.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Car, CarDto>();
    }
}
