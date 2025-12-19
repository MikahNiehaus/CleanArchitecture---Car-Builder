# Performance Optimization

## Overview

Performance optimization is about delivering fast, responsive applications that provide excellent user experience. This guide covers performance best practices for both React frontends and ASP.NET Core backends, emphasizing the golden rule: **measure before you optimize**.

## Core Web Vitals

Google's Core Web Vitals are three critical metrics for measuring web performance:

### 1. Largest Contentful Paint (LCP)
**Target: < 2.5 seconds**

Measures loading performance. LCP should occur within 2.5 seconds of when the page first starts loading.

```tsx
// Optimize images for faster LCP
function HeroImage({ src, alt }: { src: string; alt: string }) {
    return (
        <img
            src={src}
            alt={alt}
            loading="eager"  // Don't lazy load above-the-fold images
            fetchpriority="high"  // Prioritize critical images
            width={800}
            height={600}
        />
    );
}
```

### 2. Interaction to Next Paint (INP)
**Target: < 200 milliseconds**

Measures interactivity. Replaced First Input Delay (FID) in 2024.

```tsx
// Use useTransition for non-urgent updates (React 18+)
function SearchResults() {
    const [query, setQuery] = useState('');
    const [results, setResults] = useState([]);
    const [isPending, startTransition] = useTransition();

    const handleSearch = (value: string) => {
        setQuery(value); // Urgent: update input immediately

        startTransition(() => {
            // Non-urgent: update results without blocking input
            setResults(searchCars(value));
        });
    };

    return (
        <>
            <input value={query} onChange={e => handleSearch(e.target.value)} />
            {isPending ? <Spinner /> : <ResultsList results={results} />}
        </>
    );
}
```

### 3. Cumulative Layout Shift (CLS)
**Target: < 0.1**

Measures visual stability. Pages should not shift unexpectedly.

```tsx
// Reserve space for dynamic content
function ProductCard({ imageUrl }: { imageUrl: string }) {
    return (
        <div>
            {/* Specify width and height to prevent layout shift */}
            <img
                src={imageUrl}
                alt="Product"
                width={300}
                height={200}
                style={{ aspectRatio: '3/2' }}
            />
        </div>
    );
}

// Use skeleton loaders
function LoadingSkeleton() {
    return (
        <div className="skeleton" style={{ width: 300, height: 200 }}>
            {/* Matches final content dimensions */}
        </div>
    );
}
```

## React Performance Optimization

### 1. Measure Before Optimizing

```tsx
import { Profiler } from 'react';

function App() {
    const onRenderCallback = (
        id: string,
        phase: "mount" | "update",
        actualDuration: number,
        baseDuration: number,
        startTime: number,
        commitTime: number
    ) => {
        console.log(`${id} (${phase}) took ${actualDuration}ms`);
    };

    return (
        <Profiler id="CarList" onRender={onRenderCallback}>
            <CarList />
        </Profiler>
    );
}
```

**React DevTools Profiler:**
- Record interactions
- Identify slow components
- Analyze render times
- Find unnecessary re-renders

### 2. Memoization (Use Sparingly)

**Most developers overuse useMemo and useCallback.** Only use after measuring a performance problem.

```tsx
// ❌ DON'T: Premature optimization
function CarList({ cars }: { cars: Car[] }) {
    const sortedCars = useMemo(
        () => cars.sort((a, b) => a.price - b.price),
        [cars]
    );

    return <>{sortedCars.map(car => <CarCard key={car.id} car={car} />)}</>;
}

// ✅ DO: Only if sorting is proven to be slow
// For small arrays (<1000 items), useMemo overhead > sorting cost

// ✅ DO: Memoize expensive calculations
function DataAnalysis({ data }: { data: number[] }) {
    const analysis = useMemo(() => {
        // Truly expensive: complex statistical calculations
        return performComplexAnalysis(data); // 100ms+
    }, [data]);

    return <AnalysisChart data={analysis} />;
}

// ✅ DO: Memoize components with expensive rendering
const ExpensiveComponent = React.memo(({ data }: { data: ComplexData }) => {
    // Component with heavy rendering logic
    return <ComplexVisualization data={data} />;
});

// ✅ DO: useCallback when passing to memoized children
function Parent() {
    const handleClick = useCallback((id: string) => {
        console.log(id);
    }, []);

    return <MemoizedChild onClick={handleClick} />;
}
```

### 3. Code Splitting and Lazy Loading

```tsx
// Lazy load routes
import { lazy, Suspense } from 'react';

const CarList = lazy(() => import('./features/cars/CarList'));
const CarDetails = lazy(() => import('./features/cars/CarDetails'));
const Orders = lazy(() => import('./features/orders/Orders'));

function App() {
    return (
        <BrowserRouter>
            <Suspense fallback={<LoadingSpinner />}>
                <Routes>
                    <Route path="/cars" element={<CarList />} />
                    <Route path="/cars/:id" element={<CarDetails />} />
                    <Route path="/orders" element={<Orders />} />
                </Routes>
            </Suspense>
        </BrowserRouter>
    );
}

// Lazy load heavy components
const HeavyChart = lazy(() => import('./components/HeavyChart'));

function Dashboard() {
    const [showChart, setShowChart] = useState(false);

    return (
        <div>
            <button onClick={() => setShowChart(true)}>Show Chart</button>
            {showChart && (
                <Suspense fallback={<div>Loading chart...</div>}>
                    <HeavyChart />
                </Suspense>
            )}
        </div>
    );
}
```

**Build Configuration (Vite):**
```ts
// vite.config.ts
export default defineConfig({
    build: {
        rollupOptions: {
            output: {
                manualChunks: {
                    'react-vendor': ['react', 'react-dom', 'react-router-dom'],
                    'ui-library': ['@mui/material'],
                    'charts': ['recharts', 'd3']
                }
            }
        },
        chunkSizeWarningLimit: 1000
    }
});
```

### 4. Virtualization for Long Lists

```tsx
import { FixedSizeList } from 'react-window';

// ❌ DON'T: Render thousands of items at once
function SlowCarList({ cars }: { cars: Car[] }) {
    return (
        <div>
            {cars.map(car => <CarCard key={car.id} car={car} />)}
        </div>
    );
}

// ✅ DO: Use virtualization for long lists
function FastCarList({ cars }: { cars: Car[] }) {
    const Row = ({ index, style }: { index: number; style: React.CSSProperties }) => (
        <div style={style}>
            <CarCard car={cars[index]} />
        </div>
    );

    return (
        <FixedSizeList
            height={600}
            itemCount={cars.length}
            itemSize={120}
            width="100%"
        >
            {Row}
        </FixedSizeList>
    );
}
```

### 5. Image Optimization

```tsx
// Modern image formats with fallback
function OptimizedImage({ src, alt }: { src: string; alt: string }) {
    return (
        <picture>
            <source srcSet={`${src}.avif`} type="image/avif" />
            <source srcSet={`${src}.webp`} type="image/webp" />
            <img
                src={`${src}.jpg`}
                alt={alt}
                loading="lazy"
                decoding="async"
            />
        </picture>
    );
}

// Responsive images
function ResponsiveImage({ src, alt }: { src: string; alt: string }) {
    return (
        <img
            src={src}
            srcSet={`
                ${src}?w=400 400w,
                ${src}?w=800 800w,
                ${src}?w=1200 1200w
            `}
            sizes="(max-width: 600px) 400px, (max-width: 1200px) 800px, 1200px"
            alt={alt}
            loading="lazy"
        />
    );
}
```

### 6. Debouncing and Throttling

```tsx
// Debounce search input
function useDebounce<T>(value: T, delay: number): T {
    const [debouncedValue, setDebouncedValue] = useState(value);

    useEffect(() => {
        const handler = setTimeout(() => {
            setDebouncedValue(value);
        }, delay);

        return () => clearTimeout(handler);
    }, [value, delay]);

    return debouncedValue;
}

function SearchCars() {
    const [searchTerm, setSearchTerm] = useState('');
    const debouncedSearchTerm = useDebounce(searchTerm, 300);

    useEffect(() => {
        if (debouncedSearchTerm) {
            // API call only after user stops typing for 300ms
            searchCars(debouncedSearchTerm);
        }
    }, [debouncedSearchTerm]);

    return (
        <input
            value={searchTerm}
            onChange={e => setSearchTerm(e.target.value)}
            placeholder="Search cars..."
        />
    );
}
```

### 7. React 19 Compiler (Automatic Memoization)

React 19 includes an optional compiler that automatically memoizes components:

```tsx
// With React 19 Compiler enabled, no manual memoization needed
function CarList({ cars }: { cars: Car[] }) {
    // Compiler automatically optimizes this
    const sortedCars = cars.sort((a, b) => a.price - b.price);

    return <>{sortedCars.map(car => <CarCard key={car.id} car={car} />)}</>;
}

// Enable in vite.config.ts or next.config.js
// experimental: { reactCompiler: true }
```

## ASP.NET Core Performance Optimization

### 1. Asynchronous Operations

```csharp
// ✅ Always use async/await for I/O operations
public async Task<ActionResult<List<CarDto>>> GetCarsAsync(CancellationToken cancellationToken)
{
    var cars = await _carService.GetAllCarsAsync(cancellationToken);
    return Ok(cars);
}

// ❌ Never block async code
public ActionResult<List<CarDto>> GetCars()
{
    var cars = _carService.GetAllCarsAsync().Result; // Deadlock risk!
    return Ok(cars);
}
```

### 2. Database Query Optimization

```csharp
// ✅ Use AsNoTracking for read-only queries
public async Task<List<Car>> GetCarsAsync()
{
    return await _context.Cars
        .AsNoTracking() // 30-40% faster for read-only
        .ToListAsync();
}

// ✅ Project to DTOs to fetch only needed columns
public async Task<List<CarListDto>> GetCarsForListAsync()
{
    return await _context.Cars
        .AsNoTracking()
        .Select(c => new CarListDto
        {
            Id = c.Id,
            Make = c.Make,
            Model = c.Model,
            Price = c.Price
            // Don't fetch unnecessary columns
        })
        .ToListAsync();
}

// ✅ Use pagination
public async Task<PagedResult<CarDto>> GetCarsPagedAsync(int page, int pageSize)
{
    var query = _context.Cars.AsNoTracking();

    var total = await query.CountAsync();

    var cars = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(c => new CarDto(c.Id, c.Make, c.Model, c.Year, c.Price))
        .ToListAsync();

    return new PagedResult<CarDto>(cars, total, page, pageSize);
}

// ✅ Avoid N+1 queries with Include
public async Task<List<Order>> GetOrdersWithDetailsAsync()
{
    return await _context.Orders
        .Include(o => o.Customer)
        .Include(o => o.Car)
        .AsNoTracking()
        .ToListAsync();
}

// ✅ Use AsSplitQuery for multiple includes (EF Core 5+)
public async Task<List<Order>> GetOrdersWithDetailsAsync()
{
    return await _context.Orders
        .Include(o => o.Customer)
        .Include(o => o.Items) // Separate query to avoid cartesian explosion
        .AsSplitQuery()
        .ToListAsync();
}
```

### 3. Caching Strategies

**Memory Cache:**
```csharp
public class CarService
{
    private readonly IMemoryCache _cache;
    private const string CacheKey = "all-cars";

    public async Task<List<CarDto>> GetCarsAsync()
    {
        if (_cache.TryGetValue(CacheKey, out List<CarDto> cachedCars))
        {
            return cachedCars;
        }

        var cars = await _repository.GetAllAsync();
        var carDtos = _mapper.Map<List<CarDto>>(cars);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };

        _cache.Set(CacheKey, carDtos, cacheOptions);

        return carDtos;
    }
}
```

**Distributed Cache (Redis):**
```csharp
public class CarService
{
    private readonly IDistributedCache _cache;

    public async Task<List<CarDto>> GetCarsAsync()
    {
        var cacheKey = "all-cars";
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (cachedData != null)
        {
            return JsonSerializer.Deserialize<List<CarDto>>(cachedData);
        }

        var cars = await _repository.GetAllAsync();
        var carDtos = _mapper.Map<List<CarDto>>(cars);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(carDtos),
            options
        );

        return carDtos;
    }
}
```

**Response Caching:**
```csharp
// Program.cs
builder.Services.AddResponseCaching();
app.UseResponseCaching();

// Controller
[HttpGet]
[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
public async Task<ActionResult<List<CarDto>>> GetCars()
{
    var cars = await _carService.GetCarsAsync();
    return Ok(cars);
}
```

### 4. Compression

```csharp
// Program.cs
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

app.UseResponseCompression();
```

### 5. Connection Pooling

```csharp
// appsettings.json
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=localhost;Database=CarDb;Trusted_Connection=true;MultipleActiveResultSets=true;Pooling=true;Min Pool Size=5;Max Pool Size=100;"
    }
}

// Entity Framework connection resiliency
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null
            );
            sqlOptions.CommandTimeout(30);
        }
    )
);
```

### 6. Minimize Allocations

```csharp
// ✅ Use ValueTask for frequently called async methods
public ValueTask<Car> GetCarFromCacheAsync(Guid id)
{
    if (_cache.TryGetValue(id, out Car car))
    {
        return new ValueTask<Car>(car); // No allocation
    }

    return new ValueTask<Car>(GetCarFromDatabaseAsync(id));
}

// ✅ Use ArrayPool for temporary buffers
private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;

public async Task ProcessDataAsync(Stream stream)
{
    var buffer = _arrayPool.Rent(4096);
    try
    {
        await stream.ReadAsync(buffer, 0, buffer.Length);
        // Process buffer
    }
    finally
    {
        _arrayPool.Return(buffer);
    }
}

// ✅ Use struct for small, immutable types
public readonly struct Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
}
```

### 7. HTTP/2 and HTTP/3

```csharp
// Program.cs - Enable HTTP/2
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
    });

    serverOptions.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        listenOptions.UseHttps();
    });
});

// HTTP/3 (experimental)
serverOptions.ListenAnyIP(5001, listenOptions =>
{
    listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
    listenOptions.UseHttps();
});
```

## Full-Stack Optimization

### 1. API Response Optimization

```csharp
// Use minimal APIs for simple endpoints (less overhead)
app.MapGet("/api/cars", async (ICarRepository repo) =>
{
    var cars = await repo.GetAllAsync();
    return Results.Ok(cars);
});

// Use System.Text.Json with source generators (faster serialization)
[JsonSerializable(typeof(CarDto))]
[JsonSerializable(typeof(List<CarDto>))]
partial class AppJsonContext : JsonSerializerContext { }

// Program.cs
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
});
```

### 2. CDN for Static Assets

```tsx
// Use CDN for libraries when possible
// <script src="https://cdn.jsdelivr.net/npm/react@19/umd/react.production.min.js"></script>

// Or configure your build to upload assets to CDN
// vite.config.ts
export default defineConfig({
    build: {
        assetsDir: 'assets',
        rollupOptions: {
            output: {
                assetFileNames: 'assets/[name].[hash][extname]'
            }
        }
    },
    base: 'https://cdn.yourdomain.com/'
});
```

### 3. Service Worker for Caching

```tsx
// Register service worker
if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('/sw.js');
}

// sw.js - Cache static assets
self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open('v1').then((cache) => {
            return cache.addAll([
                '/',
                '/styles.css',
                '/script.js',
                '/logo.png'
            ]);
        })
    );
});
```

## Performance Monitoring

### React
```tsx
// Web Vitals reporting
import { onCLS, onFID, onLCP } from 'web-vitals';

onCLS(console.log);
onFID(console.log);
onLCP(console.log);

// Send to analytics
function sendToAnalytics(metric) {
    const body = JSON.stringify(metric);
    navigator.sendBeacon('/analytics', body);
}
```

### ASP.NET Core
```csharp
// Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Custom performance logging
public class PerformanceLoggingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        await _next(context);

        sw.Stop();

        if (sw.ElapsedMilliseconds > 1000)
        {
            _logger.LogWarning(
                "Slow request: {Method} {Path} took {Duration}ms",
                context.Request.Method,
                context.Request.Path,
                sw.ElapsedMilliseconds
            );
        }
    }
}
```

## Performance Checklist

### React
- [ ] Measure performance with React Profiler
- [ ] Monitor Core Web Vitals
- [ ] Implement code splitting for routes
- [ ] Lazy load heavy components
- [ ] Optimize images (WebP, lazy loading)
- [ ] Virtualize long lists
- [ ] Debounce expensive operations
- [ ] Minimize bundle size
- [ ] Use production builds
- [ ] Only memoize when measured bottlenecks exist

### ASP.NET Core
- [ ] Use async/await for I/O
- [ ] Enable response compression
- [ ] Implement caching (memory/distributed)
- [ ] Use AsNoTracking for read queries
- [ ] Optimize database queries
- [ ] Enable connection pooling
- [ ] Use pagination for large datasets
- [ ] Minimize allocations
- [ ] Monitor with Application Insights
- [ ] Use HTTP/2

### General
- [ ] Use CDN for static assets
- [ ] Enable HTTP caching headers
- [ ] Minimize API payloads
- [ ] Use database indexes
- [ ] Monitor and set performance budgets
- [ ] Regular performance audits
- [ ] Load testing before production

## Summary

Performance optimization is an iterative process. Always measure first, optimize bottlenecks, then measure again. Focus on user-perceived performance (Core Web Vitals) and implement caching, lazy loading, and efficient data fetching strategies for the best results.
