using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace FakePaymentService.HealthChecks;

/// <summary>
/// Health check for the payment service
/// </summary>
public class PaymentServiceHealthCheck : IHealthCheck
{
  private readonly IPaymentService _paymentService;
  private readonly ILogger<PaymentServiceHealthCheck> _logger;

  public PaymentServiceHealthCheck(IPaymentService paymentService, ILogger<PaymentServiceHealthCheck> logger)
  {
    _paymentService = paymentService;
    _logger = logger;
  }

  public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    try
    {
      _logger.LogDebug("Starting payment service health check");

      var startTime = DateTime.UtcNow;

      // Test payment service with a health check call
      // Using a known test transaction ID that the service can handle
      var testResult = await _paymentService.GetPaymentStatusAsync("HEALTH_CHECK_TEST", cancellationToken);

      var duration = DateTime.UtcNow - startTime;

      _logger.LogDebug("Payment service health check completed successfully in {Duration}ms", duration.TotalMilliseconds);

      return HealthCheckResult.Healthy("Payment service is responsive", new Dictionary<string, object>
      {
        ["service"] = "FakePaymentService",
        ["responseTime"] = duration.TotalMilliseconds,
        ["testTransaction"] = "HEALTH_CHECK_TEST",
        ["status"] = testResult.Status.ToString()
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Payment service health check failed");

      return HealthCheckResult.Unhealthy("Payment service is not responsive", ex, new Dictionary<string, object>
      {
        ["service"] = "FakePaymentService",
        ["error"] = ex.Message,
        ["errorType"] = ex.GetType().Name
      });
    }
  }
}