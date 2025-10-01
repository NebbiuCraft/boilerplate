using System;
using System.Collections.Generic;

namespace Domain.Exceptions;

public enum DomainPaymentStatus
{
  Unknown,
  Success,
  Failed,
  Cancelled
}

/// <summary>
/// Exception thrown when payment processing fails
/// </summary>
public class PaymentProcessingException : BusinessRuleException
{
  public PaymentProcessingException(string reason, DomainPaymentStatus status = default, string transactionId = null)
      : base($"Payment processing failed: {reason}", "PAYMENT_PROCESSING_FAILED")
  {
    AddContext("PaymentReason", reason);
    if (status != default)
      AddContext("PaymentStatus", status.ToString());
    if (!string.IsNullOrEmpty(transactionId))
      AddContext("TransactionId", transactionId);
  }
}

/// <summary>
/// Exception thrown when payment validation fails
/// </summary>
public class InvalidPaymentException : BusinessRuleException
{
  public InvalidPaymentException(string reason, decimal amount = 0, string currency = null)
      : base($"Invalid payment: {reason}", "INVALID_PAYMENT")
  {
    AddContext("ValidationReason", reason);
    if (amount > 0)
      AddContext("Amount", amount);
    if (!string.IsNullOrEmpty(currency))
      AddContext("Currency", currency);
  }
}

/// <summary>
/// Exception thrown when order payment state is invalid for requested operation
/// </summary>
public class InvalidPaymentStateException : BusinessRuleException
{
  public InvalidPaymentStateException(string operation, bool currentPaidStatus, string transactionId = null)
      : base($"Cannot perform '{operation}' on order with payment status: {(currentPaidStatus ? "Paid" : "Unpaid")}", "INVALID_PAYMENT_STATE")
  {
    AddContext("RequestedOperation", operation);
    AddContext("CurrentPaidStatus", currentPaidStatus);
    if (!string.IsNullOrEmpty(transactionId))
      AddContext("ExistingTransactionId", transactionId);
  }
}

/// <summary>
/// Exception thrown when payment service is unavailable or returns unexpected results
/// </summary>
public class PaymentServiceException : DomainException
{
  public PaymentServiceException(string reason, string serviceName = null, Exception innerException = null)
      : base($"Payment service error: {reason}", "PAYMENT_SERVICE_ERROR", null, innerException)
  {
    AddContext("ServiceReason", reason);
    if (!string.IsNullOrEmpty(serviceName))
      AddContext("ServiceName", serviceName);
  }
}

/// <summary>
/// Exception thrown when attempting to pay an already paid order
/// </summary>
public class DuplicatePaymentException : BusinessRuleException
{
  public DuplicatePaymentException(string message, int orderId)
      : base(message, "DUPLICATE_PAYMENT_ATTEMPT")
  {
    AddContext("OrderId", orderId);
  }
}