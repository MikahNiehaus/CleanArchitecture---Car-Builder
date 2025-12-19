# Code Organization & Clean Code Practices

## Overview

This guide covers best practices for writing clean, maintainable code in React and ASP.NET Core applications. Following these guidelines ensures your codebase remains readable, testable, and scalable as it grows.

## SOLID Principles

### Single Responsibility Principle (SRP)
Each class/component should have one reason to change.

**ASP.NET Core Example:**

**Bad:**
```csharp
public class UserService
{
    public void CreateUser(User user) { }
    public void SendWelcomeEmail(User user) { }
    public void LogUserCreation(User user) { }
    public void ValidateUser(User user) { }
}
```

**Good:**
```csharp
public class UserService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<UserService> _logger;
    private readonly IUserValidator _validator;

    public async Task CreateUserAsync(User user)
    {
        _validator.Validate(user);
        // Create user logic
        await _emailService.SendWelcomeEmailAsync(user);
        _logger.LogInformation("User created: {UserId}", user.Id);
    }
}
```

**React Example:**

**Bad:**
```tsx
function UserProfile() {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        // Fetch user
        // Handle validation
        // Log analytics
        // Update UI
    }, []);

    // Component does too much
}
```

**Good:**
```tsx
function UserProfile() {
    const { user, loading, error } = useUser();

    if (loading) return <LoadingSpinner />;
    if (error) return <ErrorDisplay error={error} />;

    return <UserDetails user={user} />;
}

// Separate concerns into custom hooks
function useUser() {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        fetchUser().then(setUser).catch(setError).finally(() => setLoading(false));
    }, []);

    return { user, loading, error };
}
```

### Open/Closed Principle (OCP)
Open for extension, closed for modification.

**ASP.NET Core Example:**
```csharp
// Base abstraction
public interface INotificationService
{
    Task SendAsync(string message, string recipient);
}

// Implementations can be extended without modifying existing code
public class EmailNotificationService : INotificationService
{
    public async Task SendAsync(string message, string recipient)
    {
        // Email implementation
    }
}

public class SmsNotificationService : INotificationService
{
    public async Task SendAsync(string message, string recipient)
    {
        // SMS implementation
    }
}

// Usage with dependency injection
public class OrderService
{
    private readonly IEnumerable<INotificationService> _notificationServices;

    public OrderService(IEnumerable<INotificationService> notificationServices)
    {
        _notificationServices = notificationServices;
    }
}
```

### Liskov Substitution Principle (LSP)
Derived classes must be substitutable for their base classes.

### Interface Segregation Principle (ISP)
Clients should not depend on interfaces they don't use.

**Good:**
```csharp
public interface IReadRepository<T>
{
    Task<T> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
}

public interface IWriteRepository<T>
{
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}

// Clients use only what they need
public class ReadOnlyCarService
{
    private readonly IReadRepository<Car> _repository;
}
```

### Dependency Inversion Principle (DIP)
Depend on abstractions, not concretions.

**ASP.NET Core:**
```csharp
// Abstraction defined in Application layer
public interface ICarRepository
{
    Task<Car> GetByIdAsync(Guid id);
}

// Concrete implementation in Infrastructure layer
public class CarRepository : ICarRepository
{
    private readonly ApplicationDbContext _context;

    public CarRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Car> GetByIdAsync(Guid id)
    {
        return await _context.Cars.FindAsync(id);
    }
}
```

## React Best Practices (2025)

### 1. Functional Components with Hooks
Always use functional components. Class components are legacy.

```tsx
// Modern approach
function CarList() {
    const [cars, setCars] = useState<Car[]>([]);

    useEffect(() => {
        fetchCars().then(setCars);
    }, []);

    return <div>{cars.map(car => <CarCard key={car.id} car={car} />)}</div>;
}
```

### 2. Custom Hooks for Reusable Logic
Extract reusable logic into custom hooks.

```tsx
function useFetch<T>(url: string) {
    const [data, setData] = useState<T | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<Error | null>(null);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const response = await fetch(url);
                const json = await response.json();
                setData(json);
            } catch (err) {
                setError(err as Error);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [url]);

    return { data, loading, error };
}

// Usage
function CarList() {
    const { data: cars, loading, error } = useFetch<Car[]>('/api/cars');

    if (loading) return <Spinner />;
    if (error) return <Error message={error.message} />;

    return <div>{cars?.map(car => <CarCard key={car.id} car={car} />)}</div>;
}
```

### 3. Composition Over Complex Components
Build complex UIs from simple, composable components.

```tsx
// Bad: Monolithic component
function CarDashboard() {
    return (
        <div>
            {/* 500 lines of JSX */}
        </div>
    );
}

// Good: Composed from smaller components
function CarDashboard() {
    return (
        <DashboardLayout>
            <CarStats />
            <CarList />
            <CarFilters />
        </DashboardLayout>
    );
}
```

### 4. Prop Destructuring and Default Values
```tsx
interface CarCardProps {
    car: Car;
    onSelect?: (car: Car) => void;
    showDetails?: boolean;
}

function CarCard({
    car,
    onSelect,
    showDetails = false
}: CarCardProps) {
    return (
        <div onClick={() => onSelect?.(car)}>
            <h3>{car.make} {car.model}</h3>
            {showDetails && <CarDetails car={car} />}
        </div>
    );
}
```

### 5. Avoid Prop Drilling with Context
```tsx
// Create context for deeply nested data
const CarContext = createContext<CarContextType | undefined>(undefined);

function CarProvider({ children }: { children: ReactNode }) {
    const [selectedCar, setSelectedCar] = useState<Car | null>(null);

    return (
        <CarContext.Provider value={{ selectedCar, setSelectedCar }}>
            {children}
        </CarContext.Provider>
    );
}

// Custom hook for consuming context
function useCarContext() {
    const context = useContext(CarContext);
    if (!context) throw new Error('useCarContext must be used within CarProvider');
    return context;
}
```

### 6. Proper TypeScript Usage
```tsx
// Define clear types
interface Car {
    id: string;
    make: string;
    model: string;
    year: number;
    price: number;
}

type CarStatus = 'available' | 'reserved' | 'sold';

interface CarWithStatus extends Car {
    status: CarStatus;
}

// Use generics for reusable components
interface ListProps<T> {
    items: T[];
    renderItem: (item: T) => ReactNode;
    keyExtractor: (item: T) => string;
}

function List<T>({ items, renderItem, keyExtractor }: ListProps<T>) {
    return <>{items.map(item => <div key={keyExtractor(item)}>{renderItem(item)}</div>)}</>;
}
```

### 7. Memoization - Use Sparingly and Wisely
Only use `useMemo` and `useCallback` when you've measured a performance problem.

```tsx
function CarList({ cars, onCarSelect }: CarListProps) {
    // ❌ DON'T: Premature optimization
    const sortedCars = useMemo(() => cars.sort((a, b) => a.price - b.price), [cars]);

    // ✅ DO: Only if sorting is expensive and causes measured performance issues
    // Most operations are fast enough without memoization

    // ✅ DO: Memoize callbacks passed to memoized children
    const handleSelect = useCallback((car: Car) => {
        onCarSelect(car);
    }, [onCarSelect]);

    return <MemoizedCarCard onSelect={handleSelect} />;
}
```

### 8. React 19+ Features
Take advantage of React 19 features when applicable:

```tsx
// useActionState for form handling
function CarForm() {
    const [state, formAction] = useActionState(createCar, initialState);

    return (
        <form action={formAction}>
            <input name="make" />
            {state.error && <span>{state.error}</span>}
        </form>
    );
}

// useOptimistic for optimistic UI updates
function CarList() {
    const [cars, setCars] = useState<Car[]>([]);
    const [optimisticCars, addOptimisticCar] = useOptimistic(
        cars,
        (state, newCar: Car) => [...state, newCar]
    );

    // Use optimisticCars for rendering
}
```

## ASP.NET Core Best Practices

### 1. Use Dependency Injection Properly
```csharp
// Register services by lifetime
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Singleton: One instance for app lifetime
        services.AddSingleton<IDateTimeService, DateTimeService>();

        // Scoped: One instance per request
        services.AddScoped<ICarRepository, CarRepository>();

        // Transient: New instance every time
        services.AddTransient<IEmailService, EmailService>();

        // MediatR for CQRS
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        return services;
    }
}
```

### 2. Use Records for DTOs and Value Objects
```csharp
// Immutable DTOs with records
public record CarDto(
    Guid Id,
    string Make,
    string Model,
    int Year,
    decimal Price
);

// Value objects
public record Money(decimal Amount, string Currency = "USD")
{
    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot add different currencies");

        return new Money(a.Amount + b.Amount, a.Currency);
    }
}
```

### 3. Async/Await Best Practices
```csharp
// ✅ DO: Use async all the way
public async Task<Car> GetCarAsync(Guid id)
{
    return await _repository.GetByIdAsync(id);
}

// ❌ DON'T: Block async code
public Car GetCar(Guid id)
{
    return _repository.GetByIdAsync(id).Result; // Deadlock risk!
}

// ✅ DO: Use ConfigureAwait(false) in libraries (not in ASP.NET Core controllers)
public async Task<Car> GetCarAsync(Guid id)
{
    return await _repository.GetByIdAsync(id).ConfigureAwait(false);
}

// ✅ DO: Use cancellation tokens
public async Task<Car> GetCarAsync(Guid id, CancellationToken cancellationToken = default)
{
    return await _repository.GetByIdAsync(id, cancellationToken);
}
```

### 4. Use FluentValidation
```csharp
public class CreateCarCommandValidator : AbstractValidator<CreateCarCommand>
{
    public CreateCarCommandValidator()
    {
        RuleFor(x => x.Make)
            .NotEmpty().WithMessage("Make is required")
            .MaximumLength(50).WithMessage("Make must not exceed 50 characters");

        RuleFor(x => x.Year)
            .GreaterThan(1900)
            .LessThanOrEqualTo(DateTime.Now.Year + 1);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero");
    }
}
```

### 5. Use Pattern Matching
```csharp
// Modern C# pattern matching
public decimal CalculateDiscount(Car car) => car switch
{
    { Year: < 2015 } => 0.20m,
    { Year: < 2020, Price: > 50000 } => 0.15m,
    { Make: "Tesla" } => 0.10m,
    _ => 0.05m
};

// Null checking
if (car is not null)
{
    // Process car
}
```

### 6. Use Primary Constructors (C# 12)
```csharp
// Modern approach with primary constructor
public class CarService(ICarRepository repository, ILogger<CarService> logger)
{
    public async Task<Car> GetCarAsync(Guid id)
    {
        logger.LogInformation("Fetching car {CarId}", id);
        return await repository.GetByIdAsync(id);
    }
}
```

### 7. Proper Exception Handling
```csharp
// Custom exceptions
public class CarNotFoundException : Exception
{
    public Guid CarId { get; }

    public CarNotFoundException(Guid carId)
        : base($"Car with ID {carId} was not found")
    {
        CarId = carId;
    }
}

// Global exception handling middleware
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An error occurred");

        var response = exception switch
        {
            CarNotFoundException => (StatusCodes.Status404NotFound, "Car not found"),
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            _ => (StatusCodes.Status500InternalServerError, "Internal server error")
        };

        context.Response.StatusCode = response.Item1;
        await context.Response.WriteAsJsonAsync(new { error = response.Item2 });
    }
}
```

## Code Organization Checklist

### React
- [ ] Use functional components exclusively
- [ ] Extract logic into custom hooks
- [ ] Keep components small and focused
- [ ] Use TypeScript for type safety
- [ ] Organize by feature, not file type
- [ ] Implement proper error boundaries
- [ ] Use composition over inheritance
- [ ] Avoid premature optimization

### ASP.NET Core
- [ ] Follow Clean Architecture layers
- [ ] Use dependency injection properly
- [ ] Implement CQRS pattern
- [ ] Use async/await consistently
- [ ] Validate input with FluentValidation
- [ ] Handle exceptions globally
- [ ] Use records for DTOs
- [ ] Apply SOLID principles

### General
- [ ] Write self-documenting code
- [ ] Use meaningful variable names
- [ ] Keep functions small (< 20 lines ideal)
- [ ] DRY (Don't Repeat Yourself)
- [ ] YAGNI (You Aren't Gonna Need It)
- [ ] KISS (Keep It Simple, Stupid)
- [ ] Write tests for critical logic
- [ ] Document complex business rules

## Naming Conventions

### React
- **Components**: PascalCase (`CarList`, `UserProfile`)
- **Hooks**: camelCase with `use` prefix (`useCarData`, `useFetch`)
- **Constants**: UPPER_SNAKE_CASE (`API_BASE_URL`)
- **Interfaces/Types**: PascalCase with `I` prefix optional (`CarProps`, `IUser`)

### C#
- **Classes/Interfaces**: PascalCase (`CarService`, `ICarRepository`)
- **Methods**: PascalCase (`GetCarAsync`, `CalculatePrice`)
- **Private fields**: _camelCase with underscore (`_repository`, `_logger`)
- **Properties**: PascalCase (`CarId`, `TotalPrice`)
- **Parameters**: camelCase (`carId`, `userName`)

## Code Review Checklist

- [ ] Does the code follow SOLID principles?
- [ ] Are there any code smells (duplicated code, long methods, etc.)?
- [ ] Is error handling appropriate?
- [ ] Are edge cases considered?
- [ ] Is the code testable?
- [ ] Are naming conventions followed?
- [ ] Is TypeScript used effectively (no `any` types)?
- [ ] Are there any security concerns?
- [ ] Is performance acceptable?
- [ ] Is the code documented where necessary?

## Summary

Clean code is not about following rules blindly—it's about writing code that's easy to understand, maintain, and evolve. By applying SOLID principles, using modern language features, and organizing code thoughtfully, you create a codebase that serves your team well for years to come.
