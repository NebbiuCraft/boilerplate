using System;
using FakePaymentService;

namespace Application.Dtos;

public class ProcessPaymentDto
{
  public int OrderId { get; set; }
  public decimal Amount { get; set; }
  public string Currency { get; set; } = "USD";
  public string PaymentMethod { get; set; } = string.Empty;
}

public class PaymentResultDto
{
  public PaymentStatus Status { get; set; }
  public string TransactionId { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public decimal ProcessedAmount { get; set; }
  public DateTime ProcessedAt { get; set; }
}