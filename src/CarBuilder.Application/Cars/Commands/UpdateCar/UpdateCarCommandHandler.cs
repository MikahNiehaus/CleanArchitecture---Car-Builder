using CarBuilder.Application.Common.Exceptions;
using CarBuilder.Application.Common.Interfaces;
using CarBuilder.Domain.Entities;
using MediatR;

namespace CarBuilder.Application.Cars.Commands.UpdateCar;

public class UpdateCarCommandHandler : IRequestHandler<UpdateCarCommand, Unit>
{
    private readonly IRepository<Car> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCarCommandHandler(IRepository<Car> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateCarCommand request, CancellationToken cancellationToken)
    {
        var car = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (car == null)
            throw new NotFoundException(nameof(Car), request.Id);

        car.Update(request.Make, request.Model, request.Year, request.Price, request.Description);

        await _repository.UpdateAsync(car, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
