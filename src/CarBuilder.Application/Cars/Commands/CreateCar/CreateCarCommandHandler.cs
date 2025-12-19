using CarBuilder.Application.Common.Interfaces;
using CarBuilder.Domain.Entities;
using MediatR;

namespace CarBuilder.Application.Cars.Commands.CreateCar;

public class CreateCarCommandHandler : IRequestHandler<CreateCarCommand, Guid>
{
    private readonly IRepository<Car> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCarCommandHandler(IRepository<Car> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCarCommand request, CancellationToken cancellationToken)
    {
        var car = new Car(
            request.Make,
            request.Model,
            request.Year,
            request.Price,
            request.Description
        );

        await _repository.AddAsync(car, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return car.Id;
    }
}
