# Car Builder - Clean Architecture App

A full-stack car management application built with .NET 9 and React, following Clean Architecture principles and industry best practices.

## Architecture

This project follows Clean Architecture with clear separation of concerns:

- **Domain Layer**: Core business entities and logic
- **Application Layer**: Business use cases (CQRS with MediatR)
- **Infrastructure Layer**: Data persistence, Identity, external services
- **API Layer**: RESTful API endpoints, JWT authentication
- **Frontend**: React with TypeScript and Tailwind CSS

## Technology Stack

### Backend
- .NET 9 (C#)
- Entity Framework Core 9
- ASP.NET Core Identity
- MediatR (CQRS pattern)
- FluentValidation
- AutoMapper
- Serilog
- JWT Bearer Authentication
- Swagger/OpenAPI

### Frontend
- React 19
- TypeScript
- Vite
- Tailwind CSS
- React Query
- Axios

### Database
- SQL Server 2022 (Docker)

## Getting Started

### Prerequisites
- .NET 9 SDK
- Docker Desktop
- Node.js 18+ and npm

### 1. Start SQL Server

```bash
docker-compose up -d
```

This will start SQL Server on `localhost:1433` with:
- Username: `sa`
- Password: `YourStrong@Passw0rd`

### 2. Run the Backend API

The migrations will be applied automatically when the API starts in development mode.

```bash
cd src/CarBuilder.API
dotnet run
```

The API will be available at:
- HTTPS: https://localhost:7001
- HTTP: http://localhost:5001
- Swagger: https://localhost:7001/swagger

### 3. Run the Frontend (Coming soon)

```bash
cd frontend
npm install
npm run dev
```

The React app will be available at http://localhost:5173

## API Endpoints

### Authentication
- `POST /api/v1/auth/register` - Register a new user
- `POST /api/v1/auth/login` - Login and receive JWT token

### Cars (Requires Authentication)
- `GET /api/v1/cars` - Get all cars
- `GET /api/v1/cars/{id}` - Get car by ID
- `POST /api/v1/cars` - Create a new car
- `PUT /api/v1/cars/{id}` - Update an existing car
- `DELETE /api/v1/cars/{id}` - Delete a car

## Project Structure

```
CleanArchitecture - Car Builder/
├── src/
│   ├── CarBuilder.Domain/          # Entities, value objects, domain logic
│   ├── CarBuilder.Application/     # Use cases, DTOs, interfaces
│   ├── CarBuilder.Infrastructure/  # EF Core, repositories, identity
│   └── CarBuilder.API/             # Controllers, middleware, configuration
├── frontend/                       # React application (coming soon)
├── documentation/                  # Best practices documentation
├── docker-compose.yml              # SQL Server configuration
└── README.md
```

## Development Features

- Automatic database migrations in development
- Structured logging with Serilog
- Global exception handling
- FluentValidation pipeline
- CORS enabled for local development
- Swagger UI for API testing

## Best Practices Implemented

- ✅ Clean Architecture with dependency inversion
- ✅ SOLID principles
- ✅ Domain-Driven Design (DDD)
- ✅ CQRS pattern with MediatR
- ✅ Repository pattern with Unit of Work
- ✅ Request validation pipeline
- ✅ Structured logging
- ✅ JWT authentication
- ✅ Entity configuration with Fluent API
- ✅ Dependency Injection
- ✅ DTOs for data transfer
- ✅ Global exception handling

## Security

- JWT token-based authentication
- Password requirements enforced
- CORS configured for specific origins
- Sensitive data not exposed in production error messages
- SQL injection prevention via EF Core parameterization

## Configuration

Key settings in `appsettings.json`:
- Connection strings
- JWT settings (secret, issuer, audience)
- Serilog configuration

**Note**: In production, use environment variables or secure vaults for sensitive configuration.

## Testing

### End-to-End Testing with Playwright MCP

The project includes comprehensive E2E testing using Playwright with Model Context Protocol (MCP) for AI-powered browser automation.

**Setup Playwright MCP:**

1. Add to Claude Code configuration (`%APPDATA%\Claude\claude_desktop_config.json` on Windows):

```json
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": ["-y", "@playwright/mcp@latest"]
    }
  }
}
```

2. Restart Claude Code

**Running E2E Tests:**

Ensure all services are running:
```bash
# Terminal 1: SQL Server
docker-compose up -d

# Terminal 2: Backend API
cd src/CarBuilder.API && dotnet run

# Terminal 3: Frontend
cd frontend && npm run dev
```

Then ask Claude Code to run tests using Playwright MCP (tests run in headed mode by default for interactive debugging).

**Test Coverage:**
- ✅ User authentication (register, login, logout)
- ✅ Car CRUD operations (create, read, update, delete)
- ✅ Protected route navigation
- ✅ Form validation and error handling
- ✅ JWT token management
- ✅ API integration

See `documentation/10-playwright-e2e-testing.md` for comprehensive testing guide.

### Unit and Integration Tests

For unit and integration testing best practices, see:
- `documentation/05-testing.md` - Testing strategies
- Frontend: Vitest + React Testing Library
- Backend: xUnit + Moq

## Next Steps

- Add unit tests for business logic
- Add integration tests for API endpoints
- Configure CI/CD pipeline with automated E2E tests
- Add more car properties (images, features, specifications)
- Implement car image upload
- Add car search and filtering
- Implement pagination for cars list
"# CleanArchitecture---Car-Builder" 
