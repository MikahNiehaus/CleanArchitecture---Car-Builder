# API Design & Integration

## Overview

Well-designed APIs are the backbone of modern web applications. This guide covers RESTful API design principles, versioning strategies, integration patterns, and best practices for ASP.NET Core Web APIs consumed by React frontends.

## RESTful API Design Principles

### 1. Resource-Based URLs

Design URLs around resources (nouns), not actions (verbs).

```
✅ GOOD:
GET    /api/cars              - Get all cars
GET    /api/cars/123          - Get specific car
POST   /api/cars              - Create new car
PUT    /api/cars/123          - Update car
DELETE /api/cars/123          - Delete car

❌ BAD:
GET    /api/getAllCars
GET    /api/getCarById?id=123
POST   /api/createCar
POST   /api/updateCar
POST   /api/deleteCar
```

### 2. Use HTTP Methods Correctly

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class CarsController : ControllerBase
{
    // GET - Retrieve resources (idempotent, safe)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CarDto>>> GetCars(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var cars = await _mediator.Send(new GetCarsQuery(page, pageSize));
        return Ok(cars);
    }

    // GET by ID - Retrieve single resource
    [HttpGet("{id}")]
    public async Task<ActionResult<CarDto>> GetCar(Guid id)
    {
        var car = await _mediator.Send(new GetCarByIdQuery(id));

        if (car == null)
            return NotFound();

        return Ok(car);
    }

    // POST - Create new resource (not idempotent)
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateCar([FromBody] CreateCarCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCar), new { id }, id);
    }

    // PUT - Update entire resource (idempotent)
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCar(Guid id, [FromBody] UpdateCarCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await _mediator.Send(command);
        return NoContent();
    }

    // PATCH - Partial update (idempotent)
    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchCar(Guid id, [FromBody] JsonPatchDocument<CarDto> patchDoc)
    {
        // Apply partial updates
        return NoContent();
    }

    // DELETE - Remove resource (idempotent)
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCar(Guid id)
    {
        await _mediator.Send(new DeleteCarCommand(id));
        return NoContent();
    }
}
```

### 3. HTTP Status Codes

Use appropriate status codes to communicate results:

```csharp
public class CarsController : ControllerBase
{
    // 200 OK - Successful GET, PUT, PATCH, DELETE
    [HttpGet("{id}")]
    public async Task<ActionResult<CarDto>> GetCar(Guid id)
    {
        var car = await _service.GetCarAsync(id);
        return Ok(car); // 200
    }

    // 201 Created - Successful POST with resource creation
    [HttpPost]
    public async Task<ActionResult> CreateCar([FromBody] CreateCarCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCar), new { id }, new { id }); // 201
    }

    // 204 No Content - Successful request with no response body
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCar(Guid id)
    {
        await _service.DeleteCarAsync(id);
        return NoContent(); // 204
    }

    // 400 Bad Request - Invalid client input
    [HttpPost]
    public async Task<ActionResult> CreateCar([FromBody] CreateCarCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState); // 400

        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCar), new { id }, new { id });
    }

    // 401 Unauthorized - Authentication required
    [Authorize]
    [HttpGet]
    public async Task<ActionResult> GetCars()
    {
        // Returns 401 if not authenticated
    }

    // 403 Forbidden - Authenticated but not authorized
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCar(Guid id)
    {
        // Returns 403 if not in Admin role
    }

    // 404 Not Found - Resource doesn't exist
    [HttpGet("{id}")]
    public async Task<ActionResult<CarDto>> GetCar(Guid id)
    {
        var car = await _service.GetCarAsync(id);

        if (car == null)
            return NotFound(); // 404

        return Ok(car);
    }

    // 409 Conflict - Request conflicts with current state
    [HttpPost]
    public async Task<ActionResult> CreateCar([FromBody] CreateCarCommand command)
    {
        if (await _service.CarExistsAsync(command.VIN))
            return Conflict("Car with this VIN already exists"); // 409

        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCar), new { id }, new { id });
    }

    // 500 Internal Server Error - Handled by global exception middleware
}
```

### 4. Use DTOs (Data Transfer Objects)

Never expose domain entities directly. Use DTOs to control data shape.

```csharp
// Domain Entity (internal)
public class Car
{
    public Guid Id { get; private set; }
    public string Make { get; private set; }
    public string Model { get; private set; }
    public int Year { get; private set; }
    public Money Price { get; private set; }
    public string InternalNotes { get; private set; } // Sensitive
    public decimal CostPrice { get; private set; }     // Sensitive

    // Business logic methods
}

// DTO for API responses (external)
public record CarDto(
    Guid Id,
    string Make,
    string Model,
    int Year,
    decimal Price
);

// AutoMapper configuration
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Car, CarDto>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Amount));
    }
}
```

## API Versioning

### Why Version APIs?

- Allow breaking changes without affecting existing clients
- Support multiple API versions simultaneously
- Provide transition period for deprecation

### Versioning Strategies

**1. URL/Route Versioning (Recommended)**

```csharp
// Program.cs
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Controller
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class CarsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CarDto>>> GetCars()
    {
        // Version 1.0 implementation
    }
}

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
public class CarsV2Controller : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CarDtoV2>>> GetCars()
    {
        // Version 2.0 with additional fields
    }
}
```

**2. Query String Versioning**

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new QueryStringApiVersionReader("api-version");
});

// Usage: GET /api/cars?api-version=2.0
```

**3. Header Versioning**

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new HeaderApiVersionReader("X-API-Version");
});

// Usage: Header - X-API-Version: 2.0
```

**4. Media Type Versioning**

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new MediaTypeApiVersionReader();
});

// Usage: Accept: application/json; version=2.0
```

### Deprecating Versions

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0", Deprecated = true)]
public class CarsController : ControllerBase
{
    // This version is deprecated
}

// Response includes: api-deprecated-versions: 1.0
```

## Pagination, Filtering, and Sorting

### Pagination

```csharp
public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

[HttpGet]
public async Task<ActionResult<PagedResult<CarDto>>> GetCars(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
{
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100; // Max limit

    var result = await _mediator.Send(new GetCarsQuery(page, pageSize));

    // Add pagination headers
    Response.Headers.Add("X-Pagination",
        JsonSerializer.Serialize(new
        {
            result.TotalCount,
            result.TotalPages,
            result.Page,
            result.PageSize
        }));

    return Ok(result);
}
```

### Filtering

```csharp
public record GetCarsQuery(
    int Page,
    int PageSize,
    string? Make = null,
    int? MinYear = null,
    int? MaxYear = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null
) : IRequest<PagedResult<CarDto>>;

[HttpGet]
public async Task<ActionResult<PagedResult<CarDto>>> GetCars(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? make = null,
    [FromQuery] int? minYear = null,
    [FromQuery] int? maxYear = null,
    [FromQuery] decimal? minPrice = null,
    [FromQuery] decimal? maxPrice = null)
{
    var query = new GetCarsQuery(page, pageSize, make, minYear, maxYear, minPrice, maxPrice);
    var result = await _mediator.Send(query);
    return Ok(result);
}

// Usage: GET /api/cars?make=Toyota&minYear=2020&maxPrice=50000
```

### Sorting

```csharp
public record GetCarsQuery(
    int Page,
    int PageSize,
    string? SortBy = "make",
    bool SortDescending = false
) : IRequest<PagedResult<CarDto>>;

[HttpGet]
public async Task<ActionResult<PagedResult<CarDto>>> GetCars(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string sortBy = "make",
    [FromQuery] bool sortDescending = false)
{
    var allowedSortFields = new[] { "make", "model", "year", "price" };

    if (!allowedSortFields.Contains(sortBy.ToLower()))
        sortBy = "make";

    var query = new GetCarsQuery(page, pageSize, sortBy, sortDescending);
    var result = await _mediator.Send(query);
    return Ok(result);
}

// Usage: GET /api/cars?sortBy=price&sortDescending=true
```

## HATEOAS (Hypermedia)

```csharp
public record CarDto(
    Guid Id,
    string Make,
    string Model,
    int Year,
    decimal Price,
    List<Link> Links
);

public record Link(string Href, string Rel, string Method);

[HttpGet("{id}")]
public async Task<ActionResult<CarDto>> GetCar(Guid id)
{
    var car = await _service.GetCarAsync(id);

    if (car == null)
        return NotFound();

    var carDto = new CarDto(
        car.Id,
        car.Make,
        car.Model,
        car.Year,
        car.Price.Amount,
        new List<Link>
        {
            new Link($"/api/v1/cars/{id}", "self", "GET"),
            new Link($"/api/v1/cars/{id}", "update", "PUT"),
            new Link($"/api/v1/cars/{id}", "delete", "DELETE"),
            new Link($"/api/v1/orders", "create-order", "POST")
        }
    );

    return Ok(carDto);
}
```

## API Documentation with Swagger/OpenAPI

```csharp
// Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Car Builder API",
        Description = "API for managing cars and orders",
        Contact = new OpenApiContact
        {
            Name = "Support",
            Email = "support@example.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Add XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Add JWT authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Car Builder API v1");
    options.RoutePrefix = string.Empty; // Swagger at root
});

// Controller with XML documentation
/// <summary>
/// Gets all cars with pagination
/// </summary>
/// <param name="page">Page number (default: 1)</param>
/// <param name="pageSize">Items per page (default: 10, max: 100)</param>
/// <returns>Paginated list of cars</returns>
/// <response code="200">Returns the list of cars</response>
[HttpGet]
[ProducesResponseType(typeof(PagedResult<CarDto>), StatusCodes.Status200OK)]
public async Task<ActionResult<PagedResult<CarDto>>> GetCars(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
{
    // Implementation
}
```

## React Integration

### API Client Setup

```tsx
// src/services/api.service.ts
import axios, { AxiosInstance, AxiosRequestConfig } from 'axios';

class ApiService {
    private api: AxiosInstance;

    constructor() {
        this.api = axios.create({
            baseURL: import.meta.env.VITE_API_BASE_URL || '/api/v1',
            headers: {
                'Content-Type': 'application/json'
            },
            timeout: 10000
        });

        // Request interceptor for auth token
        this.api.interceptors.request.use(
            (config) => {
                const token = sessionStorage.getItem('authToken');
                if (token) {
                    config.headers.Authorization = `Bearer ${token}`;
                }
                return config;
            },
            (error) => Promise.reject(error)
        );

        // Response interceptor for error handling
        this.api.interceptors.response.use(
            (response) => response,
            (error) => {
                if (error.response?.status === 401) {
                    // Redirect to login
                    window.location.href = '/login';
                }
                return Promise.reject(error);
            }
        );
    }

    async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
        const response = await this.api.get<T>(url, config);
        return response.data;
    }

    async post<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
        const response = await this.api.post<T>(url, data, config);
        return response.data;
    }

    async put<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
        const response = await this.api.put<T>(url, data, config);
        return response.data;
    }

    async delete<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
        const response = await this.api.delete<T>(url, config);
        return response.data;
    }
}

export const apiService = new ApiService();
```

### Typed API Client

```tsx
// src/features/cars/api/carsApi.ts
import { apiService } from '@/services/api.service';

export interface Car {
    id: string;
    make: string;
    model: string;
    year: number;
    price: number;
}

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

export interface GetCarsParams {
    page?: number;
    pageSize?: number;
    make?: string;
    minYear?: number;
    maxYear?: number;
    sortBy?: string;
    sortDescending?: boolean;
}

export const carsApi = {
    getCars: (params?: GetCarsParams) =>
        apiService.get<PagedResult<Car>>('/cars', { params }),

    getCarById: (id: string) =>
        apiService.get<Car>(`/cars/${id}`),

    createCar: (car: Omit<Car, 'id'>) =>
        apiService.post<{ id: string }>('/cars', car),

    updateCar: (id: string, car: Omit<Car, 'id'>) =>
        apiService.put<void>(`/cars/${id}`, { ...car, id }),

    deleteCar: (id: string) =>
        apiService.delete<void>(`/cars/${id}`)
};
```

### Using React Query

```tsx
// src/features/cars/hooks/useCars.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { carsApi, type GetCarsParams, type Car } from '../api/carsApi';

export function useCars(params?: GetCarsParams) {
    return useQuery({
        queryKey: ['cars', params],
        queryFn: () => carsApi.getCars(params),
        staleTime: 5 * 60 * 1000 // 5 minutes
    });
}

export function useCarById(id: string) {
    return useQuery({
        queryKey: ['cars', id],
        queryFn: () => carsApi.getCarById(id),
        enabled: !!id
    });
}

export function useCreateCar() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: carsApi.createCar,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['cars'] });
        }
    });
}

export function useUpdateCar() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ id, car }: { id: string; car: Omit<Car, 'id'> }) =>
            carsApi.updateCar(id, car),
        onSuccess: (_, variables) => {
            queryClient.invalidateQueries({ queryKey: ['cars'] });
            queryClient.invalidateQueries({ queryKey: ['cars', variables.id] });
        }
    });
}

// Usage in component
function CarList() {
    const { data, isLoading, error } = useCars({ page: 1, pageSize: 10 });
    const createCar = useCreateCar();

    if (isLoading) return <Spinner />;
    if (error) return <Error message={error.message} />;

    return (
        <div>
            {data?.items.map(car => <CarCard key={car.id} car={car} />)}
        </div>
    );
}
```

## API Best Practices Checklist

- [ ] Use resource-based URLs
- [ ] Apply HTTP methods correctly
- [ ] Return appropriate status codes
- [ ] Use DTOs to control API shape
- [ ] Implement API versioning
- [ ] Support pagination for collections
- [ ] Allow filtering and sorting
- [ ] Validate all inputs
- [ ] Handle errors consistently
- [ ] Document with Swagger/OpenAPI
- [ ] Use HTTPS only
- [ ] Implement rate limiting
- [ ] Add correlation IDs for tracing
- [ ] Cache responses where appropriate
- [ ] Use ETags for conditional requests
- [ ] Support CORS properly
- [ ] Monitor API performance
- [ ] Version breaking changes
- [ ] Deprecate old versions gracefully

## Summary

Well-designed APIs are intuitive, consistent, and maintainable. Follow RESTful principles, use proper versioning, document thoroughly, and provide excellent developer experience for API consumers.
