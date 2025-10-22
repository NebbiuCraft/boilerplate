# Legacy Database Integration Guide

This guide explains how to integrate a pre-existing legacy database using EF Core Database-First approach.

## Overview

The legacy database integration provides:

-   **Read-only access** to legacy system data
-   **Separate DbContext** (`LegacyDbContext`) from main application database
-   **Health monitoring** for legacy database connectivity
-   **Repository pattern** for clean data access
-   **Database-first scaffolding** for automatic entity generation

## Setup Process

### 1. Update Connection String

Update `appsettings.json` with your actual legacy database connection:

```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Data Source=boilerplate.db",
        "LegacyConnection": "Server=your-server;Database=YourLegacyDB;Trusted_Connection=true;TrustServerCertificate=true;"
    }
}
```

For SQL Server Authentication, use:

```json
"LegacyConnection": "Server=your-server;Database=YourLegacyDB;User Id=username;Password=password;TrustServerCertificate=true;"
```

### 2. Scaffold Database Entities

Run the scaffolding script to generate entities from your existing database:

```bash
cd src/Infrastructure
./scaffold-legacy-db.sh
```

This will:

-   Generate entity classes in `Entities/Legacy/`
-   Update `LegacyDbContext` with DbSets and configurations
-   Use database-first approach with data annotations

### 3. Manual Scaffolding (Alternative)

If you prefer manual control, use the EF Core CLI directly:

```bash
dotnet ef dbcontext scaffold \
    "Server=localhost;Database=LegacySystem;Trusted_Connection=true;TrustServerCertificate=true;" \
    Microsoft.EntityFrameworkCore.SqlServer \
    --output-dir "./Entities/Legacy" \
    --context-dir "./Persistence" \
    --context "LegacyDbContext" \
    --namespace "Infrastructure.Entities.Legacy" \
    --context-namespace "Infrastructure.Persistence" \
    --data-annotations \
    --use-database-names \
    --force
```

### 4. Create Repository Interfaces

Create interfaces in the Domain layer for legacy entities:

```csharp
// src/Domain/Repositories/ILegacyCustomerRepository.cs
namespace Domain.Repositories;

public interface ILegacyCustomerRepository
{
    Task<LegacyCustomer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    IQueryable<LegacyCustomer> GetAll();
    IQueryable<LegacyCustomer> GetByEmail(string email);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
```

### 5. Implement Repositories

Create repository implementations that inherit from `LegacyRepository<T>`:

```csharp
// src/Infrastructure/Repositories/LegacyCustomerRepository.cs
using Infrastructure.Entities.Legacy;
using Infrastructure.Persistence;
using Domain.Repositories;

namespace Infrastructure.Repositories;

public class LegacyCustomerRepository : LegacyRepository<LegacyCustomer>, ILegacyCustomerRepository
{
    public LegacyCustomerRepository(LegacyDbContext context) : base(context)
    {
    }

    public async Task<LegacyCustomer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await base.GetByIdAsync(id, cancellationToken);
    }

    public IQueryable<LegacyCustomer> GetByEmail(string email)
    {
        return Where(c => c.Email == email);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await AnyAsync(c => c.Id == id, cancellationToken);
    }
}
```

### 6. Register Repositories

Add repository registrations to `DependencyInjection.cs`:

```csharp
// Legacy repository registrations
services.AddScoped<ILegacyCustomerRepository, LegacyCustomerRepository>();
services.AddScoped<ILegacyOrderRepository, LegacyOrderRepository>();
```

## Usage Examples

### In Application Layer (Handlers)

```csharp
public class GetCustomerFromLegacyHandler : IRequestHandler<GetCustomerFromLegacyQuery, CustomerDto>
{
    private readonly ILegacyCustomerRepository _legacyRepository;
    private readonly IMapper _mapper;

    public GetCustomerFromLegacyHandler(ILegacyCustomerRepository legacyRepository, IMapper mapper)
    {
        _legacyRepository = legacyRepository;
        _mapper = mapper;
    }

    public async Task<CustomerDto> Handle(GetCustomerFromLegacyQuery request, CancellationToken cancellationToken)
    {
        var legacyCustomer = await _legacyRepository.GetByIdAsync(request.Id, cancellationToken);
        return _mapper.Map<CustomerDto>(legacyCustomer);
    }
}
```

### Complex Queries

```csharp
public async Task<List<LegacyCustomer>> GetActiveCustomersAsync()
{
    return await _legacyRepository
        .Where(c => c.IsActive && c.LastLoginDate > DateTime.Now.AddDays(-30))
        .OrderBy(c => c.LastName)
        .ToListAsync();
}
```

## Architecture Integration

### Clean Architecture Compliance

-   **Domain Layer**: Contains legacy repository interfaces
-   **Application Layer**: Uses legacy repositories through interfaces
-   **Infrastructure Layer**: Contains legacy entities, DbContext, and repository implementations
-   **WebApi Layer**: No direct access to legacy database

### Separation of Concerns

-   **Main Database**: Order management, CRUD operations
-   **Legacy Database**: Read-only queries, historical data, integration data
-   **Independent Health Checks**: Separate monitoring for each database
-   **Isolated Configurations**: Separate connection strings and contexts

## Health Monitoring

The legacy database health check provides:

-   Connection verification
-   Database accessibility testing
-   Detailed health information
-   Separate monitoring from main database

Access health status at:

-   `/api/Health` - Comprehensive health including legacy database
-   Monitor specifically for legacy database status

## Best Practices

### 1. Read-Only Operations

-   Use `AsNoTracking()` for all queries (included in base repository)
-   No change tracking overhead
-   Prevents accidental modifications

### 2. Performance Optimization

```csharp
// Use projection for large datasets
var customerSummaries = await _legacyRepository
    .GetAll()
    .Select(c => new CustomerSummaryDto
    {
        Id = c.Id,
        Name = c.Name,
        Email = c.Email
    })
    .ToListAsync();
```

### 3. Error Handling

```csharp
try
{
    var data = await _legacyRepository.GetByIdAsync(id);
    return data;
}
catch (SqlException ex)
{
    _logger.LogError(ex, "Failed to query legacy database for ID {Id}", id);
    throw new LegacySystemUnavailableException("Legacy system is currently unavailable", ex);
}
```

### 4. Connection Management

-   Use connection pooling (enabled by default)
-   Configure appropriate timeouts
-   Monitor connection usage

## Troubleshooting

### Common Issues

1. **Connection Failures**

    - Verify connection string format
    - Check network connectivity
    - Validate credentials and permissions

2. **Scaffolding Errors**

    - Ensure EF Core tools are installed: `dotnet tool install --global dotnet-ef`
    - Check database accessibility
    - Verify SQL Server provider installation

3. **Performance Issues**
    - Use `AsNoTracking()` for read-only queries
    - Implement proper indexing on legacy database
    - Use projection to limit data transfer

### Logging

Enable detailed logging for legacy database operations:

```json
{
    "Logging": {
        "LogLevel": {
            "Microsoft.EntityFrameworkCore.Database.Command": "Information",
            "Infrastructure.Repositories": "Debug"
        }
    }
}
```

## Security Considerations

-   Use least-privilege database accounts
-   Implement proper connection string security
-   Consider IP restrictions for database access
-   Monitor and log database access patterns
-   Use read-only database user if possible

## Migration Strategy

For updating scaffolded entities when legacy database schema changes:

1. Backup existing entities if customized
2. Re-run scaffolding script with `--force` flag
3. Review and merge any custom changes
4. Test thoroughly before deployment

This setup provides a robust, maintainable way to integrate legacy database access while maintaining clean architecture principles.
