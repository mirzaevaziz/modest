# Agent Instructions

This project uses **bd** (beads) for issue tracking. Run `bd onboard` to get started.

## Table of Contents

- [Workflow & Process](#workflow--process)
  - [Quick Reference](#quick-reference)
  - [Branching Strategy](#branching-strategy)
  - [Issue Creation Guidelines](#issue-creation-guidelines)
  - [Task Completion Protocol](#task-completion-protocol)
  - [Landing the Plane](#landing-the-plane-session-completion)
- [Development Standards](#development-standards)
  - [Code Review & Quality Standards](#code-review--quality-standards)
  - [Testing Requirements](#testing-requirements)
  - [Error Handling Guidelines](#error-handling-guidelines)
- [Project Reference](#project-reference)
  - [Project Architecture](#project-architecture)
  - [Common Commands](#common-commands-reference)
  - [Configuration & Secrets](#configuration--secrets)
  - [Debugging Tips](#debugging-tips)

---

## Workflow & Process

### Quick Reference

```bash
bd ready              # Find available work
bd show <id>          # View issue details
bd update <id> --status in_progress  # Claim work
bd close <id>         # Complete work
bd sync               # Sync with git
```

### Branching Strategy

We use trunk-based development with the following conventions:

**Branch Creation:**

Create feature branches from main using this naming convention:

```text
(chore/feat/fix/refactor/test)/(issue-number)-(short-description)
```

- **Examples:**
  - `feat/123-add-product-search`
  - `fix/456-null-reference-supplier`
  - `refactor/789-extract-base-endpoint`

**Important:** Before creating a branch, ALWAYS ask the user for:

1. Issue number (from bd)
2. Short description

**Development Process:**

- Keep branches short-lived (hours to days, not weeks)
- Commit early and often with descriptive messages
- Pull from main regularly to stay current
- Run tests locally before committing

**Merging Process:**

- All tests MUST pass before merging
- For personal project: Direct merge to main after tests pass
- Delete feature branch after merge

**Commit Guidelines:**

- Use descriptive, action-oriented commit messages
- Follow conventional commits format: `<type>: <description>`
- Examples:
  - `feat: Add pagination to product list endpoint`
  - `fix: Handle null supplier in product creation`
  - `test: Add integration tests for soft delete`

### Issue Creation Guidelines

**ALL issues MUST include:**

1. Acceptance Criteria (testable conditions)
2. Steps to Solve (implementation breakdown)
3. File List to Review (specific files)

**Keep tasks small** - completable in one session
✅ "Add migration for Users table" | ❌ "Implement authentication system"

**Organization:** Create epic first if no parent specified, then add child tasks

```bash
# Create epic and child tasks
bd new --type epic --title "Title" --description "..."
bd new --parent <epic-id> --title "Task" --description "..."

# After creating tasks, MUST prioritize and set dependencies:
bd update <id> --priority P0  # 0=highest
bd dep <blocker-id> --blocks <blocked-id>
```

**Issue Template:**

```markdown
## Acceptance Criteria

- [ ] Testable condition 1
- [ ] Testable condition 2

## Steps to Solve

1. Step 1
2. Step 2

## File List to Review

- path/to/file1.cs
- path/to/file2.cs
```

### Task Completion Protocol

**CRITICAL - After completing any task:**

1. Ask: "Should I close issue #123?" → Wait for YES
2. Run `bd close <id>` only after confirmation
3. Ask: "Should I commit?" → Wait for YES
4. Commit with detailed message

**NEVER auto-close or auto-commit**

### Landing the Plane (Session Completion)

**When ending a work session**, you MUST complete ALL steps below. Work is NOT complete until `git push` succeeds.

**MANDATORY WORKFLOW:**

1. **File issues for remaining work** - Create issues for anything that needs follow-up
2. **Run quality gates** (if code changed) - Tests, linters, builds
3. **Update issue status** - Close finished work, update in-progress items
4. **PUSH TO REMOTE** - This is MANDATORY:

   ```bash
   git pull --rebase
   bd sync
   git push
   git status  # MUST show "up to date with origin"
   ```

5. **Clean up** - Clear stashes, prune remote branches
6. **Verify** - All changes committed AND pushed
7. **Hand off** - Provide context for next session

**CRITICAL RULES:**

- Work is NOT complete until `git push` succeeds
- NEVER stop before pushing - that leaves work stranded locally
- NEVER say "ready to push when you are" - YOU must push
- If push fails, resolve and retry until it succeeds

---

## Development Standards

### Code Review & Quality Standards

**C# Coding Conventions:**

- **Target Framework:** .NET 9.0 (`net9.0`)
- **Nullable Reference Types:** Enabled (`<Nullable>enable</Nullable>`)
- **Implicit Usings:** Enabled - common namespaces auto-imported
- **LangVersion:** Latest C# features enabled
- **Namespace Convention:** `Modest.<ProjectName>` (e.g., `Modest.Core`, `Modest.API`, `Modest.Data`)
- **Assembly Naming:** `Modest.<ProjectName>`

**XML Documentation Requirements:**

- All public classes, interfaces, and methods SHOULD have XML documentation
- Use triple-slash comments (`///`) for XML docs
- Include `<summary>`, `<param>`, `<returns>` tags as appropriate
- Example pattern:

```csharp
/// <summary>
/// Repository for product data access operations.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Gets a product by its unique identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The product if found, null otherwise.</returns>
    Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}
```

    public bool Enabled { get; set; }

}

````

**Code Review Checklist:**

- [ ] All public APIs have XML documentation
- [ ] Nullable reference types properly handled (`string?` for nullable)
- [ ] Exception handling follows project patterns (see Error Handling section)
- [ ] Logging uses structured logging patterns (LoggerMessage.Define)
- [ ] Security considerations addressed (authentication, authorization)
**Code Review Checklist:**

- [ ] All public APIs have XML documentation
- [ ] Nullable reference types properly handled (`string?` for nullable)
- [ ] Exception handling follows project patterns (see Error Handling section)
- [ ] Logging uses structured logging patterns (Serilog)
- [ ] No hardcoded secrets or credentials
- [ ] Unit/integration tests added/updated for new functionality
- [ ] Code follows existing architectural patterns
- [ ] Dependencies properly injected (constructor injection preferred)
- [ ] Async/await used correctly (avoid `.Result`)

**Security Checklist:**

- [ ] Input validation implemented (FluentValidation)
- [ ] Sensitive data not logged in plain text
- [ ] MongoDB queries use proper filters (no injection vulnerabilities)
- [ ] XSS prevention (automatic in ASP.NET Core, verify when using raw HTML)

**Performance Considerations:**

- [ ] Async operations for I/O-bound work
- [ ] Efficient MongoDB queries (use indexes, projection)
- [ ] Large collections handled with pagination
- [ ] Resource disposal handled (`using` statements, IAsyncDisposable)

### Testing Requirements

**Test Project Structure:**

- Test projects located in `tests/` directory
- Current test projects:
  - `tests/Modest.IntegrationTests/` - Integration tests using Testcontainers
- Test project naming: `Modest.<ProjectName>.Tests`
- Tests use **xUnit**, **FluentAssertions**, **Testcontainers**, and **Alba**
- Test file naming: `*Tests.cs`

**Testing Framework:**

- **xUnit** - Standard testing framework
- **FluentAssertions** - Readable, expressive assertions
- **Testcontainers** - MongoDB container for integration tests
- **Alba** - Fluent API integration testing for ASP.NET Core
- **Microsoft.AspNetCore.Mvc.Testing** - Test server infrastructure

**Integration Testing Guidelines:**

- Follow **Arrange-Act-Assert (AAA)** pattern
- Use `[Fact]` for parameterless tests
- Use `[Theory]` with `[InlineData]` for parameterized tests
- Each test runs against a fresh MongoDB replica set container
- Example from codebase:

```csharp
public class ProductEndpointsTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new ProductCreateDto
        {
            Name = "Test Product",
            Code = "TST001",
            Manufacturer = "Test Manufacturer",
            Country = "US"
        };

        // Act
        var response = await Host.Scenario(s =>
        {
            s.Post.Json(request).ToUrl("/api/products");
            s.StatusCodeShouldBe(201);
        });

        // Assert
        var result = await response.ReadAsJsonAsync<ProductDto>();
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Product");
    }
}
````

**Test Naming Conventions:**

- Method name pattern: `<MethodUnderTest>_<Scenario>_<ExpectedResult>`
- Examples:
  - `GetProduct_WhenProductExists_ReturnsProduct`
  - `CreateProduct_WhenDuplicate_ReturnsConflict`
  - `DeleteProduct_WhenNotFound_ReturnsNotFound`

**What to Test:**

- **Integration Tests:**
  - API endpoint behavior
  - Database interactions (MongoDB)
  - Request/response validation
  - Error handling scenarios
  - Soft delete functionality
  - Edge cases and boundary conditions

**Code Coverage:**

- Aim for **80%+ code coverage** on new code
- Critical paths must be 100% covered
- Use `coverlet.collector` (already referenced in packages)
- Run coverage: `dotnet test --collect:"XPlat Code Coverage"`

**Testing Best Practices:**

- Tests should be independent and isolated
- Use meaningful test data (avoid "foo", "bar")
- One logical assertion per test (can have multiple Assert calls for same concept)
- Each test gets a fresh MongoDB container
- Avoid testing framework code
- Keep tests reasonably fast (integration tests may take longer than unit tests)

---

## Project Reference

### Project Architecture

**Technology Stack:**

- **.NET 9.0** - Modern, high-performance runtime
- **ASP.NET Core** - Web API framework
- **MongoDB.Driver** - Official MongoDB driver (direct, no ORM)
- **FastEndpoints** - Minimal, high-performance endpoint routing
- **Key Libraries:**
  - **FluentValidation** - Input validation
  - **Serilog** - Structured logging
  - **Testcontainers** - Containerized testing
  - **Alba** - Integration testing for ASP.NET Core
  - **FluentAssertions** - Test assertions

**Solution Structure:**

```text
modest.sln                      # Main solution file
├── src/                        # Source projects
│   ├── Modest.API/             # API endpoints, startup, handlers
│   │   ├── Endpoints/          # API endpoint classes (by domain)
│   │   │   ├── Common/         # Base endpoint patterns
│   │   │   └── References/     # Reference data endpoints (Product, Supplier)
│   │   ├── Handlers/           # Exception handlers
│   │   └── Extensions/         # Service registration extensions
│   ├── Modest.Core/            # Domain models, services, interfaces
│   │   ├── Common/             # Shared models, exceptions, helpers
│   │   ├── Features/           # Domain features (Product, Supplier, etc.)
│   │   └── Helpers/            # Pagination, sorting, validation helpers
│   └── Modest.Data/            # MongoDB repositories, data access
│       └── Features/           # Data features (repositories)
├── tests/
│   └── Modest.IntegrationTests/ # Integration tests (Testcontainers, Alba)
│       ├── Endpoints/          # Endpoint test classes
│       ├── Repositories/       # Repository test classes
│       └── Transactions/       # Transaction/edge case tests
├── Directory.Packages.props    # Centralized NuGet package versions
└── Directory.Build.props       # Build configuration
```

**Key Architectural Patterns:**

- **Clean Architecture** - Separation of API, Core (domain), and Data layers
- **Repository Pattern** - Data access abstraction (MongoDB)
- **Dependency Injection** - Constructor injection throughout
- **Soft Delete** - Entities support soft deletion with `IsDeleted` flag
- **Exception Handlers** - Global exception handling via `IExceptionHandler`
- **Validation** - FluentValidation for request validation
- **Logging** - Structured logging with Serilog

**Common Project Locations:**

- **DTOs & Models:** `src/Modest.Core/Features/<Domain>/`
- **Repositories:** `src/Modest.Data/Features/<Domain>/`
- **API Endpoints:** `src/Modest.API/Endpoints/<Domain>/`
- **Exception Handlers:** `src/Modest.API/Handlers/`
- **Integration Tests:** `tests/Modest.IntegrationTests/Endpoints/<Domain>/`

**Central Package Management:**

- Version management: `Directory.Packages.props`
- Build properties: `Directory.Build.props`

### Error Handling Guidelines

**Custom Exception Types:**

The project uses custom exceptions in `Modest.Core.Common.Exceptions`:

- **`ItemNotFoundException`** - Resource not found (maps to 404)

**Exception Usage Patterns:**

```csharp
// Not found
var product = await _repository.GetByIdAsync(id, cancellationToken)
    ?? throw new ItemNotFoundException($"Product with ID {id} not found.");

// Validation handled by FluentValidation
// FluentValidation throws ValidationException automatically
```

**Global Exception Handling:**

- Implements `IExceptionHandler` from ASP.NET Core (in `Modest.API/Handlers/ApiExceptionHandler.cs`)
- Custom handlers for:
  - `ItemNotFoundException` → 404
  - `ValidationException` (FluentValidation) → 400
  - Unhandled exceptions → 500
- Returns RFC 7807 `ProblemDetails` responses
- Includes trace ID for debugging

**Logging Patterns:**

Use **structured logging** via Serilog:

```csharp
// Standard logging
_logger.LogInformation("Processing product {ProductId}", productId);
_logger.LogError(exception, "Failed to create product");

// Structured logging with context
_logger.LogInformation("Product created: {ProductCode} by {Manufacturer}",
    product.Code, product.Manufacturer);
```

**Logging Levels:**

- **Error** - Exceptions, failures requiring attention
- **Warning** - Handled exceptions, degraded functionality
- **Information** - Important business events (product created, soft delete, etc.)
- **Debug** - Detailed diagnostic information
- **Trace** - Very detailed debugging (avoid in production)

**HTTP Status Code Mapping:**

| Exception Type                         | HTTP Status               | When to Use                           |
| -------------------------------------- | ------------------------- | ------------------------------------- |
| `ItemNotFoundException`                | 404 Not Found             | Resource doesn't exist                |
| `BusinessValidationException`          | 400 Bad Request (default) | Business rule violation               |
| `ConflictException`                    | 409 Conflict              | Duplicate resource, concurrency issue |
| `AccessForbiddenException`             | 403 Forbidden             | User lacks permission                 |
| `FluentValidation.ValidationException` | 400 Bad Request           | Input validation failure              |
| Unhandled exceptions                   | 500 Internal Server Error | Unexpected errors                     |

**Error Response Format:**

All errors return ProblemDetails (RFC 7807):

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Validation failed for one or more fields",
  "traceId": "00-abc123...",
  "errors": {
    "Email": ["Email is required"],
    "Age": ["Age must be greater than 0"]
  }
}
```

**Best Practices:**

- Use custom exceptions for domain-specific errors
- Let `GlobalExceptionHandler` convert to HTTP responses
- Log exceptions at the appropriate level
- **Debug** - Detailed diagnostic information
- **Trace** - Very detailed debugging (avoid in production)

**HTTP Status Code Mapping:**

| Exception Type                         | HTTP Status               | When to Use                           |
| -------------------------------------- | ------------------------- | ------------------------------------- |
| `ItemNotFoundException`                | 404 Not Found             | Resource doesn't exist                |
| `BusinessValidationException`          | 400 Bad Request (default) | Business rule violation               |
| `ConflictException`                    | 409 Conflict              | Duplicate resource, concurrency issue |
| `AccessForbiddenException`             | 403 Forbidden             | User lacks permission                 |
| `FluentValidation.ValidationException` | 400 Bad Request           | Input validation failure              |
| Unhandled exceptions                   | 500 Internal Server Error | Unexpected errors                     |

**Error Response Format:**

All errors return ProblemDetails (RFC 7807):

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Validation failed for one or more fields",
  "traceId": "00-abc123...",
  "errors": {
    "Email": ["Email is required"],
    "Age": ["Age must be greater than 0"]
  }
}
```

**Best Practices:**

- Use custom exceptions for domain-specific errors
- Let `GlobalExceptionHandler` convert to HTTP responses
- Log exceptions at the appropriate level
- Include context in exception messages
- Use structured logging for better querying
- Never expose sensitive data in error messages
- Include trace IDs for correlation

### Common Commands Reference

**Solution & Build:**

```bash
# Restore packages
dotnet restore

# Build entire solution
dotnet build

# Build specific project
dotnet build src/Modest.API/Modest.API.csproj

# Build in Release mode
dotnet build --configuration Release

# Clean build artifacts
dotnet clean
```

**Testing:**

```bash
# Run all tests
dotnet test

# Run tests in specific project
dotnet test tests/Modest.IntegrationTests/

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests with detailed output
dotnet test --verbosity detailed

# Run specific test
dotnet test --filter "FullyQualifiedName~ProductEndpointsTests"
```

**Package Management:**

```bash
# List outdated packages
dotnet list package --outdated

# Add package (with central version management)
dotnet add package PackageName

# Update package version in Directory.Packages.props
# Edit Directory.Packages.props manually
```

**Project Operations:**

```bash
# Create new class library
dotnet new classlib -n ProjectName -o src/ProjectName

# Add project to solution
dotnet sln add src/ProjectName/ProjectName.csproj

# Add project reference
dotnet add src/Modest.API/Modest.API.csproj reference src/Modest.Core/Modest.Core.csproj
```

**Running Projects:**

```bash
# Run API
dotnet run --project src/Modest.API/Modest.API.csproj

# Run with specific environment
dotnet run --project src/Modest.API/Modest.API.csproj --environment Development

# Watch mode (auto-rebuild on changes)
dotnet watch --project src/Modest.API/Modest.API.csproj

# Run using Docker Compose (recommended for local development)
docker-compose -f mongodb-docker/compose.yml up -d
```

**Code Analysis:**

```bash
# Format code
dotnet format

# Check code style without changes
dotnet format --verify-no-changes

# Run analyzers
dotnet build /p:RunAnalyzers=true
```

**Environment Setup:**

```bash
# Verify .NET version
dotnet --version  # Should be 9.0.x

# List installed SDKs
dotnet --list-sdks
```

### Configuration & Secrets

**Configuration Sources:**

1. **appsettings.json** - Local development defaults
2. **appsettings.Development.json** - Development overrides
3. **Environment Variables** - Override configuration per environment
4. **User Secrets** - Local development secrets (`dotnet user-secrets`)

**Configuration Patterns:**

```csharp
// appsettings.json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "modest"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    }
  }
}
```

**MongoDB Configuration:**

```csharp
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));
```

**Security Best Practices:**

- **NEVER** commit secrets to source control
- Use User Secrets for local development:

  ```bash
  dotnet user-secrets set "MongoDB:ConnectionString" "mongodb://..."
  ```

- Keep connection strings and API keys out of source control
- Use environment-specific appsettings files

### Debugging Tips

**Pre-Debugging Checklist:**

Before reporting issues or debugging, verify:

- [ ] All packages restored: `dotnet restore`
- [ ] Solution builds: `dotnet build`
- [ ] Correct .NET 9.0 SDK installed: `dotnet --version`
- [ ] IDE (VS Code/Rider/Visual Studio) using correct SDK version
- [ ] MongoDB is running (Docker Compose or local instance)
- [ ] No conflicting processes on required ports

**Common Issues & Solutions:**

### 1. MongoDB Connection Issues

```bash
# Start MongoDB using Docker Compose
docker-compose -f mongodb-docker/compose.yml up -d

# Check MongoDB is running
docker ps | grep mongo

# Check connection string in appsettings.json
# Default: mongodb://localhost:27017
```

### 2. Build Errors After Pull

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### 3. Test Discovery Issues

```bash
# Rebuild test project
dotnet build tests/Modest.IntegrationTests/
# Run with diagnostics
dotnet test --verbosity detailed
```

**Effective Error Reporting:**

When asking for help or reporting issues:

1. **Provide context:**
   - What you were trying to do
   - What you expected to happen
   - What actually happened

2. **Include relevant information:**
   - Full error message and stack trace
   - Project/file affected
   - Recent changes made
   - .NET SDK version: `dotnet --version`

3. **Code snippets:**
   - Use markdown code blocks
   - Include enough context (10-20 lines around error)
   - Indicate line numbers if relevant

4. **Build/test output:**
   - Run with `--verbosity detailed` for more info

**Debugging Tools:**

- **VS Code:** Use built-in debugger with launch configurations
- **JetBrains Rider:** Excellent .NET debugging experience
- **dotnet-trace:** Performance investigation

  ```bash
  dotnet tool install --global dotnet-trace
  dotnet trace collect --process-id <PID>
  ```

- **dotnet-counters:** Live performance metrics

  ```bash
  dotnet tool install --global dotnet-counters
  dotnet counters monitor --process-id <PID>
  ```

**Project-Specific Troubleshooting:**

- **MongoDB Connection:** Ensure MongoDB is running via Docker Compose
- **Database Queries:** Enable MongoDB command logging to see actual queries
- **Test Containers:** Each test gets a fresh MongoDB container - ensure Docker is running
- **API Endpoints:** Access Swagger UI at `/swagger` when API is running

**Performance Debugging:**

- Enable MongoDB query logging to see slow queries
- Use MongoDB profiling to analyze query performance
- Check FastEndpoints' built-in performance features
