# .NET 8 Web API Best Practices & Guidelines

## Task
You are an expert in **C#, .NET 8, ASP.NET Core Web API, Clean Architecture, Entity Framework Core, Dapper, OpenTelemetry (OTEL), Structured Logging (Serilog), FluentValidation, MediatR, AutoMapper/Mapster, and Unit Testing**.

You must build and maintain a secure, observable, testable, and modular .NET 8 Web API application that follows enterprise-grade best practices. The architecture should be maintainable and scalable, while staying simple and runnable out-of-the-box — using a flat folder structure with only one main project (plus optional test project).

**Important Notes:**
- When using MCP Postgres, use MCP server direct queries to Postgres only (no codebase searching needed)
- Always validate changes with integration tests
- Always implement error handling and logging

---

## 1. General Standards

- Apply **Clean Architecture**, **SOLID principles**, and **Domain-Driven Design (DDD)**.
- All logic must reside in the **Application Layer**, separate from Infrastructure.
- Every function must include:
  - OpenTelemetry **tracing**
  - Structured **logging** (via `ILogger` + Serilog)
  - A corresponding **unit test**
  - Proper error handling with standardized responses
- Use `record` types for DTOs and enable `nullable` reference types.
- Use **Dependency Injection** only — no static access patterns or service locators.

### Error Handling Standards
```csharp
// Standard error response structure
public record ApiError(string Message, string? Details = null);

// Controller error handling pattern
try 
{
    // Business logic
}
catch (NotFoundException ex)
{
    _logger.LogWarning(ex, "Resource not found");
    return NotFound(new ApiError(ex.Message));
}
catch (ValidationException ex)
{
    _logger.LogWarning(ex, "Validation failed");
    return BadRequest(new ApiError(ex.Message));
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error");
    return StatusCode(500, new ApiError("An unexpected error occurred"));
}
```

---

## 2. Module Creation Requirements

### Module Creation Process
1. **Planning Phase**
   - Identify the module's domain model and relationships
   - Define the required endpoints and DTOs
   - Plan the database schema changes
   - Identify integration points with other modules

2. **Implementation Order**
   - Create the domain model class extending BaseEntity
   - Add DbSet to AppDbContext and create migration
   - Implement Repository interface and class
   - Create DTOs and mapping configurations
   - Implement Service with business logic
   - Create FluentValidation validator
   - Implement Controller endpoints
   - Write unit and integration tests

3. **Integration Steps**
   - Register dependencies in Program.cs
   - Update DatabaseSeeder if needed
   - Add OpenAPI documentation
   - Implement logging and telemetry
   - Create integration tests

4. **Testing Checklist**
   - Unit tests for Repository
   - Unit tests for Service
   - Unit tests for Validator
   - Integration tests for Controller
   - Test database migrations
   - Test error handling scenarios

### Required Components for Each Module
```
ModuleName/
├── Controllers/
│   └── ModuleNameController.cs       # REST API endpoints
├── Models/
│   └── ModuleName.cs                 # Domain model
├── Dtos/
│   ├── ModuleNameDto.cs              # Response DTO
│   └── CreateModuleNameRequest.cs    # Request DTO
├── Repository/
│   ├── IModuleNameRepository.cs      # Repository interface
│   └── ModuleNameRepository.cs       # Repository implementation
├── Services/
│   └── ModuleNameService.cs          # Business logic
├── Validators/
│   └── CreateModuleNameValidator.cs  # Request validation
├── Tests/
│   ├── UnitTests/
│   │   ├── ModuleNameServiceTests.cs
│   │   └── ModuleNameRepositoryTests.cs
│   └── IntegrationTests/
│       └── ModuleNameControllerTests.cs
```

### Module Implementation Checklist
- [ ] Entity Model with required attributes
- [ ] DTOs with proper documentation
- [ ] Repository interface and implementation
- [ ] Service with OTEL tracing
- [ ] Controller with standard REST endpoints
- [ ] FluentValidation validator
- [ ] Unit tests (min 80% coverage)
- [ ] Integration tests for all endpoints
- [ ] Error handling in all layers
- [ ] OpenAPI documentation
- [ ] Proper logging setup

---

## 3. Standard Base Classes and Interfaces

```csharp
// Base Repository Interface
public interface IBaseRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> CreateAsync(T entity);
    Task<T?> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}

// Base Service Class
public abstract class BaseService<T>
{
    protected readonly ILogger<BaseService<T>> _logger;
    protected readonly ActivitySource _activitySource;
    
    protected BaseService(ILogger<BaseService<T>> logger)
    {
        _logger = logger;
        _activitySource = new ActivitySource(typeof(T).Name);
    }
}

// Base Controller Class
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected readonly ILogger _logger;
    protected static readonly ActivitySource _activitySource;

    protected BaseApiController(ILogger logger)
    {
        _logger = logger;
        _activitySource = new ActivitySource(GetType().Name);
    }

    protected IActionResult HandleError(Exception ex)
    {
        return ex switch
        {
            NotFoundException => NotFound(new ApiError(ex.Message)),
            ValidationException => BadRequest(new ApiError(ex.Message)),
            _ => StatusCode(500, new ApiError("An unexpected error occurred"))
        };
    }
}
```

---

## 4. OpenTelemetry & Logging Standards

### OTEL Implementation
```csharp
// Service method pattern
public async Task<T> DoSomethingAsync()
{
    using var activity = _activitySource.StartActivity("DoSomething");
    try 
    {
        activity?.SetTag("custom.tag", "value");
        // Method implementation
    }
    catch (Exception ex)
    {
        activity?.SetTag("error", true);
        activity?.SetTag("error.type", ex.GetType().Name);
        throw;
    }
}
```

### Logging Standards
- Use semantic logging with structured data
- Include correlation IDs
- Log at appropriate levels:
  - TRACE: Detailed debugging
  - DEBUG: Debugging info
  - INFO: Normal operations
  - WARN: Unexpected but handled issues
  - ERROR: Unhandled exceptions
  - FATAL: Application crashes

---

## 5. Unit Testing Requirements

- **Every function must have a unit test**.
- Use `xUnit` for test framework, `Moq` for mocking, `FluentAssertions` for expressive assertions.
- Place unit tests in `/tests/UnitTests/UseCases/<Feature>`

---

## 6. Security Best Practices

- Validate all inputs using `FluentValidation`.
- Secure secrets via:
  - Azure Key Vault
  - Environment Variables
  - dotnet `user-secrets` in development
- Never log sensitive information like passwords or tokens.

---

## 7. Database Standards

### Entity Requirements
- All entities must include:
  ```csharp
  public class BaseEntity
  {
      public int Id { get; set; }
      public DateTime CreatedAt { get; set; }
      public DateTime UpdatedAt { get; set; }
      public string CreatedBy { get; set; } = string.Empty;
      public string UpdatedBy { get; set; } = string.Empty;
  }
  ```

### Query Optimization
- Use compiled queries for frequent operations
- Implement proper indexing strategy
- Use projections for large datasets
- Implement caching where appropriate

- Use **PostgreSQL/SQLite** as the primary relational database.
- Configure connection via `appsettings.json` and environment variables.
- Use **Npgsql** or  **SQLite** provider for EF Core:
**PostgreSQL**
  ```bash
  dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
  ```
**SQLite**
  ```bash 
  dotnet add package Microsoft.EntityFrameworkCore.Sqlite
  ```
- Use migrations for schema versioning:
  ```bash
  dotnet ef migrations add InitialCreate
  dotnet ef database update
  ```
- Place `DbContext` folder and register it in `Program.cs`:
  **PostgreSQL**
  ```csharp
  services.AddDbContext<AppDbContext>(options =>
      options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
  ```
  **SQLite**
  ```csharp
  var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite("Filename=:memory:")
    .Options;
  ```
- Make sure to enable PostgreSQL telemetry:
  ```csharp
  builder.Services.AddOpenTelemetry()
      .WithTracing(b => b.AddNpgsql());
  ```

---

## 8. Performance Requirements

### Response Time Targets
- API endpoints should respond within:
  - GET: 100ms
  - POST/PUT: 200ms
  - DELETE: 100ms
- Batch operations should use pagination
- Cache frequently accessed data
- Use async/await consistently

### Monitoring Requirements
- Track all HTTP requests
- Monitor database query performance
- Log all critical operations
- Set up alerts for:
  - Response time > 500ms
  - Error rate > 1%
  - Database connection issues

---

## 9. Best Practices

| Feature                     | Benefit                                                                 |
|-----------------------------|--------------------------------------------------------------------------|
| **MediatR**               | Clean CQRS with logging/validation pipeline behaviors                    |
| **FluentValidation**      | Declarative input validation with DI                                    |
| **AutoMapper / Mapster**  | Reduces manual DTO mapping                                               |
| **Validation Pipelines**  | Global request validation in MediatR handlers                            |
| **Minimal APIs**          | Simpler routes for lightweight endpoints or microservices                |
| **EF Fluent Configs**     | Clean separation for model configuration logic                           |
| **Domain Events**         | Decouple domain logic from infrastructure side effects                   |

---

## 10. Final Summary

| Area                   | Requirement                                                                   |
|------------------------|--------------------------------------------------------------------------------|
| Architecture         | Clean Architecture with simplified Infrastructure abstraction                 |
| Observability        | OTEL spans + structured logs + correlation IDs                                |
| Testing              | Unit test required for all business logic                                     |
| Logging              | Serilog for structured, contextual logs                                       |
| Security             | Sanitized inputs, secure configs, no secrets in logs                          |
| Scalability          | Clear separation enables feature growth with minimal tech debt                |

---

## 11. Deployment & DevOps

### CI/CD Requirements
- Automated builds
- Run all tests
- Code coverage reports
- Security scanning
- Automated deployment

### Environment Configuration
- Use proper configuration per environment
- Secure sensitive data
- Implement health checks
- Set up monitoring

---

## 12. Documentation Requirements

### API Documentation
- OpenAPI/Swagger documentation
- XML comments for all public APIs
- Example requests/responses
- Error response documentation

### Code Documentation
- XML comments for public methods
- README.md for each module
- Architecture decision records
- Deployment instructions
