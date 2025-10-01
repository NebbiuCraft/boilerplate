using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WebApi.Middleware;

/// <summary>
/// Global exception handling middleware that converts domain exceptions to appropriate HTTP responses
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

  public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
  {
    _next = next;
    _logger = logger;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception exception)
    {
      await HandleExceptionAsync(context, exception);
    }
  }

  private async Task HandleExceptionAsync(HttpContext context, Exception exception)
  {
    var response = context.Response;
    response.ContentType = "application/json";

    var errorResponse = exception switch
    {
      EntityNotFoundException ex => CreateErrorResponse(
          HttpStatusCode.NotFound,
          ex.ErrorCode,
          ex.Message,
          ex.ErrorContext,
          ex),

      BusinessRuleException ex => CreateErrorResponse(
          HttpStatusCode.BadRequest,
          ex.ErrorCode,
          ex.Message,
          ex.ErrorContext,
          ex),

      DomainException ex => CreateErrorResponse(
          HttpStatusCode.UnprocessableEntity,
          ex.ErrorCode,
          ex.Message,
          ex.ErrorContext,
          ex),

      ArgumentNullException ex => CreateErrorResponse(
          HttpStatusCode.BadRequest,
          "MISSING_ARGUMENT",
          ex.Message,
          new Dictionary<string, object> { ["ParameterName"] = ex.ParamName ?? "Unknown" },
          ex),

      ArgumentException ex => CreateErrorResponse(
          HttpStatusCode.BadRequest,
          "INVALID_ARGUMENT",
          ex.Message,
          new Dictionary<string, object> { ["ParameterName"] = ex.ParamName ?? "Unknown" },
          ex),

      _ => CreateErrorResponse(
          HttpStatusCode.InternalServerError,
          "INTERNAL_SERVER_ERROR",
          "An unexpected error occurred",
          new Dictionary<string, object> { ["ExceptionType"] = exception.GetType().Name },
          exception)
    };

    response.StatusCode = (int)errorResponse.StatusCode;

    // Log the exception with appropriate level
    LogException(exception, errorResponse.StatusCode);

    var jsonResponse = JsonSerializer.Serialize(new
    {
      error = new
      {
        code = errorResponse.ErrorCode,
        message = errorResponse.Message,
        context = errorResponse.Context,
        timestamp = errorResponse.Timestamp,
        traceId = context.TraceIdentifier
      }
    }, new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });

    await response.WriteAsync(jsonResponse);
  }

  private void LogException(Exception exception, HttpStatusCode statusCode)
  {
    var logLevel = statusCode switch
    {
      HttpStatusCode.InternalServerError => LogLevel.Error,
      HttpStatusCode.BadRequest => LogLevel.Warning,
      HttpStatusCode.NotFound => LogLevel.Information,
      _ => LogLevel.Warning
    };

    if (exception is DomainException domainException)
    {
      _logger.Log(logLevel, exception,
        "Domain exception occurred: {ErrorCode} - {ErrorMessage} | Context: {@ErrorContext} | Details: {LogDetails}",
        domainException.ErrorCode,
        domainException.Message,
        domainException.ErrorContext,
        domainException.GetLogDetails());
    }
    else
    {
      _logger.Log(logLevel, exception,
        "Unhandled exception occurred: {ExceptionType} - {ErrorMessage} | StackTrace: {StackTrace}",
        exception.GetType().Name,
        exception.Message,
        exception.StackTrace);
    }
  }

  private static ErrorResponse CreateErrorResponse(
      HttpStatusCode statusCode,
      string errorCode,
      string message,
      Dictionary<string, object> context,
      Exception exception)
  {
    return new ErrorResponse
    {
      StatusCode = statusCode,
      ErrorCode = errorCode,
      Message = message,
      Context = context,
      Timestamp = DateTime.UtcNow
    };
  }

  private class ErrorResponse
  {
    public HttpStatusCode StatusCode { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Context { get; set; } = new();
    public DateTime Timestamp { get; set; }
  }
}