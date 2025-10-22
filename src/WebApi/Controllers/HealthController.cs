using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers;

/// <summary>
/// Health check controller for monitoring application health
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[SwaggerTag("Application health monitoring and diagnostics")]
public class HealthController : ControllerBase
{
  private readonly HealthCheckService _healthCheckService;
  private readonly ILogger<HealthController> _logger;

  public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
  {
    _healthCheckService = healthCheckService;
    _logger = logger;
  }

  /// <summary>
  /// Get comprehensive health status of the application
  /// </summary>
  /// <returns>Detailed health status including all registered health checks</returns>
  /// <response code="200">Health status retrieved successfully</response>
  /// <response code="503">One or more health checks failed</response>
  [HttpGet]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
  [SwaggerOperation(
      Summary = "Get application health status",
      Description = "Returns comprehensive health information including database connectivity, external service status, and overall system health",
      OperationId = "GetHealth",
      Tags = new[] { "Health" }
  )]
  public async Task<IActionResult> GetHealth()
  {
    _logger.LogDebug("Health check requested");

    var report = await _healthCheckService.CheckHealthAsync();

    var response = new
    {
      status = report.Status.ToString(),
      totalDuration = report.TotalDuration.TotalMilliseconds,
      checks = report.Entries.Select(entry => new
      {
        name = entry.Key,
        status = entry.Value.Status.ToString(),
        duration = entry.Value.Duration.TotalMilliseconds,
        description = entry.Value.Description,
        data = entry.Value.Data.Any() ? entry.Value.Data : null,
        exception = entry.Value.Exception?.Message,
        tags = entry.Value.Tags.Any() ? entry.Value.Tags : null
      }).ToList(),
      timestamp = DateTime.UtcNow
    };

    _logger.LogInformation(
        "Health check completed | Overall Status: {HealthStatus} | Total Duration: {Duration}ms | Failed Checks: {FailedCount}",
        report.Status,
        report.TotalDuration.TotalMilliseconds,
        report.Entries.Count(e => e.Value.Status == HealthStatus.Unhealthy));

    // Log individual failed checks
    foreach (var failedCheck in report.Entries.Where(e => e.Value.Status == HealthStatus.Unhealthy))
    {
      _logger.LogWarning(
          "Health check failed | Check: {CheckName} | Duration: {Duration}ms | Exception: {Exception}",
          failedCheck.Key,
          failedCheck.Value.Duration.TotalMilliseconds,
          failedCheck.Value.Exception?.Message ?? "No exception details");
    }

    var statusCode = report.Status switch
    {
      HealthStatus.Healthy => 200,
      HealthStatus.Degraded => 200,
      HealthStatus.Unhealthy => 503,
      _ => 503
    };

    return StatusCode(statusCode, response);
  }

  /// <summary>
  /// Simple liveness probe for container orchestration
  /// </summary>
  /// <returns>Basic liveness status</returns>
  /// <response code="200">Application is alive and responding</response>
  [HttpGet("live")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
  [SwaggerOperation(
      Summary = "Liveness probe",
      Description = "Simple endpoint to verify the application is running. Used by container orchestrators like Kubernetes for liveness probes",
      OperationId = "GetLiveness",
      Tags = new[] { "Health" }
  )]
  public IActionResult GetLiveness()
  {
    _logger.LogDebug("Liveness probe requested");

    return Ok(new
    {
      status = "Alive",
      timestamp = DateTime.UtcNow,
      uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime
    });
  }

  /// <summary>
  /// Readiness probe for container orchestration
  /// </summary>
  /// <returns>Readiness status based on health checks</returns>
  /// <response code="200">Application is ready to serve requests</response>
  /// <response code="503">Application is not ready (dependencies unavailable)</response>
  [HttpGet("ready")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
  [SwaggerOperation(
      Summary = "Readiness probe",
      Description = "Endpoint that checks if the application is ready to serve requests by verifying all dependencies are healthy. Used by container orchestrators for readiness probes",
      OperationId = "GetReadiness",
      Tags = new[] { "Health" }
  )]
  public async Task<IActionResult> GetReadiness()
  {
    _logger.LogDebug("Readiness probe requested");

    var report = await _healthCheckService.CheckHealthAsync();

    var response = new
    {
      status = report.Status == HealthStatus.Healthy ? "Ready" : "NotReady",
      timestamp = DateTime.UtcNow,
      details = report.Status.ToString()
    };

    _logger.LogDebug(
        "Readiness check completed | Status: {ReadinessStatus}",
        response.status);

    return report.Status == HealthStatus.Healthy
        ? Ok(response)
        : StatusCode(503, response);
  }
}