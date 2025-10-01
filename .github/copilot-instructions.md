## Copilot Instructions for Boilerplate (Order Management System)

### Architecture Overview

-   **Clean Architecture + DDD**: Five-layer structure with clear separation of concerns
    -   `Domain`: Business entities (`Order`, `OrderItem`), repository interfaces, domain services, and custom exceptions
    -   `Application`: CQRS handlers, DTOs, AutoMapper profiles, and application service interfaces
    -   `Infrastructure`: EF Core persistence, repository implementations, and data access
    -   `Services`: External service implementations (`FakePaymentService` implementing `IPaymentService`)
    -   `WebApi`: Controllers, middleware (`GlobalExceptionHandlingMiddleware`), and startup configuration

### Data Flow & Request Pipeline

1. **API → MediatR → Handler → Repository**: Controllers use `IMediator.Send()` to dispatch to handlers
2. **Rich Domain Models**: Entities have business methods (e.g., `Order.AddItem()`, `Order.MarkPaid()`) with validation
3. **Domain Events**: Entities raise domain events via `RaiseDomainEvent()`, published through `IDomainEventPublisher`
4. **Global Exception Handling**: Domain exceptions are caught by middleware and converted to proper HTTP responses
5. **AutoMapper Integration**: DTOs are mapped to/from entities in handlers using `IMapper`

### Key Patterns & Conventions

-   **CQRS Structure**: Commands/Queries in `Application/Orders/Commands|Queries/` with co-located handlers
-   **Record-based Commands**: Use `public record CreateOrderCommand(CreateOrderDto OrderDto) : IRequest<int>`
-   **Repository Pattern**: All repos inherit from `EfRepository<T>` base class with soft delete support (`entity.Active = false`)
-   **Aggregate Roots**: Domain entities implement `IAggregateRoot` and extend `Entity` base class
-   **Rich Domain Logic**: Business rules in entity methods, not in handlers (e.g., `Order.AddItem()` validates quantity > 0)
-   **Service Interfaces**: Application layer defines interfaces (`IPaymentService`), Services layer provides implementations
-   **Domain Events**: Entities inherit from `Entity` base class with `RaiseDomainEvent()`, published via `IDomainEventPublisher`
-   **Event Handlers**: Domain events trigger side effects through `INotificationHandler<DomainEventNotification>`

### Database & Migrations

-   **SQLite Database**: `boilerplate.db` file, connection string in `appsettings.json`
-   **Auto-Migration**: `Program.cs` runs `dbContext.Database.Migrate()` on startup
-   **EF Configurations**: Entity configurations in `Infrastructure/Persistence/Configurations/`
-   **Migration Commands**: Use npm scripts (`ef:add-migration`, `ef:update-db`, `ef:reset-db`) from package.json

### Development Workflow

**Build/Run:**

-   `npm run build` or `dotnet build` from solution root
-   `npm run run` or `npm run run:watch` for development with hot reload
-   API serves at `https://localhost:7XXX` with Swagger UI at `/swagger`
-   **Structured Logging**: Serilog outputs to console and file (`logs/` directory)

**Database Management:**

-   `npm run ef:add-migration <MigrationName>` to create migrations
-   `npm run ef:update-db` to apply pending migrations
-   `npm run ef:reset-db` to drop and recreate database

**Testing:**

-   No test project present - create in solution root following existing naming patterns

### Logging & Observability

-   **Serilog Integration**: Structured logging with console and file outputs
-   **Request Logging**: HTTP requests automatically logged with timing and context
-   **Rich Context**: Domain events, command handlers, and controllers use structured logging
-   **Log Scopes**: Event handlers use logging scopes for better correlation
-   **Exception Handling**: Global middleware provides structured exception logging
-   **Configuration**: Logging configured via `appsettings.json` with environment-specific overrides

### Health Checks & Monitoring

-   **Clean Architecture**: Health checks in appropriate layers (Infrastructure for DB, Services for external services)
-   **Comprehensive Endpoints**: `/api/Health` (detailed), `/api/Health/live` (liveness), `/api/Health/ready` (readiness)
-   **Structured Logging**: Health check results logged with detailed context and timing
-   **Container Ready**: Separate liveness/readiness probes for Kubernetes/Docker deployments
-   **Extension Pattern**: Each layer provides health check registration extensions (`AddInfrastructureHealthChecks`, `AddPaymentServiceHealthChecks`)

### Exception Handling Strategy

-   **Domain Exceptions**: Custom exceptions in `Domain/Exceptions/` (e.g., `InvalidOrderItemException`)
-   **Middleware Processing**: `GlobalExceptionHandlingMiddleware` maps domain exceptions to HTTP status codes
-   **Validation**: Business rule validation in entity methods, not in controllers or handlers

### Domain Events System

-   **Event Definition**: Domain events inherit from `DomainEvent` record in `Domain/Events/`
-   **Event Raising**: Entities use `RaiseDomainEvent()` method from `Entity` base class
-   **Event Publishing**: `IDomainEventPublisher` publishes events through MediatR after business operations
-   **Event Handling**: `INotificationHandler<DomainEventNotification>` in `Application/Orders/EventHandlers/`
-   **Event Types**: Order lifecycle events (`OrderCreated`, `OrderItemAdded`, `OrderTotalCalculated`) and payment events (`PaymentInitiated`, `PaymentSuccessful`, `PaymentFailed`)
-   **Mock Handlers**: All event handlers include mock business logic (emails, analytics, inventory, fraud detection)

### Service Registration Pattern

-   **Extension Methods**: Each layer has `DependencyInjection.cs` with `Add[Layer]()` extension
-   **MediatR**: Auto-registered from Application assembly
-   **AutoMapper**: Auto-registered with profiles from Application assembly
-   **Repositories**: Manually registered in Infrastructure DI (not auto-discovered)
-   **Domain Events**: `IDomainEventPublisher` registered as scoped service in Application layer

### Key Files for Extension

-   `src/Application/Orders/Commands/CreateOrderCommand.cs`: Command/handler pattern example
-   `src/Domain/Entities/Order.cs`: Rich domain model with business methods
-   `src/Infrastructure/Repositories/EfRepository.cs`: Base repository with soft delete
-   `src/WebApi/Middleware/GlobalExceptionHandlingMiddleware.cs`: Exception → HTTP mapping
-   `src/Services/FakePaymentService/FakePaymentService.cs`: External service implementation pattern
-   `src/Domain/Events/*.cs`: Domain events for order/payment lifecycle
-   `src/Application/Orders/EventHandlers/*.cs`: Domain event handlers with mock business logic
-   `src/WebApi/Configuration/LoggingConfiguration.cs`: Serilog configuration and setup
-   `src/Infrastructure/HealthChecks/DatabaseHealthCheck.cs`: Database health monitoring
-   `src/Services/FakePaymentService/HealthChecks/PaymentServiceHealthCheck.cs`: Payment service health monitoring
-   `src/WebApi/Controllers/HealthController.cs`: Health check API endpoints
-   `package.json`: All EF Core and development commands

---

**Adding New Features:** Follow existing Order patterns - create commands/queries in Application, add business logic to Domain entities, implement repositories in Infrastructure, expose via WebApi controllers.
