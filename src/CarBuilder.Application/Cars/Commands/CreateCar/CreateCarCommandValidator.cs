using FluentValidation;

namespace CarBuilder.Application.Cars.Commands.CreateCar;

public class CreateCarCommandValidator : AbstractValidator<CreateCarCommand>
{
    public CreateCarCommandValidator()
    {
        RuleFor(x => x.Make)
            .NotEmpty().WithMessage("Make is required")
            .MaximumLength(50).WithMessage("Make must not exceed 50 characters");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required")
            .MaximumLength(50).WithMessage("Model must not exceed 50 characters");

        RuleFor(x => x.Year)
            .GreaterThanOrEqualTo(1900).WithMessage("Year must be 1900 or later")
            .LessThanOrEqualTo(DateTime.UtcNow.Year + 1).WithMessage($"Year cannot exceed {DateTime.UtcNow.Year + 1}");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero")
            .LessThanOrEqualTo(10_000_000).WithMessage("Price cannot exceed $10,000,000");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
