using System;
using System.Collections.Generic;

namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when an order is not found
/// </summary>
public class OrderNotFoundException : EntityNotFoundException
{
  public OrderNotFoundException(int orderId)
      : base($"Order with ID {orderId} was not found", "ORDER_NOT_FOUND")
  {
    AddContext("OrderId", orderId);
  }
}

/// <summary>
/// Exception thrown when trying to perform invalid operations on an order
/// </summary>
public class InvalidOrderOperationException : BusinessRuleException
{
  public InvalidOrderOperationException(string operation, string reason, int? orderId = null)
      : base($"Cannot perform operation '{operation}': {reason}", "INVALID_ORDER_OPERATION")
  {
    AddContext("Operation", operation);
    AddContext("Reason", reason);
    if (orderId.HasValue)
      AddContext("OrderId", orderId.Value);
  }
}

/// <summary>
/// Exception thrown when order item validation fails
/// </summary>
public class InvalidOrderItemException : BusinessRuleException
{
  public InvalidOrderItemException(string productName, int quantity, string reason)
      : base($"Invalid order item: {reason}", "INVALID_ORDER_ITEM")
  {
    AddContext("ProductName", productName);
    AddContext("Quantity", quantity);
    AddContext("ValidationReason", reason);
  }
}

/// <summary>
/// Exception thrown when order total calculation fails
/// </summary>
public class OrderCalculationException : BusinessRuleException
{
  public OrderCalculationException(string reason, decimal? currentTotal = null, int? itemCount = null)
      : base($"Order calculation failed: {reason}", "ORDER_CALCULATION_ERROR")
  {
    AddContext("CalculationReason", reason);
    if (currentTotal.HasValue)
      AddContext("CurrentTotal", currentTotal.Value);
    if (itemCount.HasValue)
      AddContext("ItemCount", itemCount.Value);
  }
}