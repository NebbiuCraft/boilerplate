using System;
using System.Linq;
using Application;
using Infrastructure;
using Infrastructure.Persistence;
using FakePaymentService;
using WebApi.Middleware;
using WebApi.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// Configure Serilog early
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting Boilerplate Order Management API");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog with structured logging
    builder.Host.ConfigureSerilog();

    // Add services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Boilerplate Order Management API",
            Version = "v1",
            Description = "A Clean Architecture implementation with DDD principles for order management",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "API Support",
                Email = "support@example.com"
            }
        });

        // Enable annotations
        options.EnableAnnotations();

        // Include XML comments
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);
    });

    // Add health checks
    builder.Services.AddHealthChecks()
        .AddInfrastructureHealthChecks()
        .AddApplicationChecks();

    // Register external services at composition root
    builder.Services.AddPaymentServices();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // Initialize database
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Log.Information("Applying database migrations...");
        // Apply pending migrations automatically
        dbContext.Database.Migrate();
        Log.Information("Database migrations completed");
    }

    // Configure middleware
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault());
        };
    });

    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        Log.Information("Development environment detected - enabling Swagger");
        app.UseSwagger();
        app.UseSwaggerUI(options =>
       {
           options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
       });
    }

    app.UseAuthorization();
    app.MapControllers();

    Log.Information("Boilerplate Order Management API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
