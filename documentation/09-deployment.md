# Deployment & DevOps

## Overview

Modern deployment practices emphasize automation, reliability, and rapid iteration. This guide covers containerization, CI/CD pipelines, cloud deployment, and DevOps best practices for React + ASP.NET Core applications.

## Docker Containerization

### 1. Multi-Stage Dockerfile for ASP.NET Core + React

```dockerfile
# Stage 1: Build React app
FROM node:20-alpine AS client-build
WORKDIR /app/client

# Copy package files
COPY ClientApp/package*.json ./
RUN npm ci

# Copy source and build
COPY ClientApp/ ./
RUN npm run build

# Stage 2: Build .NET app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS server-build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Run tests before final stage
FROM server-build AS test
WORKDIR /app
COPY --from=server-build /app .
RUN dotnet test --no-restore --verbosity normal

# Stage 3: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published app
COPY --from=server-build /app/out ./

# Copy React build
COPY --from=client-build /app/client/build ./wwwroot

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Run app
ENTRYPOINT ["dotnet", "CarBuilder.API.dll"]
```

### 2. Docker Compose for Local Development

```yaml
# docker-compose.yml
version: '3.8'

services:
  # SQL Server
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    healthcheck:
      test: /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT 1"
      interval: 10s
      timeout: 3s
      retries: 10

  # Redis Cache
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    command: redis-server --appendonly yes

  # API
  api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=CarBuilderDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
      - Redis__Configuration=redis:6379
    depends_on:
      sqlserver:
        condition: service_healthy
      redis:
        condition: service_started
    volumes:
      - ./logs:/app/logs

  # Seq (Log aggregation)
  seq:
    image: datalust/seq:latest
    environment:
      - ACCEPT_EULA=Y
    ports:
      - "5341:80"
    volumes:
      - seq-data:/data

volumes:
  sqlserver-data:
  redis-data:
  seq-data:
```

### 3. .dockerignore

```
# Ignore build artifacts
**/bin/
**/obj/
**/out/
**/publish/

# Ignore Node modules and build
**/node_modules/
**/build/
**/dist/

# Ignore Git
.git/
.gitignore

# Ignore IDE
.vs/
.vscode/
*.user
*.suo

# Ignore Docker
**/.dockerignore
**/docker-compose*
**/Dockerfile*

# Ignore tests
**/tests/
**/*.Tests/

# Ignore documentation
**/documentation/
**/*.md
```

## CI/CD with GitHub Actions

### 1. Build and Test Workflow

```yaml
# .github/workflows/build-test.yml
name: Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

env:
  DOTNET_VERSION: '8.0.x'
  NODE_VERSION: '20.x'

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: Test@Password123
          MSSQL_PID: Developer
        ports:
          - 1433:1433
        options: >-
          --health-cmd "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Test@Password123 -Q 'SELECT 1'"
          --health-interval 10s
          --health-timeout 3s
          --health-retries 10

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      # Setup .NET
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      # Setup Node.js
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'
          cache-dependency-path: ClientApp/package-lock.json

      # Restore dependencies
      - name: Restore .NET dependencies
        run: dotnet restore

      - name: Restore npm dependencies
        working-directory: ./ClientApp
        run: npm ci

      # Build
      - name: Build .NET
        run: dotnet build --no-restore --configuration Release

      - name: Build React app
        working-directory: ./ClientApp
        run: npm run build

      # Run tests
      - name: Run .NET tests
        run: dotnet test --no-build --configuration Release --verbosity normal --collect:"XPlat Code Coverage"

      - name: Run React tests
        working-directory: ./ClientApp
        run: npm test -- --coverage --watchAll=false

      # Upload coverage
      - name: Upload .NET coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          files: ./tests/**/coverage.cobertura.xml
          flags: dotnet

      - name: Upload React coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          files: ./ClientApp/coverage/coverage-final.json
          flags: react

      # Code quality
      - name: Run SonarCloud scan
        uses: SonarSource/sonarcloud-github-action@master
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
```

### 2. Docker Build and Push Workflow

```yaml
# .github/workflows/docker-publish.yml
name: Docker Build and Publish

on:
  push:
    branches: [ main ]
    tags: [ 'v*.*.*' ]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v3

      - name: Log in to Container Registry
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Extract metadata
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
          tags: |
            type=ref,event=branch
            type=ref,event=pr
            type=semver,pattern={{version}}
            type=semver,pattern={{major}}.{{minor}}
            type=sha

      - name: Build and push Docker image
        uses: docker/build-push-action@v5
        with:
          context: .
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          cache-from: type=gha
          cache-to: type=gha,mode=max
          build-args: |
            BUILD_VERSION=${{ github.sha }}

      - name: Run Trivy vulnerability scanner
        uses: aquasecurity/trivy-action@master
        with:
          image-ref: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.sha }}
          format: 'sarif'
          output: 'trivy-results.sarif'

      - name: Upload Trivy results to GitHub Security
        uses: github/codeql-action/upload-sarif@v3
        with:
          sarif_file: 'trivy-results.sarif'
```

### 3. Deploy to Azure

```yaml
# .github/workflows/deploy-azure.yml
name: Deploy to Azure

on:
  workflow_run:
    workflows: ["Docker Build and Publish"]
    types:
      - completed
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    if: ${{ github.event.workflow_run.conclusion == 'success' }}

    steps:
      - name: Azure Login
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Deploy to Azure Container Instances
        uses: azure/aci-deploy@v1
        with:
          resource-group: ${{ secrets.AZURE_RESOURCE_GROUP }}
          dns-name-label: carbuilder-${{ github.sha }}
          image: ghcr.io/${{ github.repository }}:${{ github.sha }}
          registry-login-server: ghcr.io
          registry-username: ${{ github.actor }}
          registry-password: ${{ secrets.GITHUB_TOKEN }}
          name: carbuilder-api
          location: 'eastus'
          ports: 8080
          environment-variables: |
            ASPNETCORE_ENVIRONMENT=Production
          secure-environment-variables: |
            ConnectionStrings__DefaultConnection=${{ secrets.DB_CONNECTION_STRING }}
            JwtSettings__SecretKey=${{ secrets.JWT_SECRET }}

      - name: Run database migrations
        run: |
          az container exec \
            --resource-group ${{ secrets.AZURE_RESOURCE_GROUP }} \
            --name carbuilder-api \
            --exec-command "dotnet ef database update"
```

## Azure DevOps Pipeline

```yaml
# azure-pipelines.yml
trigger:
  branches:
    include:
      - main
      - develop

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'
  dotnetVersion: '8.0.x'
  nodeVersion: '20.x'

stages:
  - stage: Build
    jobs:
      - job: BuildBackend
        displayName: 'Build .NET Backend'
        steps:
          - task: UseDotNet@2
            displayName: 'Install .NET SDK'
            inputs:
              version: $(dotnetVersion)

          - task: DotNetCoreCLI@2
            displayName: 'Restore NuGet packages'
            inputs:
              command: 'restore'

          - task: Cache@2
            displayName: 'Cache NuGet packages'
            inputs:
              key: 'nuget | "$(Agent.OS)" | **/packages.lock.json'
              path: $(NUGET_PACKAGES)
              cacheHitVar: CACHE_RESTORED

          - task: DotNetCoreCLI@2
            displayName: 'Build solution'
            inputs:
              command: 'build'
              arguments: '--configuration $(buildConfiguration)'

          - task: DotNetCoreCLI@2
            displayName: 'Run tests'
            inputs:
              command: 'test'
              arguments: '--configuration $(buildConfiguration) --collect:"XPlat Code Coverage"'

          - task: PublishCodeCoverageResults@1
            displayName: 'Publish code coverage'
            inputs:
              codeCoverageTool: 'Cobertura'
              summaryFileLocation: '$(Agent.TempDirectory)/**/*coverage.cobertura.xml'

      - job: BuildFrontend
        displayName: 'Build React Frontend'
        steps:
          - task: NodeTool@0
            displayName: 'Install Node.js'
            inputs:
              versionSpec: $(nodeVersion)

          - task: Cache@2
            displayName: 'Cache npm packages'
            inputs:
              key: 'npm | "$(Agent.OS)" | ClientApp/package-lock.json'
              path: ClientApp/node_modules

          - script: |
              cd ClientApp
              npm ci
              npm run build
              npm test -- --coverage --watchAll=false
            displayName: 'Build and test React app'

  - stage: Deploy
    dependsOn: Build
    condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
    jobs:
      - deployment: DeployToProduction
        displayName: 'Deploy to Production'
        environment: 'Production'
        strategy:
          runOnce:
            deploy:
              steps:
                - task: Docker@2
                  displayName: 'Build and push Docker image'
                  inputs:
                    containerRegistry: 'DockerHub'
                    repository: 'carbuilder/api'
                    command: 'buildAndPush'
                    Dockerfile: '**/Dockerfile'
                    tags: |
                      $(Build.BuildId)
                      latest

                - task: AzureWebAppContainer@1
                  displayName: 'Deploy to Azure Web App'
                  inputs:
                    azureSubscription: 'Azure-Subscription'
                    appName: 'carbuilder-api'
                    containers: 'carbuilder/api:$(Build.BuildId)'
```

## Deployment Strategies

### 1. Blue-Green Deployment

```yaml
# Deploy to staging (blue)
- name: Deploy to Staging
  run: |
    kubectl apply -f k8s/staging/
    kubectl wait --for=condition=available --timeout=300s deployment/api-staging

# Run smoke tests
- name: Run smoke tests
  run: |
    npm run test:smoke -- --url https://staging.carbuilder.com

# Switch traffic to staging (promote to production)
- name: Promote to Production
  run: |
    kubectl patch service api-service -p '{"spec":{"selector":{"version":"staging"}}}'
```

### 2. Canary Deployment

```yaml
# Deploy canary (10% traffic)
apiVersion: v1
kind: Service
metadata:
  name: api-service
spec:
  selector:
    app: api
  ports:
    - port: 80
      targetPort: 8080

---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: api-stable
spec:
  replicas: 9
  selector:
    matchLabels:
      app: api
      version: stable
  template:
    metadata:
      labels:
        app: api
        version: stable
    spec:
      containers:
        - name: api
          image: carbuilder/api:v1.0.0

---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: api-canary
spec:
  replicas: 1
  selector:
    matchLabels:
      app: api
      version: canary
  template:
    metadata:
      labels:
        app: api
        version: canary
    spec:
      containers:
        - name: api
          image: carbuilder/api:v1.1.0
```

### 3. Rolling Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: api
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  selector:
    matchLabels:
      app: api
  template:
    metadata:
      labels:
        app: api
    spec:
      containers:
        - name: api
          image: carbuilder/api:latest
          ports:
            - containerPort: 8080
          livenessProbe:
            httpGet:
              path: /health
              port: 8080
            initialDelaySeconds: 30
            periodSeconds: 10
          readinessProbe:
            httpGet:
              path: /health/ready
              port: 8080
            initialDelaySeconds: 5
            periodSeconds: 5
```

## Environment Configuration

### 1. Environment Variables

```csharp
// appsettings.json (Development)
{
    "Logging": {
        "LogLevel": {
            "Default": "Debug"
        }
    },
    "AllowedHosts": "*"
}

// appsettings.Production.json
{
    "Logging": {
        "LogLevel": {
            "Default": "Warning"
        }
    },
    "AllowedHosts": "carbuilder.com,*.carbuilder.com"
}

// Use environment variables for secrets
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
```

### 2. Azure App Configuration

```csharp
// Program.cs
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(Environment.GetEnvironmentVariable("AZURE_APP_CONFIG_CONNECTION_STRING"))
           .ConfigureKeyVault(kv =>
           {
               kv.SetCredential(new DefaultAzureCredential());
           })
           .Select("CarBuilder:*")
           .Select("CarBuilder:*", builder.Environment.EnvironmentName);
});
```

## Monitoring and Observability

### 1. Health Checks

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddRedis(builder.Configuration["Redis:Configuration"])
    .AddUrlGroup(new Uri("https://api.external.com/health"), "External API");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
```

### 2. Application Insights

```csharp
builder.Services.AddApplicationInsightsTelemetry();

// Custom metrics
_telemetryClient.TrackMetric("CarsCreated", 1);
_telemetryClient.TrackEvent("OrderPlaced", new Dictionary<string, string>
{
    { "OrderId", order.Id.ToString() },
    { "TotalAmount", order.Total.ToString() }
});
```

### 3. Prometheus Metrics

```csharp
dotnet add package prometheus-net.AspNetCore

// Program.cs
app.UseMetricServer(); // /metrics endpoint
app.UseHttpMetrics();  // HTTP request metrics
```

## Best Practices Checklist

### Docker
- [ ] Use multi-stage builds
- [ ] Run as non-root user
- [ ] Implement health checks
- [ ] Use .dockerignore
- [ ] Scan for vulnerabilities
- [ ] Keep images small
- [ ] Use specific version tags
- [ ] Layer caching optimization

### CI/CD
- [ ] Automated testing on every PR
- [ ] Code quality checks (SonarQube, CodeQL)
- [ ] Security scanning (Trivy, Snyk)
- [ ] Build artifacts caching
- [ ] Parallel job execution
- [ ] Automated deployments
- [ ] Rollback capability
- [ ] Environment-specific configurations

### Deployment
- [ ] Blue-green or canary deployments
- [ ] Database migration automation
- [ ] Configuration management
- [ ] Secrets management (Key Vault)
- [ ] Health checks
- [ ] Graceful shutdown
- [ ] Resource limits
- [ ] Auto-scaling policies

### Monitoring
- [ ] Application logs centralized
- [ ] Metrics collection
- [ ] Distributed tracing
- [ ] Alerting configured
- [ ] Performance monitoring
- [ ] Error tracking
- [ ] User analytics
- [ ] Infrastructure monitoring

## Summary

Modern deployment practices combine containerization, automated pipelines, and robust monitoring to deliver reliable, scalable applications. By implementing these DevOps practices, you ensure consistent deployments, rapid iteration, and production confidence.
