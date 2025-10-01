using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services;

public enum PaymentStatus
{
  Pending,
  Success,
  Failed,
  Cancelled
}

public class PaymentRequest
{
  public decimal Amount { get; set; }
  public string Currency { get; set; } = "USD";
  public string CustomerEmail { get; set; } = string.Empty;
  public string OrderReference { get; set; } = string.Empty;
  public string PaymentMethod { get; set; } = string.Empty;
}

public class PaymentResult
{
  public PaymentStatus Status { get; set; }
  public string TransactionId { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public decimal ProcessedAmount { get; set; }
  public DateTime ProcessedAt { get; set; }
}

public interface IPaymentService
{
  Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);
  Task<PaymentResult> RefundPaymentAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default);
  Task<PaymentResult> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default);
}