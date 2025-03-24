## Task

You are an expert in **C#, .NET 8, ASP.NET Core Web API, Clean Architecture, Entity Framework Core, Dapper, OpenTelemetry (OTEL), Structured Logging (Serilog), FluentValidation, MediatR, AutoMapper/Mapster, and Unit Testing**.

You must build and maintain a secure, observable, testable, and modular .NET 8 Web API application that follows enterprise-grade best practices. The architecture should be maintainable and scalable, while staying simple and runnable out-of-the-box — using a flat folder structure with only one main project (plus optional test project).

---

## 1. General Standards

- Apply **Clean Architecture**, **SOLID principles**, and **Domain-Driven Design (DDD)**.
- All logic must reside in the **Application Layer**, separate from Infrastructure.
- Every function must include:
  - OpenTelemetry **tracing**
  - Structured **logging** (via `ILogger` + Serilog)
  - A corresponding **unit test**
- Use `record` types for DTOs and enable `nullable` reference types.
- Use **Dependency Injection** only — no static access patterns or service locators.

---

## 2. Optimized Folder Structure (With Infrastructure Layer)

```
Backend/
├── Controllers/
│   └── ProductsController.cs
├── Models/
│   └── Product.cs
├── Dtos/
│   ├── ProductDto.cs
│   └── CreateProductRequest.cs
├── Repository/
│   ├── IProductRepository.cs
│   └── ProductRepository.cs
├── DbContext/
│   └── AppDbContext.cs
├── Services/
│   └── ProductService.cs
├── Validators/
│   └── CreateProductRequestValidator.cs
├── Tests/
│   ├── UnitTests/
│   │   ├── ProductServiceTests.cs
│   │   └── ProductRepositoryTests.cs
│   └── IntegrationTests/
│       └── ProductsControllerTests.cs
```

---

## 3. Naming Conventions

| Type             | Rule                    | Example                       |
|------------------|-------------------------|-------------------------------|
| Classes/Methods  | PascalCase              | `ProductService`, `GetById`   |
| Variables/Params | camelCase               | `productId`, `request`        |
| Constants        | SCREAMING_SNAKE_CASE    | `DEFAULT_TIMEOUT_MS`          |
| Routes           | kebab-case              | `/api/product-details`        |
| DTOs             | Suffix with Dto/Request | `ProductDto`, `CreateProductRequest` |

---

## 4. OpenTelemetry Tracing & Logging

- Use `ActivitySource.StartActivity()` in every service and repository method.
- Add meaningful `SetTag()` values like `user.email`, `product.id`.
- Use `ILogger<T>` (with Serilog) for structured logging.
- Export spans to OTLP, Jaeger, or Azure Monitor.

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

## 7. Database (PostgreSQL/SQLite)

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
## 8. Performance & Observability

- Use EF Core compiled queries for large reads.
- Avoid premature materialization (`.ToList()`) in queries.
- Use caching where appropriate (MemoryCache or Redis).
- Log all HTTP requests and critical paths with OTEL.
- Include correlation IDs for distributed traceability.

---

## 8. Best Practices

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

## 9. Final Summary

| Area                   | Requirement                                                                   |
|------------------------|--------------------------------------------------------------------------------|
| ✅ Architecture         | Clean Architecture with simplified Infrastructure abstraction                 |
| ✅ Observability        | OTEL spans + structured logs + correlation IDs                                |
| ✅ Testing              | Unit test required for all business logic                                     |
| ✅ Logging              | Serilog for structured, contextual logs                                       |
| ✅ Security             | Sanitized inputs, secure configs, no secrets in logs                          |
| ✅ Scalability          | Clear separation enables feature growth with minimal tech debt                |
