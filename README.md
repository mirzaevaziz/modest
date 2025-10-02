# Modest

**Modest** is a modern, modular reference application for product management, built with .NET 9, MongoDB, and clean architecture principles. It demonstrates best practices for scalable, testable, and maintainable backend development.

---

## Table of Contents

- [Features](#features)
- [Architecture & Patterns](#architecture--patterns)
- [Technologies & Packages](#technologies--packages)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Testing](#testing)
- [Development Patterns](#development-patterns)
- [Contributing](#contributing)
- [License](#license)

---

## Features

- Product CRUD (Create, Read, Update, Delete) endpoints
- Lookup endpoints for manufacturers and countries
- Pagination, filtering, and search
- Unique constraints and validation (compound unique index on Name, Manufacturer, Country)
- Soft delete support (deleted products can be restored on create/update)
- Modular, testable codebase
- Comprehensive integration tests using Testcontainers

---

## Architecture & Patterns

- **Clean Architecture**: Separation of concerns between API, Core (domain/services), and Infrastructure.
- **CQRS (Command Query Responsibility Segregation)**: Commands and queries are handled separately for clarity and scalability.
- **Repository Pattern**: Abstracts data access for MongoDB (using MongoDB.Driver directly, no EF Core).
- **Dependency Injection**: All services and repositories are injected for testability.
- **Endpoint Routing**: Minimal API endpoints using [FastEndpoints](https://fast-endpoints.com/).
- **Validation**: FluentValidation for request validation.
- **Soft Delete**: Entities support soft deletion with `IsDeleted` flag.
- **Logging**: Structured logging with Serilog and Microsoft.Extensions.Logging.
- **Test Isolation**: Each test runs against a fresh MongoDB replica set container.

---

## Technologies & Packages

- **.NET 9** – Modern, high-performance runtime for backend services
- **MongoDB.Driver** – Official MongoDB driver for .NET (direct, no EF Core)
- **FastEndpoints** – Minimal, high-performance endpoint routing for APIs
- **FastEndpoints.Swagger** – OpenAPI/Swagger UI integration
- **FluentValidation** – Strongly-typed validation for request DTOs
- **Serilog** – Structured logging for diagnostics and production
- **Testcontainers** – Containerized integration testing (MongoDB replica set)
- **Alba** – Fluent API integration testing for ASP.NET Core
- **xUnit** – Unit and integration test framework
- **FluentAssertions** – Readable, expressive assertions for tests
- **Microsoft.AspNetCore.OpenApi** – OpenAPI/Swagger support for ASP.NET Core
- **Microsoft.AspNetCore.Mvc.Testing** – Test server for integration tests
- **coverlet.collector** – Code coverage for .NET projects
- **Docker** – Local development and test environment for MongoDB

---

## Project Structure

```
modest/
├── src/
│   ├── Modest.API/              # API endpoints, startup, handlers, endpoint definitions
│   │   ├── Endpoints/           # API endpoint classes (by domain)
│   │   ├── Handlers/            # Exception handlers, logging, etc.
│   │   └── ...                  # Program.cs, appsettings, etc.
│   ├── Modest.Core/             # Domain models, services, interfaces, helpers
│   │   ├── Common/              # Shared models and exceptions
│   │   ├── Features/            # Domain features (e.g., Product)
│   │   └── Helpers/             # Pagination, sorting, validation helpers
│   └── Modest.Data/             # MongoDB repositories, data access, DI
│       ├── Features/            # Data features (e.g., ProductRepository)
│       ├── DependencyInjection.cs
│       └── ...
├── tests/
│   └── Modest.IntegrationTests/ # Integration tests (Testcontainers, Alba)
│       ├── Endpoints/           # Endpoint test classes
│       ├── Transactions/        # Transaction/edge case tests
│       ├── IntegrationTestBase.cs
│       ├── WebFixture.cs
│       └── ...
├── Directory.Packages.props     # Centralized NuGet package versions
├── Directory.Build.props        # Build configuration
├── compose.yml                  # Docker Compose for local dev/test
└── README.md
```

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (for running MongoDB in tests)

### Running the API

1. **Configure MongoDB**  
   Set your MongoDB connection string in `appsettings.Development.json`:

   ```json
   {
     "MongoDb": {
       "ConnectionString": "mongodb://localhost:27017",
       "DatabaseName": "ModestDb"
     }
   }
   ```

2. **Run the API**

   ```bash
   dotnet run --project src/Modest.API
   ```

3. **API Documentation**

   Visit `/api/swagger` for the Swagger UI (OpenAPI docs).

---

## Testing

Integration tests use Testcontainers to spin up a MongoDB replica set automatically.

```bash
dotnet test
```

- Tests are written with xUnit, Alba, and FluentAssertions.
- Each test run uses a fresh MongoDB container for isolation.

---

## Development Patterns

- **Endpoints**: Each endpoint is a class inheriting from `Endpoint<TRequest, TResponse>`.
- **Services**: Business logic is in services, injected into endpoints.
- **Validation**: Validators are defined for each request DTO.
- **Repositories**: Data access is abstracted via interfaces. All test data setup uses the repository, not direct DbContext access.
- **DTOs**: Data Transfer Objects are used for API contracts.
- **Logging**: Uses Serilog and Microsoft.Extensions.Logging.
- **Test Isolation**: Each test resets the MongoDB database for isolation. Integration tests use Testcontainers with a MongoDB replica set, and the Alba host is restarted after each reset for true isolation.
- Robust update logic: On update, if a duplicate (by FullName) exists and is deleted, it is restored; if not deleted, a validation error is thrown.

### .NET Version

This project targets **.NET 9**. Ensure you have the latest .NET 9 SDK installed.

---

## Contributing

Contributions are welcome! Please open issues or submit pull requests.

---

## License

This project is licensed under the MIT License.

---

**Enjoy building with Modest!**
