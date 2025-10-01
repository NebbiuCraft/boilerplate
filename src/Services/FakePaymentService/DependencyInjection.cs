using Microsoft.Extensions.DependencyInjection;

namespace FakePaymentService;

/// <summary>
/// Dependency injection extensions for FakePaymentService
/// </summary>
public static class DependencyInjection
{
  /// <summary>
  /// Add FakePaymentService and related services
  /// </summary>
  public static IServiceCollection AddPaymentServices(this IServiceCollection services)
  {
    services.AddSingleton<IPaymentService, FakePaymentService>();
    return services;
  }
}