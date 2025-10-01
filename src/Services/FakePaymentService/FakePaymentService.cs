using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Services;
using Microsoft.Extensions.Logging;

namespace FakePaymentService;

public class FakePaymentService : IPaymentService
{
  private readonly ILogger<FakePaymentService> _logger;
  private static readonly Random _random = new();

  public FakePaymentService(ILogger<FakePaymentService> logger)
  {
    _logger = logger;
  }

  public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("Processing payment for order {OrderReference} with amount {Amount} {Currency}",
        request.OrderReference, request.Amount, request.Currency);

    // Simulate processing delay
    await Task.Delay(1000, cancellationToken);

    // Simulate different payment outcomes based on amount
    var result = new PaymentResult
    {
      TransactionId = $"TXN_{Guid.NewGuid():N}",
      ProcessedAmount = request.Amount,
      ProcessedAt = DateTime.UtcNow
    };

    // Fake payment logic - fail if amount is negative or > 10000
    if (request.Amount <= 0)
    {
      result.Status = PaymentStatus.Failed;
      result.Message = "Invalid payment amount";
      _logger.LogWarning("Payment failed for order {OrderReference}: Invalid amount", request.OrderReference);
    }
    else if (request.Amount > 10000)
    {
      result.Status = PaymentStatus.Failed;
      result.Message = "Amount exceeds transaction limit";
      _logger.LogWarning("Payment failed for order {OrderReference}: Amount exceeds limit", request.OrderReference);
    }
    else if (request.CustomerEmail.Contains("fail", StringComparison.OrdinalIgnoreCase))
    {
      // Simulate failure for testing - any email containing "fail"
      result.Status = PaymentStatus.Failed;
      result.Message = "Payment method declined";
      _logger.LogWarning("Payment failed for order {OrderReference}: Simulated failure", request.OrderReference);
    }
    else
    {
      result.Status = PaymentStatus.Success;
      result.Message = "Payment processed successfully";
      _logger.LogInformation("Payment successful for order {OrderReference}, transaction ID: {TransactionId}",
          request.OrderReference, result.TransactionId);
    }

    return result;
  }

  public async Task<PaymentResult> RefundPaymentAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("Processing refund for transaction {TransactionId} with amount {Amount}",
        transactionId, amount);

    await Task.Delay(500, cancellationToken);

    return new PaymentResult
    {
      Status = PaymentStatus.Success,
      TransactionId = $"REF_{Guid.NewGuid():N}",
      Message = "Refund processed successfully",
      ProcessedAmount = amount,
      ProcessedAt = DateTime.UtcNow
    };
  }

  public async Task<PaymentResult> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("Checking payment status for transaction {TransactionId}", transactionId);

    await Task.Delay(200, cancellationToken);

    // Simulate status check - assume all transactions are successful for simplicity
    return new PaymentResult
    {
      Status = PaymentStatus.Success,
      TransactionId = transactionId,
      Message = "Transaction found and completed",
      ProcessedAt = DateTime.UtcNow.AddMinutes(-5) // Simulate it was processed 5 minutes ago
    };
  }
}