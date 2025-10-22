using System;
using FakePaymentService;

namespace Application.Dtos;


public class PaymentResultDto
{
  public PaymentStatus Status { get; set; }
  public string TransactionId { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public decimal ProcessedAmount { get; set; }
  public DateTime ProcessedAt { get; set; }
}