# .NET 8 Clean Architecture Backend with GitHub Copilot Demo

This project demonstrates a .NET 8 Web API implementation following Clean Architecture principles, integrated with GitHub Copilot for enhanced development experience.

## 🚀 Project Overview

This backend application is built with:
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core with SQLite
- Clean Architecture
- MediatR for CQRS
- FluentValidation
- Mapster
- OpenTelemetry
- Structured Logging (Serilog)
- xUnit for testing

## 🛠 Prerequisites

- .NET 8 SDK
- Visual Studio Code
- GitHub Copilot extension
- Git

## 📦 Getting Started

1. Clone the repository:
```bash
git clone <your-repository-url>
cd Backend
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Apply database migrations:
```bash
dotnet ef database update
```

4. Run the application:
```bash
dotnet run
```

The API will be available at `https://localhost:5001`

## 🧪 Running Tests

Execute tests using:
```bash
dotnet test
```

## 🤖 GitHub Copilot Integration Guide

### Setting Up GitHub Copilot

1. Install GitHub Copilot in VS Code:
   - Open VS Code
   - Go to Extensions (Ctrl+Shift+X)
   - Search for "GitHub Copilot"
   - Click Install

2. Authenticate with GitHub:
   - After installation, click on the GitHub Copilot icon in the status bar
   - Follow the authentication prompts

### Using Custom Instructions with GitHub Copilot

This project includes custom instructions for GitHub Copilot to ensure consistent code generation following our architecture patterns.

#### Key Custom Instruction Areas:

1. **Architecture Standards**
   - Clean Architecture implementation
   - SOLID principles adherence
   - Domain-Driven Design practices

2. **Code Organization**
   ```
   Backend/
   ├── Controllers/
   ├── Models/
   ├── Dtos/
   ├── Repository/
   ├── DbContext/
   ├── Services/
   ├── Validators/
   └── Tests/
   ```

3. **Best Practices**
   - Input validation using FluentValidation
   - OpenTelemetry integration
   - Structured logging with Serilog
   - Unit testing requirements
   - Dependency injection patterns

### 💡 Copilot Usage Tips

1. **Generate Controllers**
   - Type `// ProductsController` and let Copilot suggest the implementation
   - Copilot will follow the project's CQRS pattern

2. **Create DTOs**
   - Start with `public record ProductDto` and Copilot will suggest properties
   - Copilot maintains naming conventions

3. **Repository Implementation**
   - Type `// IProductRepository interface` for interface suggestions
   - Implementation will include proper OpenTelemetry tracing

4. **Unit Tests**
   - Start with `[Fact]` and let Copilot suggest test cases
   - Follows project's testing patterns with xUnit

### 🔍 Validation

1. **Architecture Compliance**
   - Copilot suggestions maintain separation of concerns
   - Proper dependency injection patterns
   - Clean Architecture layer separation

2. **Code Quality**
   - Maintains consistent naming conventions
   - Includes required logging and tracing
   - Proper exception handling

## 🏗 Project Structure

The project follows Clean Architecture with these key components:

- **Controllers**: API endpoints
- **Models**: Domain entities
- **DTOs**: Data transfer objects
- **Repository**: Data access layer
- **Services**: Business logic
- **Validators**: Request validation
- **Tests**: Unit and integration tests

## 📚 Additional Resources

- [Clean Architecture Documentation](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [GitHub Copilot Documentation](https://docs.github.com/en/copilot)
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)