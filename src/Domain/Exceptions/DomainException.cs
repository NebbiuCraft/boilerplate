using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Exceptions;

/// <summary>
/// Base domain exception with rich error context for logging and debugging
/// </summary>
public abstract class DomainException : Exception
{
  public string ErrorCode { get; }
  public Dictionary<string, object> ErrorContext { get; }
  public DateTime OccurredAt { get; }

  protected DomainException(
      string message,
      string errorCode,
      Dictionary<string, object> errorContext = null,
      Exception innerException = null)
      : base(message, innerException)
  {
    ErrorCode = errorCode;
    ErrorContext = errorContext ?? new Dictionary<string, object>();
    OccurredAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Add context information to the exception
  /// </summary>
  public DomainException AddContext(string key, object value)
  {
    ErrorContext[key] = value;
    return this;
  }

  /// <summary>
  /// Get formatted error information for logging
  /// </summary>
  public virtual string GetLogDetails()
  {
    var context = string.Join(", ", ErrorContext.Select(kvp => $"{kvp.Key}={kvp.Value}"));
    return $"[{ErrorCode}] {Message} | Context: {context} | OccurredAt: {OccurredAt:yyyy-MM-dd HH:mm:ss} UTC";
  }
}

/// <summary>
/// Exception for business rule violations
/// </summary>
public abstract class BusinessRuleException : DomainException
{
  protected BusinessRuleException(
      string message,
      string errorCode,
      Dictionary<string, object> errorContext = null,
      Exception innerException = null)
      : base(message, errorCode, errorContext, innerException)
  {
  }
}

/// <summary>
/// Exception for entity not found scenarios
/// </summary>
public abstract class EntityNotFoundException : DomainException
{
  protected EntityNotFoundException(
      string message,
      string errorCode,
      Dictionary<string, object> errorContext = null,
      Exception innerException = null)
      : base(message, errorCode, errorContext, innerException)
  {
  }
}