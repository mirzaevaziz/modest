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
- Unique constraints and validation
- Soft delete support
- Modular, testable codebase
- Comprehensive integration tests using Testcontainers

---

## Architecture & Patterns

- **Clean Architecture**: Separation of concerns between API, Core (domain/services), and Infrastructure.
- **CQRS (Command Query Responsibility Segregation)**: Commands and queries are handled separately for clarity and scalability.
- **Repository Pattern**: Abstracts data access for MongoDB.
- **Dependency Injection**: All services and repositories are injected for testability.
- **Endpoint Routing**: Minimal API endpoints using [FastEndpoints](https://fast-endpoints.com/).
- **Validation**: FluentValidation for request validation.
- **Soft Delete**: Entities support soft deletion with `IsDeleted` flag.
- **Logging**: Structured logging with Serilog and Microsoft.Extensions.Logging.
- **Test Isolation**: Each test runs against a fresh MongoDB replica set container.

---

## Technologies & Packages

- **.NET 9**
- **MongoDB** (with [MongoDB.Driver](https://www.nuget.org/packages/MongoDB.Driver/))
- **MongoDB.EntityFrameworkCore** (EF Core provider for MongoDB)
- **FastEndpoints** (Minimal API framework)
- **FastEndpoints.Swagger** (OpenAPI/Swagger support)
- **FluentValidation** (Validation)
- **Serilog** (Logging)
- **Testcontainers** (Integration test containers)
- **Testcontainers.MongoDb** (MongoDB container for tests)
- **Alba** (API integration testing)
- **xUnit** (Testing framework)
- **FluentAssertions** (Assertions)
- **Microsoft.AspNetCore.OpenApi** (OpenAPI/Swagger support)
- **Microsoft.AspNetCore.Mvc.Testing** (Test server for integration tests)
- **coverlet.collector** (Code coverage)

---

## Project Structure

```
modest/
├── src/
│   ├── Modest.API/             # API endpoints and startup
│   └── Modest.Core/            # Domain models, services, interfaces
├── tests/
│   └── Modest.IntegrationTests/ # Integration tests (Testcontainers, Alba)
├── Directory.Packages.props     # Centralized NuGet package versions
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
- **Repositories**: Data access is abstracted via interfaces.
- **DTOs**: Data Transfer Objects are used for API contracts.
- **Logging**: Uses Serilog and Microsoft.Extensions.Logging.
- **Test Isolation**: Each test resets the MongoDB database for isolation.

---

## Contributing

Contributions are welcome! Please open issues or submit pull requests.

---

## License

This project is licensed under the MIT License.

---

**Enjoy building with Modest!**
