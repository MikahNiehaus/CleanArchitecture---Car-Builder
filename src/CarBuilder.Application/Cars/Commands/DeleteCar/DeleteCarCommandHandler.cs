using CarBuilder.Application.Common.Exceptions;
using CarBuilder.Application.Common.Interfaces;
using CarBuilder.Domain.Entities;
using MediatR;

namespace CarBuilder.Application.Cars.Commands.DeleteCar;

public class DeleteCarCommandHandler : IRequestHandler<DeleteCarCommand, Unit>
{
    private readonly IRepository<Car> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCarCommandHandler(IRepository<Car> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteCarCommand request, CancellationToken cancellationToken)
    {
        var car = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (car == null)
            throw new NotFoundException(nameof(Car), request.Id);

        await _repository.DeleteAsync(car, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
