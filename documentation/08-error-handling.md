# Error Handling & Logging

## Overview

Proper error handling and logging are essential for building robust, maintainable applications. This guide covers comprehensive strategies for handling errors gracefully and implementing effective logging in both React and ASP.NET Core applications.

## ASP.NET Core Error Handling

### 1. Global Exception Handling Middleware

```csharp
// Middleware/ExceptionHandlingMiddleware.cs
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
            ),

            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                notFoundEx.Message,
                null
            ),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                null
            ),

            ForbiddenAccessException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                null
            ),

            ConflictException conflictEx => (
                StatusCodes.Status409Conflict,
                conflictEx.Message,
                null
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An error occurred while processing your request",
                null
            )
        };

        context.Response.StatusCode = statusCode;

        var response = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            Errors = errors,
            TraceId = context.TraceIdentifier,
            // Only include details in development
            Details = _environment.IsDevelopment() ? exception.StackTrace : null
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}

public record ErrorResponse
{
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public object? Errors { get; init; }
    public string TraceId { get; init; } = string.Empty;
    public string? Details { get; init; }
}

// Program.cs
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

### 2. Custom Exceptions

```csharp
// Application/Common/Exceptions/CustomExceptions.cs
public abstract class CustomException : Exception
{
    protected CustomException(string message) : base(message) { }

    protected CustomException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class NotFoundException : CustomException
{
    public NotFoundException(string name, object key)
        : base($"{name} with id '{key}' was not found") { }
}

public class ValidationException : CustomException
{
    public IEnumerable<ValidationFailure> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> errors)
        : base("One or more validation failures occurred")
    {
        Errors = errors;
    }
}

public class ConflictException : CustomException
{
    public ConflictException(string message) : base(message) { }
}

public class ForbiddenAccessException : CustomException
{
    public ForbiddenAccessException() : base("Access denied") { }
}

// Usage in handlers
public class GetCarByIdQueryHandler : IRequestHandler<GetCarByIdQuery, CarDto>
{
    public async Task<CarDto> Handle(GetCarByIdQuery request, CancellationToken cancellationToken)
    {
        var car = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (car == null)
            throw new NotFoundException(nameof(Car), request.Id);

        return _mapper.Map<CarDto>(car);
    }
}
```

### 3. Validation with FluentValidation

```csharp
// Application/Common/Behaviours/ValidationBehaviour.cs
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}

// Register in Program.cs
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
```

### 4. Problem Details (RFC 7807)

```csharp
// Program.cs
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

        var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
    };
});

// Use in controllers
[HttpGet("{id}")]
public async Task<ActionResult<CarDto>> GetCar(Guid id)
{
    var car = await _service.GetCarAsync(id);

    if (car == null)
    {
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Car not found",
            detail: $"Car with ID {id} does not exist"
        );
    }

    return Ok(car);
}
```

## ASP.NET Core Logging

### 1. Structured Logging with Serilog

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.Seq
dotnet add package Serilog.Enrichers.Environment
dotnet add package Serilog.Enrichers.Thread
```

```csharp
// Program.cs
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Seq("http://localhost:5341") // Optional: Seq for log aggregation
    .CreateLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // ... rest of configuration

    var app = builder.Build();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

### 2. Structured Logging Best Practices

```csharp
public class CarService
{
    private readonly ILogger<CarService> _logger;

    public async Task<Car> GetCarAsync(Guid id)
    {
        // ✅ DO: Use structured logging
        _logger.LogInformation("Retrieving car with ID: {CarId}", id);

        var car = await _repository.GetByIdAsync(id);

        if (car == null)
        {
            _logger.LogWarning("Car not found: {CarId}", id);
            throw new NotFoundException(nameof(Car), id);
        }

        // ✅ DO: Log complex objects
        _logger.LogInformation("Car retrieved successfully: {@Car}", car);

        return car;
    }

    public async Task CreateCarAsync(CreateCarCommand command)
    {
        try
        {
            _logger.LogInformation("Creating car: {Make} {Model}", command.Make, command.Model);

            var car = new Car(command.Make, command.Model, command.Year, new Money(command.Price));
            await _repository.AddAsync(car);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Car created successfully with ID: {CarId}", car.Id);
        }
        catch (Exception ex)
        {
            // ✅ DO: Log exceptions with context
            _logger.LogError(ex, "Failed to create car: {Make} {Model}", command.Make, command.Model);
            throw;
        }
    }

    // ❌ DON'T: String interpolation (not structured)
    // _logger.LogInformation($"Car {id} was retrieved");

    // ❌ DON'T: String concatenation
    // _logger.LogInformation("Car " + id + " was retrieved");
}
```

### 3. Log Levels

```csharp
// Trace: Very detailed diagnostic information (disabled in production)
_logger.LogTrace("Entering method GetCarAsync with ID: {CarId}", id);

// Debug: Detailed information for debugging (disabled in production)
_logger.LogDebug("Querying database for car: {CarId}", id);

// Information: General informational messages
_logger.LogInformation("Car created: {CarId}", id);

// Warning: Unexpected events that don't stop execution
_logger.LogWarning("Car not found in cache, fetching from database: {CarId}", id);

// Error: Errors and exceptions
_logger.LogError(ex, "Failed to create car: {CarId}", id);

// Critical: Critical failures requiring immediate attention
_logger.LogCritical(ex, "Database connection lost");
```

### 4. Application Insights Integration

```csharp
// Install package
// dotnet add package Microsoft.ApplicationInsights.AspNetCore

// Program.cs
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

// Custom telemetry
public class CarService
{
    private readonly TelemetryClient _telemetryClient;

    public async Task<Car> GetCarAsync(Guid id)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var car = await _repository.GetByIdAsync(id);

            _telemetryClient.TrackEvent("CarRetrieved", new Dictionary<string, string>
            {
                { "CarId", id.ToString() },
                { "Make", car.Make }
            });

            _telemetryClient.TrackMetric("CarRetrievalTime", sw.ElapsedMilliseconds);

            return car;
        }
        catch (Exception ex)
        {
            _telemetryClient.TrackException(ex);
            throw;
        }
    }
}
```

## React Error Handling

### 1. Error Boundaries

```tsx
// src/components/ErrorBoundary.tsx
import { Component, ErrorInfo, ReactNode } from 'react';

interface Props {
    children: ReactNode;
    fallback?: ReactNode;
    onError?: (error: Error, errorInfo: ErrorInfo) => void;
}

interface State {
    hasError: boolean;
    error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
    constructor(props: Props) {
        super(props);
        this.state = { hasError: false, error: null };
    }

    static getDerivedStateFromError(error: Error): State {
        return { hasError: true, error };
    }

    componentDidCatch(error: Error, errorInfo: ErrorInfo) {
        // Log to error reporting service
        console.error('Error caught by boundary:', error, errorInfo);

        // Optional callback
        this.props.onError?.(error, errorInfo);

        // Send to logging service
        logErrorToService(error, errorInfo);
    }

    render() {
        if (this.state.hasError) {
            return this.props.fallback || (
                <div className="error-boundary">
                    <h2>Something went wrong</h2>
                    <details>
                        <summary>Error details</summary>
                        <pre>{this.state.error?.message}</pre>
                    </details>
                    <button onClick={() => this.setState({ hasError: false, error: null })}>
                        Try again
                    </button>
                </div>
            );
        }

        return this.props.children;
    }
}

// Usage
function App() {
    return (
        <ErrorBoundary fallback={<ErrorFallback />}>
            <Router>
                <Routes>
                    <Route path="/cars" element={
                        <ErrorBoundary fallback={<div>Error loading cars</div>}>
                            <CarList />
                        </ErrorBoundary>
                    } />
                </Routes>
            </Router>
        </ErrorBoundary>
    );
}
```

### 2. API Error Handling

```tsx
// src/utils/errors.ts
export class ApiError extends Error {
    constructor(
        public statusCode: number,
        message: string,
        public errors?: Record<string, string[]>
    ) {
        super(message);
        this.name = 'ApiError';
    }
}

// src/services/api.service.ts
class ApiService {
    private async handleResponse<T>(response: AxiosResponse<T>): Promise<T> {
        if (response.status >= 200 && response.status < 300) {
            return response.data;
        }

        throw new ApiError(
            response.status,
            response.data?.message || 'An error occurred',
            response.data?.errors
        );
    }

    private async handleError(error: any): Promise<never> {
        if (axios.isAxiosError(error)) {
            if (error.response) {
                // Server responded with error status
                throw new ApiError(
                    error.response.status,
                    error.response.data?.message || error.message,
                    error.response.data?.errors
                );
            } else if (error.request) {
                // Request made but no response
                throw new ApiError(0, 'No response from server');
            }
        }

        // Something else happened
        throw new ApiError(500, error.message || 'Unknown error occurred');
    }

    async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
        try {
            const response = await this.api.get<T>(url, config);
            return this.handleResponse(response);
        } catch (error) {
            return this.handleError(error);
        }
    }
}
```

### 3. Displaying Errors

```tsx
// src/components/ErrorDisplay.tsx
interface ErrorDisplayProps {
    error: Error | ApiError;
    onRetry?: () => void;
}

export function ErrorDisplay({ error, onRetry }: ErrorDisplayProps) {
    const isApiError = error instanceof ApiError;

    return (
        <div className="error-display" role="alert">
            <h3>Error</h3>
            <p>{error.message}</p>

            {isApiError && error.errors && (
                <ul>
                    {Object.entries(error.errors).map(([field, messages]) => (
                        <li key={field}>
                            <strong>{field}:</strong> {messages.join(', ')}
                        </li>
                    ))}
                </ul>
            )}

            {onRetry && (
                <button onClick={onRetry}>Try Again</button>
            )}
        </div>
    );
}

// Usage with React Query
function CarList() {
    const { data, error, isError, refetch } = useCars();

    if (isError) {
        return <ErrorDisplay error={error} onRetry={refetch} />;
    }

    return (/* ... */);
}
```

### 4. Toast Notifications for Errors

```tsx
// Using react-hot-toast
import toast from 'react-hot-toast';

export function useErrorToast() {
    const showError = useCallback((error: Error | ApiError) => {
        if (error instanceof ApiError) {
            if (error.errors) {
                // Show validation errors
                Object.entries(error.errors).forEach(([field, messages]) => {
                    messages.forEach(message => {
                        toast.error(`${field}: ${message}`);
                    });
                });
            } else {
                toast.error(error.message);
            }
        } else {
            toast.error(error.message || 'An error occurred');
        }
    }, []);

    return { showError };
}

// Usage
function CarForm() {
    const createCar = useCreateCar();
    const { showError } = useErrorToast();

    const handleSubmit = async (data: CarFormData) => {
        try {
            await createCar.mutateAsync(data);
            toast.success('Car created successfully');
        } catch (error) {
            showError(error as Error);
        }
    };

    return (/* form */);
}
```

## React Logging

### 1. Custom Logger

```tsx
// src/utils/logger.ts
type LogLevel = 'debug' | 'info' | 'warn' | 'error';

interface LogEntry {
    level: LogLevel;
    message: string;
    timestamp: Date;
    context?: Record<string, any>;
}

class Logger {
    private isDevelopment = import.meta.env.DEV;

    private log(level: LogLevel, message: string, context?: Record<string, any>) {
        const entry: LogEntry = {
            level,
            message,
            timestamp: new Date(),
            context
        };

        // Console logging
        if (this.isDevelopment || level === 'error') {
            const logMethod = console[level] || console.log;
            logMethod(`[${level.toUpperCase()}]`, message, context || '');
        }

        // Send to logging service in production
        if (!this.isDevelopment && level === 'error') {
            this.sendToLoggingService(entry);
        }
    }

    private async sendToLoggingService(entry: LogEntry) {
        try {
            await fetch('/api/logs', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(entry)
            });
        } catch (error) {
            // Fail silently or use fallback
            console.error('Failed to send log to service', error);
        }
    }

    debug(message: string, context?: Record<string, any>) {
        this.log('debug', message, context);
    }

    info(message: string, context?: Record<string, any>) {
        this.log('info', message, context);
    }

    warn(message: string, context?: Record<string, any>) {
        this.log('warn', message, context);
    }

    error(message: string, context?: Record<string, any>) {
        this.log('error', message, context);
    }
}

export const logger = new Logger();

// Usage
function CarList() {
    const { data, error } = useCars();

    useEffect(() => {
        if (error) {
            logger.error('Failed to load cars', {
                error: error.message,
                userId: currentUser?.id
            });
        } else if (data) {
            logger.info('Cars loaded successfully', {
                count: data.items.length
            });
        }
    }, [data, error]);

    return (/* ... */);
}
```

### 2. Integration with Error Tracking Services

```tsx
// Sentry integration
import * as Sentry from '@sentry/react';

Sentry.init({
    dsn: import.meta.env.VITE_SENTRY_DSN,
    environment: import.meta.env.MODE,
    integrations: [
        new Sentry.BrowserTracing(),
        new Sentry.Replay()
    ],
    tracesSampleRate: 1.0,
    replaysSessionSampleRate: 0.1,
    replaysOnErrorSampleRate: 1.0
});

// Wrap app
function App() {
    return (
        <Sentry.ErrorBoundary fallback={<ErrorFallback />}>
            <Router />
        </Sentry.ErrorBoundary>
    );
}

// Custom logging
logger.error('Custom error', { userId: '123' });
Sentry.captureMessage('Custom error', {
    level: 'error',
    extra: { userId: '123' }
});
```

## Best Practices Checklist

### ASP.NET Core
- [ ] Implement global exception handling middleware
- [ ] Use custom exceptions for business logic errors
- [ ] Return appropriate HTTP status codes
- [ ] Use structured logging with Serilog
- [ ] Log at appropriate levels
- [ ] Never log sensitive data (passwords, tokens)
- [ ] Use Problem Details (RFC 7807)
- [ ] Implement validation with FluentValidation
- [ ] Add correlation IDs for request tracing
- [ ] Monitor with Application Insights or similar

### React
- [ ] Use Error Boundaries for component errors
- [ ] Handle API errors gracefully
- [ ] Show user-friendly error messages
- [ ] Provide retry mechanisms
- [ ] Log errors to monitoring service
- [ ] Display validation errors clearly
- [ ] Use toast notifications for feedback
- [ ] Never expose stack traces to users
- [ ] Test error scenarios
- [ ] Provide offline fallbacks

### General
- [ ] Different handling for dev vs production
- [ ] Centralize error handling logic
- [ ] Document error codes and messages
- [ ] Monitor error rates
- [ ] Set up alerts for critical errors
- [ ] Regular error log review
- [ ] User-friendly error messages
- [ ] Include troubleshooting steps
- [ ] Track error resolution

## Summary

Effective error handling and logging require planning and consistent implementation. By using global exception handlers, structured logging, error boundaries, and proper user feedback mechanisms, you create applications that gracefully handle failures and provide valuable diagnostic information for troubleshooting.
