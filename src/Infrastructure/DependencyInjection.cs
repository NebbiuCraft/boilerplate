using Domain.Repositories;
using Infrastructure.HealthChecks;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Main application database (SQLite)
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        // Legacy database (SQL Server)
        services.AddDbContext<LegacyDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("LegacyConnection")));

        // Repository registrations
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Legacy repository registrations will be added here
        // Example: services.AddScoped<ILegacyTableRepository, LegacyTableRepository>();

        return services;
    }

    /// <summary>
    /// Add infrastructure health checks
    /// </summary>
    public static IHealthChecksBuilder AddInfrastructureHealthChecks(this IHealthChecksBuilder builder)
    {
        return builder
            .AddCheck<DatabaseHealthCheck>("database")
            .AddCheck<LegacyDatabaseHealthCheck>("legacy-database");
    }
}