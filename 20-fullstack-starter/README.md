# Full-Stack Starter Application

This is a full-stack application built with .NET 9 and Angular, orchestrated using .NET Aspire.

## Projects Overview

### Application Projects

- **AppHost** - .NET Aspire orchestration host that manages and coordinates all services in the solution, including the WebAPI and Frontend.

- **WebApi** - ASP.NET Core Web API that exposes RESTful endpoints. Depends on `AppServices` for business logic and `ServiceDefaults` for Aspire integration.

- **Frontend** - Angular application that provides the user interface. Consumes the WebApi through generated API clients.

- **AppServices** - Core business logic and data access layer using Entity Framework Core with SQLite. Contains database context, models, and migrations.

- **Importer** - Console application for importing data from CSV files. Depends on `AppServices` for database operations.

- **ServiceDefaults** - Shared Aspire service defaults project containing common telemetry, resilience, and service discovery configurations.

- **TestInfrastructure** - Shared test infrastructure providing common fixtures and utilities for database testing. Includes `DatabaseFixture` which configures an in-memory SQLite database for tests, eliminating the need for a physical database file during testing.

### Test Projects

- **WebApiTests** - Integration tests for the WebApi project.

- **AppServicesTests** - Unit and integration tests for the AppServices project. Uses the in-memory SQLite database from `TestInfrastructure` via xUnit's `IClassFixture<DatabaseFixture>` pattern.

- **ImporterTests** - Unit tests for the Importer project, including command-line parsing and data import functionality.

- **FrontendTests** - End-to-end tests for the Frontend application using Playwright. Tests the application from the user's perspective in a real browser environment.

## Building the Solution

### Build .NET Projects

```bash
dotnet build
```

### Build Frontend

```bash
cd Frontend
npm install
npm run build
```

> **Note for Windows Users**: The `package.json` files in `Frontend` contains `start` scripts configured for Linux/macOS (using `$PORT`). Windows users must rename the script:
> - Rename `start` to `start:linux` (or delete it)
> - Rename `start:windows` to `start`

## API Integration

The Frontend and WebApi are integrated through OpenAPI specifications:

1. **WebApi** - Automatically generates an OpenAPI specification file (`WebApi.json`) to the `Frontend` directory during build. This is configured in the WebApi project file with `<OpenApiDocumentsDirectory>../Frontend</OpenApiDocumentsDirectory>`.

2. **Frontend** - Uses `ng-openapi-gen` to generate TypeScript API client code from the `WebApi.json` specification. This happens automatically when running `npm start`, which executes `npm run generate-web-api` to create the API clients in `src/app/api`.

## Running the Application

### Create the Database

Before running the application for the first time, create the SQLite database:

```bash
dotnet ef database update --project AppServices
```

### Start the Application

Start the entire application stack using .NET Aspire:

```bash
dotnet run --project AppHost
```

This will launch the Aspire dashboard where you can monitor and access all running services, including:
- WebApi
- Frontend
- SQLite database
- Telemetry and logging

The Aspire dashboard will provide URLs to access each service.

## Running Tests

### .NET Tests

Run all .NET tests:

```bash
dotnet test
```

### Frontend E2E Tests

The Frontend E2E tests use Playwright and are orchestrated by Aspire alongside the other services.

> **Note for Windows Users**: The `package.json` files in `FrontendTests` contains `start` scripts configured for Linux/macOS (using `$PORT`). Windows users must rename the script:
> - Rename `start` to `start:linux` (or delete it)
> - Rename `start:windows` to `start`

Simply start the Aspire AppHost:

```bash
dotnet run --project AppHost
```

Aspire will automatically:
- Launch all services (WebApi, Frontend, FrontendTests)
- Set the necessary environment variables so Playwright can find the Angular app
- Start the Playwright UI for running the E2E tests

You can monitor and access all services, including the Playwright test UI, through the Aspire dashboard.
