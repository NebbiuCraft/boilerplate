using Microsoft.Extensions.Diagnostics.HealthChecks;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Infrastructure.HealthChecks;

/// <summary>
/// Health check for the legacy database connection
/// </summary>
public class LegacyDatabaseHealthCheck : IHealthCheck
{
  private readonly LegacyDbContext _context;
  private readonly ILogger<LegacyDatabaseHealthCheck> _logger;

  public LegacyDatabaseHealthCheck(LegacyDbContext context, ILogger<LegacyDatabaseHealthCheck> logger)
  {
    _context = context;
    _logger = logger;
  }

  public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    try
    {
      _logger.LogDebug("Checking legacy database health...");

      // Try to open a connection and execute a simple query
      await _context.Database.CanConnectAsync(cancellationToken);

      // Optional: Execute a simple query to verify database accessibility
      // You can uncomment this once you have tables scaffolded
      // var count = await _context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);

      _logger.LogDebug("Legacy database health check passed");

      return HealthCheckResult.Healthy("Legacy database is accessible", new Dictionary<string, object>
      {
        ["database"] = _context.Database.GetConnectionString() ?? "Unknown",
        ["provider"] = _context.Database.ProviderName ?? "Unknown"
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Legacy database health check failed");

      return HealthCheckResult.Unhealthy(
          "Legacy database is not accessible",
          ex,
          new Dictionary<string, object>
          {
            ["database"] = _context.Database.GetConnectionString() ?? "Unknown",
            ["provider"] = _context.Database.ProviderName ?? "Unknown",
            ["error"] = ex.Message
          });
    }
  }
}