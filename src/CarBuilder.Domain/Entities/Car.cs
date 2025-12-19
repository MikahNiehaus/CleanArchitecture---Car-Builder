using CarBuilder.Domain.Common;
using CarBuilder.Domain.Exceptions;

namespace CarBuilder.Domain.Entities;

public class Car : BaseEntity
{
    public string Make { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public decimal Price { get; private set; }
    public string? Description { get; private set; }

    // EF Core constructor
    private Car() { }

    public Car(string make, string model, int year, decimal price, string? description = null)
    {
        SetMake(make);
        SetModel(model);
        SetYear(year);
        SetPrice(price);
        Description = description;
    }

    public void Update(string make, string model, int year, decimal price, string? description)
    {
        SetMake(make);
        SetModel(model);
        SetYear(year);
        SetPrice(price);
        Description = description;
        SetUpdatedAt();
    }

    private void SetMake(string make)
    {
        if (string.IsNullOrWhiteSpace(make))
            throw new DomainException("Car make cannot be empty");

        if (make.Length > 50)
            throw new DomainException("Car make cannot exceed 50 characters");

        Make = make.Trim();
    }

    private void SetModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new DomainException("Car model cannot be empty");

        if (model.Length > 50)
            throw new DomainException("Car model cannot exceed 50 characters");

        Model = model.Trim();
    }

    private void SetYear(int year)
    {
        var currentYear = DateTime.UtcNow.Year;

        if (year < 1900)
            throw new DomainException("Car year must be 1900 or later");

        if (year > currentYear + 1)
            throw new DomainException($"Car year cannot exceed {currentYear + 1}");

        Year = year;
    }

    private void SetPrice(decimal price)
    {
        if (price <= 0)
            throw new DomainException("Car price must be greater than zero");

        if (price > 10_000_000)
            throw new DomainException("Car price cannot exceed $10,000,000");

        Price = price;
    }
}
