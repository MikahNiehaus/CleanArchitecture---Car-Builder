# Project Structure & Architecture

## Overview

This guide covers the recommended project structure for a React + ASP.NET Core application following Clean Architecture principles. The architecture separates concerns into distinct layers, ensuring maintainability, testability, and scalability.

## Clean Architecture in ASP.NET Core

Clean Architecture organizes code into four primary layers, each with specific responsibilities:

### 1. Domain Layer (Core)
The innermost layer, completely independent of external concerns.

**Characteristics:**
- Contains enterprise business rules
- Defines entities, value objects, enums, and domain events
- Has NO dependencies on other layers
- Pure C# with no framework-specific code

**Structure:**
```
Domain/
├── Entities/
│   ├── Car.cs
│   ├── Customer.cs
│   └── Order.cs
├── ValueObjects/
│   ├── Money.cs
│   └── Address.cs
├── Enums/
│   └── OrderStatus.cs
├── Events/
│   └── OrderCreatedEvent.cs
└── Exceptions/
    └── DomainException.cs
```

**Example Entity:**
```csharp
public class Car
{
    public Guid Id { get; private set; }
    public string Make { get; private set; }
    public string Model { get; private set; }
    public int Year { get; private set; }
    public Money Price { get; private set; }

    private Car() { } // EF Core

    public Car(string make, string model, int year, Money price)
    {
        if (string.IsNullOrWhiteSpace(make))
            throw new DomainException("Make cannot be empty");

        Id = Guid.NewGuid();
        Make = make;
        Model = model;
        Year = year;
        Price = price;
    }
}
```

### 2. Application Layer
Contains application business rules and orchestrates the flow of data.

**Characteristics:**
- Defines interfaces for infrastructure (repository, external services)
- Contains application-specific business logic
- Uses CQRS pattern (Commands/Queries)
- Depends ONLY on Domain layer

**Structure:**
```
Application/
├── Common/
│   ├── Interfaces/
│   │   ├── IRepository.cs
│   │   ├── IUnitOfWork.cs
│   │   └── IDateTime.cs
│   ├── Behaviours/
│   │   ├── ValidationBehaviour.cs
│   │   └── LoggingBehaviour.cs
│   └── Mappings/
│       └── MappingProfile.cs
├── Cars/
│   ├── Commands/
│   │   ├── CreateCar/
│   │   │   ├── CreateCarCommand.cs
│   │   │   ├── CreateCarCommandValidator.cs
│   │   │   └── CreateCarCommandHandler.cs
│   │   └── UpdateCar/
│   ├── Queries/
│   │   ├── GetCars/
│   │   │   ├── GetCarsQuery.cs
│   │   │   └── GetCarsQueryHandler.cs
│   │   └── GetCarById/
│   └── DTOs/
│       ├── CarDto.cs
│       └── CarDetailDto.cs
└── DependencyInjection.cs
```

**Example Command:**
```csharp
public record CreateCarCommand(
    string Make,
    string Model,
    int Year,
    decimal Price
) : IRequest<Guid>;

public class CreateCarCommandValidator : AbstractValidator<CreateCarCommand>
{
    public CreateCarCommandValidator()
    {
        RuleFor(x => x.Make).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Year).GreaterThan(1900).LessThanOrEqualTo(DateTime.Now.Year + 1);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

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
            new Money(request.Price)
        );

        await _repository.AddAsync(car, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return car.Id;
    }
}
```

### 3. Infrastructure Layer
Implements interfaces defined in the Application layer.

**Characteristics:**
- Contains data access implementations (EF Core)
- External service integrations
- File system access, email, etc.
- Depends on Application and Domain layers

**Structure:**
```
Infrastructure/
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   │   ├── CarConfiguration.cs
│   │   └── OrderConfiguration.cs
│   ├── Migrations/
│   └── Repositories/
│       ├── Repository.cs
│       └── CarRepository.cs
├── Services/
│   ├── DateTimeService.cs
│   ├── EmailService.cs
│   └── FileStorageService.cs
├── Identity/
│   └── IdentityService.cs
└── DependencyInjection.cs
```

**Example Repository:**
```csharp
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }
}
```

### 4. Presentation Layer (API + React)
The outermost layer that users interact with.

**API Structure:**
```
API/
├── Controllers/
│   ├── CarsController.cs
│   └── OrdersController.cs
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Filters/
│   └── ValidationFilter.cs
├── Program.cs
└── appsettings.json
```

**Example Controller:**
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class CarsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CarsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<CarDto>>> GetCars()
    {
        var query = new GetCarsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateCar([FromBody] CreateCarCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCarById), new { id }, id);
    }
}
```

## React Project Structure

Following modern React best practices with feature-based organization:

```
src/
├── app/
│   ├── App.tsx
│   ├── App.test.tsx
│   └── store.ts                 # Global store if using Redux/Zustand
├── features/
│   ├── cars/
│   │   ├── components/
│   │   │   ├── CarCard.tsx
│   │   │   ├── CarList.tsx
│   │   │   └── CarForm.tsx
│   │   ├── hooks/
│   │   │   ├── useCars.ts
│   │   │   └── useCarForm.ts
│   │   ├── api/
│   │   │   └── carsApi.ts
│   │   ├── types/
│   │   │   └── car.types.ts
│   │   └── __tests__/
│   │       └── CarList.test.tsx
│   ├── orders/
│   └── auth/
├── shared/
│   ├── components/
│   │   ├── Button/
│   │   │   ├── Button.tsx
│   │   │   ├── Button.test.tsx
│   │   │   └── Button.module.css
│   │   ├── Input/
│   │   └── Layout/
│   ├── hooks/
│   │   ├── useLocalStorage.ts
│   │   └── useDebounce.ts
│   ├── utils/
│   │   ├── formatters.ts
│   │   └── validators.ts
│   └── constants/
│       └── api.constants.ts
├── services/
│   ├── api.service.ts           # Axios/Fetch configuration
│   └── auth.service.ts
├── types/
│   └── global.types.ts
├── styles/
│   ├── global.css
│   └── variables.css
├── config/
│   └── env.ts
└── main.tsx
```

## Integration Patterns

### Approach 1: Integrated SPA Template (Recommended for Small-Medium Projects)

Single project containing both API and React app:

```
ProjectName/
├── Controllers/              # ASP.NET Core API
├── ClientApp/               # React application
│   ├── src/
│   ├── public/
│   └── package.json
├── Program.cs
└── ProjectName.csproj
```

**Benefits:**
- Single deployment unit
- Simplified development setup
- Built-in proxy during development
- Easier for small teams

**Program.cs Configuration:**
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
```

### Approach 2: Separate Projects (Recommended for Large/Enterprise Projects)

Separate backend API and frontend SPA:

```
Solution/
├── src/
│   ├── Domain/
│   ├── Application/
│   ├── Infrastructure/
│   ├── API/
│   └── Web/                 # React app (separate project)
└── tests/
```

**Benefits:**
- Independent deployment
- Better separation of concerns
- Easier to scale teams
- Can use different hosting strategies

## Key Architectural Principles

### 1. Dependency Inversion Principle
- Inner layers define interfaces
- Outer layers implement those interfaces
- Dependencies always point inward

### 2. Clear Boundaries
- Each layer has a single responsibility
- Avoid circular dependencies
- Use DTOs for layer communication

### 3. Independent Domain Layer
- No technology-specific dependencies
- Contains pure business logic
- Easily testable in isolation

### 4. Feature-Based Organization (React)
- Group by feature, not by file type
- Co-locate related components, hooks, and tests
- Makes features easier to understand and maintain

### 5. API as a Contract
- Backend exposes well-defined REST APIs
- Frontend consumes APIs without knowing implementation
- Changes in one layer don't cascade to others

## Best Practices

### DO:
- Keep Domain layer free of dependencies
- Use interfaces for abstraction
- Apply SOLID principles consistently
- Organize React code by feature
- Use TypeScript for type safety
- Write tests for each layer
- Document your architecture decisions

### DON'T:
- Reference outer layers from inner layers
- Put business logic in controllers or components
- Mix concerns between layers
- Create circular dependencies
- Use global state unnecessarily
- Skip validation at boundaries
- Couple your code to specific frameworks

## Recommended Patterns

1. **CQRS (Command Query Responsibility Segregation)**: Separate read and write operations
2. **Mediator Pattern**: Use MediatR for decoupled request handling
3. **Repository Pattern**: Abstract data access
4. **Unit of Work Pattern**: Manage transactions
5. **Custom Hooks (React)**: Encapsulate component logic
6. **Composition over Inheritance**: Build complex UIs from simple components

## Migration Strategy

If working with an existing monolithic application:

1. Start by defining clear domain entities
2. Create the application layer with interfaces
3. Gradually move business logic from controllers to handlers
4. Implement repositories and move data access
5. Refactor frontend to feature-based organization
6. Add tests at each step

## Tools & Templates

- **Jason Taylor's CleanArchitecture Template**: Official .NET 9 template on GitHub
- **Create React App**: Quick start for React projects
- **Vite**: Modern, fast alternative to CRA
- **Visual Studio ASP.NET Core with React template**: Integrated development experience

## Summary

Clean Architecture with React and ASP.NET Core provides a solid foundation for building maintainable, scalable applications. By keeping concerns separated and dependencies pointed inward, you create a system that's easy to test, understand, and evolve over time.
