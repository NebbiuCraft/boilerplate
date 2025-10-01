using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Common;
using Domain.Events;
using Domain.Exceptions;
namespace Domain.Entities;

public class Order : Entity, IAggregateRoot
{
    public string CustomerEmail { get; private set; }
    public bool IsPaid { get; private set; }
    public string TransactionId { get; private set; } = string.Empty;
    public DateTime? PaymentDate { get; private set; }
    public decimal TotalAmount { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // Parameterless constructor for EF Core
    private Order() { }

    public Order(string customerEmail)
    {
        CustomerEmail = customerEmail;

        // Raise domain event for order creation
        RaiseDomainEvent(new OrderCreatedEvent(Id, CustomerEmail, 0));
    }

    public void AddItem(string productName, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new InvalidOrderItemException(productName ?? "null", quantity, "Product name cannot be empty");

        if (quantity <= 0)
            throw new InvalidOrderItemException(productName, quantity, "Quantity must be positive");

        _items.Add(new OrderItem(productName, quantity));

        // Raise domain event for item addition
        RaiseDomainEvent(new OrderItemAddedEvent(Id, productName, quantity, CustomerEmail));
    }

    public void MarkPaid()
    {
        IsPaid = true;
    }

    public void MarkPaid(string transactionId, DateTime paymentDate)
    {
        if (IsPaid)
            throw new InvalidPaymentStateException("mark paid", true, TransactionId);

        if (string.IsNullOrWhiteSpace(transactionId))
            throw new InvalidPaymentException("Transaction ID cannot be empty");

        IsPaid = true;
        TransactionId = transactionId;
        PaymentDate = paymentDate;

        // Raise domain event for successful payment
        RaiseDomainEvent(new PaymentSuccessfulEvent(Id, transactionId, TotalAmount, paymentDate, CustomerEmail));
    }

    public decimal CalculateTotalAmount()
    {
        if (!_items.Any())
            throw new OrderCalculationException("Cannot calculate total for order with no items", 0, 0);

        try
        {
            // Simple calculation - in real scenario, this would include pricing, taxes, etc.
            TotalAmount = _items.Sum(item => item.Quantity * 10.0m); // Assuming $10 per item

            return TotalAmount;
        }
        catch (Exception ex)
        {
            throw new OrderCalculationException("Error occurred during total calculation", TotalAmount, _items.Count)
                .AddContext("InnerException", ex.Message);
        }
    }

    public void SetTotalAmount(decimal amount)
    {
        if (amount < 0)
            throw new InvalidOrderOperationException("set total amount", "Total amount cannot be negative", Id)
                .AddContext("AttemptedAmount", amount);

        TotalAmount = amount;
    }

    /// <summary>
    /// Raises a domain event indicating payment initiation for this order
    /// </summary>
    public void InitiatePayment(decimal amount, string currency, string paymentMethod)
    {
        RaiseDomainEvent(new PaymentInitiatedEvent(Id, amount, currency, CustomerEmail, paymentMethod));
    }

    /// <summary>
    /// Raises a domain event indicating payment failure for this order
    /// </summary>
    public void RecordPaymentFailure(decimal attemptedAmount, string currency, string paymentMethod, string failureReason)
    {
        RaiseDomainEvent(new PaymentFailedEvent(Id, attemptedAmount, currency, CustomerEmail, failureReason, paymentMethod));
    }
}