using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace WebApi.Configuration;

/// <summary>
/// Extension methods for configuring Serilog throughout the application
/// </summary>
public static class LoggingConfiguration
{
  /// <summary>
  /// Configure Serilog with structured logging best practices
  /// </summary>
  public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder)
  {
    return hostBuilder.UseSerilog((context, services, configuration) =>
    {
      configuration
              .ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .Enrich.WithProperty("Application", "Boilerplate.OrderManagement")
              .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
              .WriteTo.Console(
                  outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}")
              .WriteTo.File(
                  path: $"logs/boilerplate-{context.HostingEnvironment.EnvironmentName}-.log",
                  rollingInterval: RollingInterval.Day,
                  retainedFileCountLimit: 30,
                  shared: true,
                  restrictedToMinimumLevel: LogEventLevel.Information);

      // Enable debug logging in development
      if (context.HostingEnvironment.IsDevelopment())
      {
        configuration.MinimumLevel.Debug();
      }
    });
  }
}