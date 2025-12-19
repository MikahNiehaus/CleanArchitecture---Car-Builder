namespace CarBuilder.Application.Cars.DTOs;

public record CarDto(
    Guid Id,
    string Make,
    string Model,
    int Year,
    decimal Price,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
