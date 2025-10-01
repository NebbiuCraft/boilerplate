using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.HealthChecks;

/// <summary>
/// Health check for database connectivity and EF Core context
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
  private readonly AppDbContext _dbContext;
  private readonly ILogger<DatabaseHealthCheck> _logger;

  public DatabaseHealthCheck(AppDbContext dbContext, ILogger<DatabaseHealthCheck> logger)
  {
    _dbContext = dbContext;
    _logger = logger;
  }

  public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    try
    {
      _logger.LogDebug("Starting database connectivity health check");

      var startTime = DateTime.UtcNow;

      // Test database connectivity with a simple query
      var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

      if (!canConnect)
      {
        return HealthCheckResult.Unhealthy("Cannot connect to database", null, new Dictionary<string, object>
        {
          ["database"] = "SQLite",
          ["connectionTest"] = "FAILED"
        });
      }

      // Test a simple query to ensure database is operational
      await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);

      var duration = DateTime.UtcNow - startTime;

      _logger.LogDebug("Database health check completed successfully in {Duration}ms", duration.TotalMilliseconds);

      return HealthCheckResult.Healthy("Database is accessible and operational", new Dictionary<string, object>
      {
        ["database"] = "SQLite",
        ["responseTime"] = duration.TotalMilliseconds,
        ["connectionTest"] = "SUCCESS",
        ["queryTest"] = "SUCCESS"
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Database health check failed");

      return HealthCheckResult.Unhealthy("Database is not accessible", ex, new Dictionary<string, object>
      {
        ["database"] = "SQLite",
        ["error"] = ex.Message,
        ["errorType"] = ex.GetType().Name
      });
    }
  }
}