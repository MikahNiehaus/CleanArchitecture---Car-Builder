# Testing Strategies

## Overview

Comprehensive testing ensures code quality, prevents regressions, and provides confidence when refactoring. This guide covers testing strategies for React and ASP.NET Core applications, from unit tests to end-to-end tests.

## Testing Pyramid

```
        /\
       /E2E\          <- Few (Slow, Expensive, High Confidence)
      /------\
     /Integration\    <- Some (Medium Speed, Medium Cost)
    /------------\
   /  Unit Tests  \   <- Many (Fast, Cheap, Focused)
  /----------------\
```

## React Testing

### Tools and Setup

**Core Dependencies:**
```json
{
    "devDependencies": {
        "@testing-library/react": "^15.0.0",
        "@testing-library/jest-dom": "^6.1.5",
        "@testing-library/user-event": "^14.5.1",
        "@vitest/ui": "^1.0.0",
        "vitest": "^1.0.0",
        "jsdom": "^23.0.0"
    }
}
```

**Vitest Configuration (vite.config.ts):**
```ts
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

export default defineConfig({
    plugins: [react()],
    test: {
        globals: true,
        environment: 'jsdom',
        setupFiles: './src/test/setup.ts',
        coverage: {
            provider: 'v8',
            reporter: ['text', 'json', 'html'],
            exclude: ['node_modules/', 'src/test/']
        }
    }
});
```

**Setup File (src/test/setup.ts):**
```ts
import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach } from 'vitest';

afterEach(() => {
    cleanup();
});
```

### 1. Component Testing Best Practices

**Test Behavior, Not Implementation:**

```tsx
// ❌ DON'T: Test implementation details
import { render } from '@testing-library/react';

test('CarCard component', () => {
    const { container } = render(<CarCard car={mockCar} />);
    expect(container.querySelector('.car-card')).toBeInTheDocument();
    // Fragile: breaks if you change CSS class
});

// ✅ DO: Test user-facing behavior
import { render, screen } from '@testing-library/react';

test('displays car information', () => {
    const car = { id: '1', make: 'Toyota', model: 'Camry', price: 25000 };
    render(<CarCard car={car} />);

    expect(screen.getByText('Toyota Camry')).toBeInTheDocument();
    expect(screen.getByText('$25,000')).toBeInTheDocument();
});
```

**Use Accessible Queries:**

```tsx
import { render, screen } from '@testing-library/react';

test('CarForm submission', async () => {
    const handleSubmit = vi.fn();
    render(<CarForm onSubmit={handleSubmit} />);

    // ✅ Prefer getByRole (most accessible)
    const makeInput = screen.getByRole('textbox', { name: /make/i });
    const submitButton = screen.getByRole('button', { name: /submit/i });

    // ✅ Good alternatives
    const modelInput = screen.getByLabelText(/model/i);
    const priceInput = screen.getByPlaceholderText(/enter price/i);

    // ❌ Avoid data-testid unless necessary
    const element = screen.getByTestId('car-form');
});
```

**Query Priority (from React Testing Library):**
1. `getByRole` - Most accessible
2. `getByLabelText` - Good for forms
3. `getByPlaceholderText` - For inputs
4. `getByText` - For non-interactive elements
5. `getByDisplayValue` - Current form values
6. `getByAltText` - For images
7. `getByTitle` - For title attributes
8. `getByTestId` - Last resort

### 2. Testing User Interactions

```tsx
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

describe('CarForm', () => {
    test('submits form with valid data', async () => {
        const user = userEvent.setup();
        const handleSubmit = vi.fn();

        render(<CarForm onSubmit={handleSubmit} />);

        // Type in inputs
        await user.type(screen.getByLabelText(/make/i), 'Toyota');
        await user.type(screen.getByLabelText(/model/i), 'Camry');
        await user.type(screen.getByLabelText(/year/i), '2024');

        // Click submit
        await user.click(screen.getByRole('button', { name: /submit/i }));

        // Assert
        expect(handleSubmit).toHaveBeenCalledWith({
            make: 'Toyota',
            model: 'Camry',
            year: 2024
        });
    });

    test('shows validation errors', async () => {
        const user = userEvent.setup();
        render(<CarForm onSubmit={vi.fn()} />);

        // Submit without filling form
        await user.click(screen.getByRole('button', { name: /submit/i }));

        // Check for error messages
        expect(screen.getByText(/make is required/i)).toBeInTheDocument();
        expect(screen.getByText(/model is required/i)).toBeInTheDocument();
    });
});
```

### 3. Testing Async Operations

```tsx
import { render, screen, waitFor } from '@testing-library/react';

describe('CarList', () => {
    test('loads and displays cars', async () => {
        const mockCars = [
            { id: '1', make: 'Toyota', model: 'Camry', price: 25000 },
            { id: '2', make: 'Honda', model: 'Accord', price: 27000 }
        ];

        // Mock API call
        global.fetch = vi.fn(() =>
            Promise.resolve({
                ok: true,
                json: () => Promise.resolve(mockCars)
            })
        ) as any;

        render(<CarList />);

        // Initially shows loading
        expect(screen.getByText(/loading/i)).toBeInTheDocument();

        // Wait for cars to appear
        await waitFor(() => {
            expect(screen.getByText('Toyota Camry')).toBeInTheDocument();
        });

        expect(screen.getByText('Honda Accord')).toBeInTheDocument();
    });

    test('displays error on fetch failure', async () => {
        global.fetch = vi.fn(() => Promise.reject(new Error('API Error')));

        render(<CarList />);

        await waitFor(() => {
            expect(screen.getByText(/error loading cars/i)).toBeInTheDocument();
        });
    });
});
```

### 4. Testing Custom Hooks

```tsx
import { renderHook, waitFor } from '@testing-library/react';

describe('useCars', () => {
    test('fetches cars successfully', async () => {
        const mockCars = [{ id: '1', make: 'Toyota', model: 'Camry' }];

        global.fetch = vi.fn(() =>
            Promise.resolve({
                ok: true,
                json: () => Promise.resolve(mockCars)
            })
        ) as any;

        const { result } = renderHook(() => useCars());

        expect(result.current.loading).toBe(true);

        await waitFor(() => {
            expect(result.current.loading).toBe(false);
        });

        expect(result.current.cars).toEqual(mockCars);
        expect(result.current.error).toBeNull();
    });
});
```

### 5. Testing Context and State

```tsx
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

const TestWrapper = ({ children }: { children: ReactNode }) => (
    <CarProvider>
        <AuthProvider>
            {children}
        </AuthProvider>
    </CarProvider>
);

test('CarDetails uses context', async () => {
    const user = userEvent.setup();

    render(
        <TestWrapper>
            <CarDetails carId="1" />
        </TestWrapper>
    );

    await user.click(screen.getByRole('button', { name: /add to cart/i }));

    expect(screen.getByText(/added to cart/i)).toBeInTheDocument();
});
```

### 6. Mocking Modules

```tsx
// Mock external dependencies
vi.mock('../api/carsApi', () => ({
    fetchCars: vi.fn(() => Promise.resolve([
        { id: '1', make: 'Toyota', model: 'Camry' }
    ])),
    createCar: vi.fn()
}));

import { fetchCars, createCar } from '../api/carsApi';

test('CarList uses API', async () => {
    render(<CarList />);

    await waitFor(() => {
        expect(fetchCars).toHaveBeenCalled();
    });

    expect(screen.getByText('Toyota Camry')).toBeInTheDocument();
});
```

### 7. Snapshot Testing (Use Sparingly)

```tsx
import { render } from '@testing-library/react';

test('CarCard matches snapshot', () => {
    const car = { id: '1', make: 'Toyota', model: 'Camry', price: 25000 };
    const { container } = render(<CarCard car={car} />);

    expect(container).toMatchSnapshot();
});

// ⚠️ Snapshots are brittle and don't test behavior
// Use only for stable, design-system components
```

## ASP.NET Core Testing

### Tools and Setup

**Core Dependencies:**
```xml
<ItemGroup>
    <PackageReference Include="xUnit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
    <PackageReference Include="Moq" Version="4.20.69" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
</ItemGroup>
```

### 1. Unit Testing

**Test Domain Logic:**
```csharp
public class CarTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesCar()
    {
        // Arrange & Act
        var car = new Car("Toyota", "Camry", 2024, new Money(25000));

        // Assert
        car.Make.Should().Be("Toyota");
        car.Model.Should().Be("Camry");
        car.Year.Should().Be(2024);
        car.Price.Amount.Should().Be(25000);
    }

    [Fact]
    public void Constructor_WithEmptyMake_ThrowsException()
    {
        // Arrange & Act
        Action act = () => new Car("", "Camry", 2024, new Money(25000));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Make cannot be empty");
    }

    [Theory]
    [InlineData(1899)]
    [InlineData(2026)]
    public void Constructor_WithInvalidYear_ThrowsException(int year)
    {
        Action act = () => new Car("Toyota", "Camry", year, new Money(25000));

        act.Should().Throw<DomainException>();
    }
}
```

**Test Application Layer:**
```csharp
public class CreateCarCommandHandlerTests
{
    private readonly Mock<IRepository<Car>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly CreateCarCommandHandler _handler;

    public CreateCarCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Car>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new CreateCarCommandHandler(_mockRepository.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesCar()
    {
        // Arrange
        var command = new CreateCarCommand("Toyota", "Camry", 2024, 25000);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Car>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        _mockRepository.Verify(
            r => r.AddAsync(It.Is<Car>(c => c.Make == "Toyota" && c.Model == "Camry"), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _mockUnitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
```

**Test with FluentValidation:**
```csharp
public class CreateCarCommandValidatorTests
{
    private readonly CreateCarCommandValidator _validator;

    public CreateCarCommandValidatorTests()
    {
        _validator = new CreateCarCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_PassesValidation()
    {
        var command = new CreateCarCommand("Toyota", "Camry", 2024, 25000);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyMake_FailsValidation()
    {
        var command = new CreateCarCommand("", "Camry", 2024, 25000);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Make");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public void Validate_WithInvalidPrice_FailsValidation(decimal price)
    {
        var command = new CreateCarCommand("Toyota", "Camry", 2024, price);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }
}
```

### 2. Integration Testing

**Custom Web Application Factory:**
```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add InMemory database
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Build service provider and create database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();

            // Seed test data
            SeedTestData(db);
        });
    }

    private void SeedTestData(ApplicationDbContext context)
    {
        context.Cars.AddRange(
            new Car("Toyota", "Camry", 2024, new Money(25000)),
            new Car("Honda", "Accord", 2024, new Money(27000))
        );
        context.SaveChanges();
    }
}
```

**Integration Tests:**
```csharp
public class CarsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public CarsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCars_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/cars");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetCars_ReturnsAllCars()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/cars");
        var content = await response.Content.ReadAsStringAsync();
        var cars = JsonSerializer.Deserialize<List<CarDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        cars.Should().HaveCount(2);
        cars.Should().Contain(c => c.Make == "Toyota");
        cars.Should().Contain(c => c.Make == "Honda");
    }

    [Fact]
    public async Task CreateCar_WithValidData_ReturnsCreated()
    {
        // Arrange
        var newCar = new CreateCarCommand("Tesla", "Model 3", 2024, 45000);
        var content = new StringContent(
            JsonSerializer.Serialize(newCar),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client.PostAsync("/api/v1/cars", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateCar_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var invalidCar = new { Make = "", Model = "Model 3", Year = 2024, Price = -1000 };
        var content = new StringContent(
            JsonSerializer.Serialize(invalidCar),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client.PostAsync("/api/v1/cars", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

### 3. Repository Testing

```csharp
public class CarRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly CarRepository _repository;

    public CarRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new CarRepository(_context);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _context.Cars.AddRange(
            new Car("Toyota", "Camry", 2024, new Money(25000)),
            new Car("Honda", "Accord", 2024, new Money(27000))
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsCar()
    {
        // Arrange
        var car = _context.Cars.First();

        // Act
        var result = await _repository.GetByIdAsync(car.Id);

        // Assert
        result.Should().NotBeNull();
        result.Make.Should().Be(car.Make);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCars()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

## Testing Best Practices

### AAA Pattern (Arrange-Act-Assert)

```csharp
[Fact]
public async Task Example_Test()
{
    // Arrange: Set up test data and dependencies
    var car = new Car("Toyota", "Camry", 2024, new Money(25000));
    var mockRepo = new Mock<IRepository<Car>>();

    // Act: Execute the behavior being tested
    var result = await service.DoSomething(car);

    // Assert: Verify the outcome
    result.Should().NotBeNull();
}
```

### Test Organization

```
tests/
├── UnitTests/
│   ├── Domain/
│   │   └── CarTests.cs
│   ├── Application/
│   │   ├── Commands/
│   │   │   └── CreateCarCommandHandlerTests.cs
│   │   └── Queries/
│   │       └── GetCarsQueryHandlerTests.cs
│   └── Infrastructure/
│       └── Repositories/
│           └── CarRepositoryTests.cs
├── IntegrationTests/
│   ├── Controllers/
│   │   └── CarsControllerTests.cs
│   └── CustomWebApplicationFactory.cs
└── E2ETests/
    └── CarWorkflowTests.cs
```

### Test Naming Conventions

```csharp
// Pattern: MethodName_StateUnderTest_ExpectedBehavior

[Fact]
public void CreateCar_WithEmptyMake_ThrowsDomainException() { }

[Fact]
public void CalculateDiscount_ForNewCar_ReturnsZeroDiscount() { }

[Fact]
public void GetCarById_WithValidId_ReturnsCar() { }

[Fact]
public void GetCarById_WithInvalidId_ReturnsNull() { }
```

## Testing Checklist

### React
- [ ] Test component behavior, not implementation
- [ ] Use accessible queries (getByRole, getByLabelText)
- [ ] Test user interactions with userEvent
- [ ] Mock external dependencies
- [ ] Test async operations with waitFor
- [ ] Test error states
- [ ] Test loading states
- [ ] Achieve >80% code coverage for critical paths
- [ ] Keep tests isolated and independent

### ASP.NET Core
- [ ] Unit test domain logic
- [ ] Unit test application layer (commands/queries)
- [ ] Integration test API endpoints
- [ ] Test validation rules
- [ ] Test exception handling
- [ ] Mock external dependencies
- [ ] Use InMemory database for integration tests
- [ ] Test authentication/authorization
- [ ] Achieve >80% code coverage

### General
- [ ] Follow AAA pattern
- [ ] One assertion per test (when possible)
- [ ] Clear, descriptive test names
- [ ] Fast execution (<5 min for full suite)
- [ ] Deterministic (no flaky tests)
- [ ] Independent (tests don't depend on each other)
- [ ] Automated in CI/CD pipeline
- [ ] Regularly review and maintain tests

## Summary

Effective testing requires the right balance of unit, integration, and end-to-end tests. Focus on testing behavior rather than implementation, use appropriate tools for each layer, and maintain tests as carefully as production code.
