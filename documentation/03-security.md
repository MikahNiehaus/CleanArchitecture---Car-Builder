# Security Best Practices

## Overview

Security is paramount in modern web applications. This guide covers comprehensive security practices for React frontends and ASP.NET Core backends, addressing common vulnerabilities and implementing defense-in-depth strategies.

## React Security

### 1. Cross-Site Scripting (XSS) Prevention

XSS is one of the most common web vulnerabilities. React provides built-in protection, but you must use it correctly.

#### Built-in Protection
React's JSX automatically escapes values, making it safe by default:

```tsx
// ✅ SAFE: React escapes automatically
function UserGreeting({ userName }: { userName: string }) {
    return <h1>Hello, {userName}</h1>;
}

// Even malicious input is escaped
// userName = "<script>alert('XSS')</script>"
// Renders as text, not executed
```

#### Dangerous Patterns to Avoid

**dangerouslySetInnerHTML:**
```tsx
// ❌ DANGEROUS: Never use without sanitization
function BlogPost({ html }: { html: string }) {
    return <div dangerouslySetInnerHTML={{ __html: html }} />;
}

// ✅ SAFE: Sanitize first with DOMPurify
import DOMPurify from 'dompurify';

function BlogPost({ html }: { html: string }) {
    const sanitizedHtml = DOMPurify.sanitize(html, {
        ALLOWED_TAGS: ['b', 'i', 'em', 'strong', 'a', 'p'],
        ALLOWED_ATTR: ['href']
    });

    return <div dangerouslySetInnerHTML={{ __html: sanitizedHtml }} />;
}
```

**URL Validation:**
```tsx
// ❌ DANGEROUS: javascript: protocol can execute code
function Link({ url }: { url: string }) {
    return <a href={url}>Click</a>;
}

// ✅ SAFE: Validate URL protocol
function SafeLink({ url }: { url: string }) {
    const isSafeUrl = (url: string): boolean => {
        try {
            const parsed = new URL(url);
            return ['http:', 'https:', 'mailto:'].includes(parsed.protocol);
        } catch {
            return false;
        }
    };

    if (!isSafeUrl(url)) {
        return <span>Invalid URL</span>;
    }

    return <a href={url} rel="noopener noreferrer">Click</a>;
}
```

**Direct DOM Manipulation:**
```tsx
// ❌ DANGEROUS: Bypasses React's protection
function BadComponent() {
    const ref = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (ref.current) {
            ref.current.innerHTML = userInput; // XSS risk!
        }
    }, []);

    return <div ref={ref} />;
}

// ✅ SAFE: Use React's rendering
function GoodComponent() {
    return <div>{userInput}</div>;
}
```

### 2. Cross-Site Request Forgery (CSRF) Prevention

#### Anti-CSRF Tokens
```tsx
// Token stored in meta tag from server
const csrfToken = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content');

// Include in all state-changing requests
async function createCar(car: Car) {
    const response = await fetch('/api/cars', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': csrfToken || ''
        },
        body: JSON.stringify(car)
    });

    return response.json();
}
```

#### SameSite Cookies
Server should set cookies with SameSite attribute (handled on backend):
```
Set-Cookie: sessionId=abc123; SameSite=Strict; Secure; HttpOnly
```

### 3. Secure Authentication Token Storage

```tsx
// ❌ DANGEROUS: Never store in localStorage (vulnerable to XSS)
localStorage.setItem('authToken', token);

// ✅ BETTER: Use httpOnly cookies (set by backend)
// Frontend cannot access the token, reducing XSS impact

// ✅ ACCEPTABLE: For non-sensitive tokens in memory
const AuthContext = createContext<AuthContextType | null>(null);

function AuthProvider({ children }: { children: ReactNode }) {
    const [token, setToken] = useState<string | null>(null);

    // Token only exists in memory, lost on refresh
    // User must re-authenticate after refresh or store in sessionStorage

    return (
        <AuthContext.Provider value={{ token, setToken }}>
            {children}
        </AuthContext.Provider>
    );
}

// ✅ COMPROMISE: sessionStorage for better UX (closes on tab close)
sessionStorage.setItem('authToken', token);
```

### 4. Input Validation and Sanitization

```tsx
// Client-side validation (never trust, always validate server-side too)
interface CarFormData {
    make: string;
    model: string;
    year: number;
    price: number;
}

function CarForm() {
    const [errors, setErrors] = useState<Partial<CarFormData>>({});

    const validate = (data: CarFormData): boolean => {
        const newErrors: Partial<CarFormData> = {};

        if (!data.make || data.make.trim().length === 0) {
            newErrors.make = 'Make is required';
        }

        if (data.make && data.make.length > 50) {
            newErrors.make = 'Make must not exceed 50 characters';
        }

        if (data.year < 1900 || data.year > new Date().getFullYear() + 1) {
            newErrors.year = 'Invalid year';
        }

        if (data.price <= 0) {
            newErrors.price = 'Price must be greater than zero';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (data: CarFormData) => {
        if (!validate(data)) return;

        // Send to server (which will validate again)
        await createCar(data);
    };

    return (/* form JSX */);
}
```

### 5. Dependency Security

```bash
# Regularly audit dependencies
npm audit

# Fix vulnerabilities
npm audit fix

# Use Snyk or GitHub Dependabot
npm install -g snyk
snyk test
```

**package.json best practices:**
```json
{
    "dependencies": {
        "react": "^19.0.0",
        "react-dom": "^19.0.0"
    },
    "devDependencies": {
        "@types/react": "^19.0.0"
    },
    "scripts": {
        "audit": "npm audit",
        "update-check": "npm outdated"
    }
}
```

### 6. Content Security Policy (CSP)

```tsx
// Set CSP headers (done on server/build config)
// Example for index.html or server response
/*
Content-Security-Policy:
    default-src 'self';
    script-src 'self';
    style-src 'self' 'unsafe-inline';
    img-src 'self' data: https:;
    connect-src 'self' https://api.example.com;
    font-src 'self';
    object-src 'none';
    base-uri 'self';
    form-action 'self';
    frame-ancestors 'none';
    upgrade-insecure-requests;
*/
```

### 7. Secure Headers Implementation

React apps should configure these headers (typically in hosting config):

```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Referrer-Policy: strict-origin-when-cross-origin
Permissions-Policy: geolocation=(), microphone=(), camera=()
```

## ASP.NET Core Security

### 1. Authentication with JWT

#### Secure JWT Configuration

```csharp
// appsettings.json (use User Secrets or Azure Key Vault for secrets)
{
    "JwtSettings": {
        "SecretKey": "USE_USER_SECRETS_NOT_APPSETTINGS",
        "Issuer": "https://yourapp.com",
        "Audience": "https://yourapp.com",
        "ExpirationMinutes": 60,
        "RefreshTokenExpirationDays": 7
    }
}

// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"])
            ),
            ClockSkew = TimeSpan.Zero // Reduce default 5-minute clock skew
        };

        // Require HTTPS in production
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        // Handle JWT in cookies instead of Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["authToken"];
                return Task.CompletedTask;
            }
        };
    });
```

#### Token Generation

```csharp
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
```

#### Production Recommendations
```csharp
// For production, use RSA instead of symmetric keys
var rsa = RSA.Create();
rsa.ImportRSAPrivateKey(privateKeyBytes, out _);

var key = new RsaSecurityKey(rsa);
var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
```

### 2. Password Security

```csharp
// ✅ Use ASP.NET Core Identity's built-in password hasher
public class UserService
{
    private readonly IPasswordHasher<User> _passwordHasher;

    public async Task<User> RegisterAsync(string email, string password)
    {
        var user = new User { Email = email };

        // Automatically uses PBKDF2 with salt
        var hashedPassword = _passwordHasher.HashPassword(user, password);
        user.PasswordHash = hashedPassword;

        await _repository.AddAsync(user);
        return user;
    }

    public async Task<bool> VerifyPasswordAsync(string email, string password)
    {
        var user = await _repository.GetByEmailAsync(email);
        if (user == null) return false;

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password
        );

        return result == PasswordVerificationResult.Success;
    }
}

// Password policy
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
    options.Password.RequiredUniqueChars = 4;
});
```

### 3. Input Validation

```csharp
// Use FluentValidation for comprehensive validation
public class CreateCarCommandValidator : AbstractValidator<CreateCarCommand>
{
    public CreateCarCommandValidator()
    {
        RuleFor(x => x.Make)
            .NotEmpty()
            .MaximumLength(50)
            .Matches("^[a-zA-Z0-9 -]+$") // Only alphanumeric, spaces, hyphens
            .WithMessage("Make contains invalid characters");

        RuleFor(x => x.Year)
            .GreaterThan(1900)
            .LessThanOrEqualTo(DateTime.UtcNow.Year + 1);

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .LessThan(10_000_000);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}

// Validation filter
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            context.Result = new BadRequestObjectResult(new { errors });
            return;
        }

        await next();
    }
}
```

### 4. SQL Injection Prevention

```csharp
// ✅ Entity Framework Core prevents SQL injection by default
public async Task<Car> GetCarByIdAsync(Guid id)
{
    // Parameterized automatically
    return await _context.Cars
        .Where(c => c.Id == id)
        .FirstOrDefaultAsync();
}

// ✅ For raw SQL, use parameters
public async Task<List<Car>> SearchCarsAsync(string make)
{
    return await _context.Cars
        .FromSqlInterpolated($"SELECT * FROM Cars WHERE Make = {make}")
        .ToListAsync();
}

// ❌ NEVER concatenate user input
// var cars = _context.Cars.FromSqlRaw($"SELECT * FROM Cars WHERE Make = '{make}'");
```

### 5. CORS Configuration

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowedMethods("GET", "POST", "PUT", "DELETE")
              .AllowedHeaders("Content-Type", "Authorization")
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });

    // Development only
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use appropriate policy based on environment
app.UseCors(app.Environment.IsDevelopment() ? "DevelopmentPolicy" : "ProductionPolicy");
```

### 6. HTTPS and Security Headers

```csharp
// Force HTTPS
app.UseHttpsRedirection();
app.UseHsts(); // HTTP Strict Transport Security

// Security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add(
        "Content-Security-Policy",
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'"
    );

    await next();
});
```

### 7. Rate Limiting (ASP.NET Core 7+)

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later.",
            token
        );
    };
});

app.UseRateLimiter();
```

### 8. Secrets Management

```bash
# Never commit secrets to source control

# Development: Use User Secrets
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "your-secret-key"

# Production: Use Azure Key Vault, AWS Secrets Manager, etc.
```

```csharp
// Azure Key Vault integration
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential()
);
```

## Security Checklist

### React Frontend
- [ ] No sensitive data in client-side code
- [ ] Sanitize HTML with DOMPurify if using dangerouslySetInnerHTML
- [ ] Validate URLs before rendering links
- [ ] Use HTTPS only
- [ ] Implement CSP headers
- [ ] Store tokens securely (httpOnly cookies preferred)
- [ ] Validate all user input client-side (and server-side)
- [ ] Keep dependencies updated
- [ ] Run `npm audit` regularly
- [ ] Implement proper error boundaries (don't expose stack traces)

### ASP.NET Core Backend
- [ ] Use HTTPS everywhere
- [ ] Implement authentication and authorization
- [ ] Use JWT with short expiration times
- [ ] Never store plain-text passwords
- [ ] Validate and sanitize all inputs
- [ ] Use parameterized queries (EF Core does this)
- [ ] Configure CORS properly
- [ ] Implement rate limiting
- [ ] Use security headers
- [ ] Store secrets in Key Vault/User Secrets
- [ ] Keep framework and packages updated
- [ ] Log security events
- [ ] Implement proper error handling (don't expose internals)

### General
- [ ] Follow principle of least privilege
- [ ] Implement defense in depth
- [ ] Regular security audits
- [ ] Penetration testing for production apps
- [ ] Monitor for suspicious activity
- [ ] Have an incident response plan
- [ ] Keep security patches up to date
- [ ] Educate team on security best practices

## Common Vulnerabilities (OWASP Top 10)

1. **Broken Access Control**: Implement proper authorization
2. **Cryptographic Failures**: Use HTTPS, encrypt sensitive data
3. **Injection**: Validate inputs, use parameterized queries
4. **Insecure Design**: Follow security best practices from the start
5. **Security Misconfiguration**: Harden all configurations
6. **Vulnerable Components**: Keep dependencies updated
7. **Authentication Failures**: Implement MFA, secure sessions
8. **Software and Data Integrity Failures**: Verify package integrity
9. **Security Logging Failures**: Log security events
10. **Server-Side Request Forgery**: Validate URLs, whitelist destinations

## Summary

Security is not a one-time effort but an ongoing process. By implementing these practices, regularly auditing your application, and staying informed about new vulnerabilities, you can significantly reduce your attack surface and protect your users' data.
