using System.Reflection;
using Application.Common;
using Microsoft.Extensions.DependencyInjection;
namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(Assembly.GetExecutingAssembly());
        });

        // Register domain event publisher
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        return services;
    }


    /// <summary>
    /// Add infrastructure health checks
    /// </summary>
    public static IHealthChecksBuilder AddApplicationChecks(this IHealthChecksBuilder builder)
    {
        return builder.AddCheck<FakePaymentService.HealthChecks.PaymentServiceHealthCheck>("external_payment_service");

    }
}