using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Application.Common;
using Application.Dtos;
using FakePaymentService;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Orders.Commands;

public record ProcessOrderPaymentCommand(ProcessPaymentDto PaymentDto) : IRequest<PaymentResultDto>;

public class ProcessOrderPaymentHandler : IRequestHandler<ProcessOrderPaymentCommand, PaymentResultDto>
{
  private readonly IOrderRepository _orderRepository;
  private readonly IPaymentService _paymentService;
  private readonly IMapper _mapper;
  private readonly IDomainEventPublisher _domainEventPublisher;
  private readonly ILogger<ProcessOrderPaymentHandler> _logger;

  public ProcessOrderPaymentHandler(
      IOrderRepository orderRepository,
      IPaymentService paymentService,
      IMapper mapper,
      IDomainEventPublisher domainEventPublisher,
      ILogger<ProcessOrderPaymentHandler> logger)
  {
    _orderRepository = orderRepository;
    _paymentService = paymentService;
    _mapper = mapper;
    _domainEventPublisher = domainEventPublisher;
    _logger = logger;
  }

  public async Task<PaymentResultDto> Handle(ProcessOrderPaymentCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation(
        "Processing payment for order {OrderId} | Amount: {RequestedAmount} {Currency} | Payment Method: {PaymentMethod}",
        request.PaymentDto.OrderId,
        request.PaymentDto.Amount,
        request.PaymentDto.Currency,
        request.PaymentDto.PaymentMethod);

    var order = await _orderRepository.GetByIdAsync(request.PaymentDto.OrderId, cancellationToken);
    if (order == null)
    {
      _logger.LogWarning("Payment processing failed - Order {OrderId} not found", request.PaymentDto.OrderId);
      throw new OrderNotFoundException(request.PaymentDto.OrderId);
    }

    if (order.IsPaid)
    {
      _logger.LogWarning(
          "Duplicate payment attempt for order {OrderId} | Existing Transaction: {ExistingTransactionId} | Payment Date: {PaymentDate}",
          request.PaymentDto.OrderId,
          order.TransactionId,
          order.PaymentDate);

      throw new DuplicatePaymentException("Order is already paid", request.PaymentDto.OrderId)
        .AddContext("ExistingTransactionId", order.TransactionId)
        .AddContext("PaymentDate", order.PaymentDate);
    }

    // Calculate total amount if not set
    if (order.TotalAmount == 0)
    {
      _logger.LogDebug("Calculating total amount for order {OrderId}", order.Id);
      order.CalculateTotalAmount();
    }

    // Use provided amount or calculated total
    var amountToProcess = request.PaymentDto.Amount > 0 ? request.PaymentDto.Amount : order.TotalAmount;

    _logger.LogInformation(
        "Initiating payment processing | Order: {OrderId} | Customer: {CustomerEmail} | Final Amount: {FinalAmount} {Currency}",
        order.Id,
        order.CustomerEmail,
        amountToProcess,
        request.PaymentDto.Currency);

    // Raise payment initiation event
    order.InitiatePayment(amountToProcess, request.PaymentDto.Currency, request.PaymentDto.PaymentMethod);
    await _domainEventPublisher.PublishEventsAsync(order);

    var paymentRequest = new PaymentRequest
    {
      Amount = amountToProcess,
      Currency = request.PaymentDto.Currency,
      CustomerEmail = order.CustomerEmail,
      OrderReference = $"ORDER_{order.Id}",
      PaymentMethod = request.PaymentDto.PaymentMethod
    };

    var paymentResult = await _paymentService.ProcessPaymentAsync(paymentRequest, cancellationToken);

    // Update order based on payment result
    if (paymentResult.Status == PaymentStatus.Success)
    {
      _logger.LogInformation(
          "Payment successful | Order: {OrderId} | Transaction: {TransactionId} | Processed Amount: {ProcessedAmount}",
          order.Id,
          paymentResult.TransactionId,
          paymentResult.ProcessedAmount);

      order.MarkPaid(paymentResult.TransactionId, paymentResult.ProcessedAt);
      order.SetTotalAmount(paymentResult.ProcessedAmount);
      await _orderRepository.SaveAsync(cancellationToken);

      // Publish payment successful events
      await _domainEventPublisher.PublishEventsAsync(order);
    }
    else
    {
      _logger.LogWarning(
          "Payment failed | Order: {OrderId} | Status: {PaymentStatus} | Error: {ErrorMessage} | Attempted Amount: {AttemptedAmount}",
          order.Id,
          paymentResult.Status,
          paymentResult.Message,
          amountToProcess);

      // Record payment failure and publish events
      order.RecordPaymentFailure(amountToProcess, request.PaymentDto.Currency,
          request.PaymentDto.PaymentMethod, paymentResult.Message ?? "Payment processing failed");
      await _domainEventPublisher.PublishEventsAsync(order);
    }

    return _mapper.Map<PaymentResultDto>(paymentResult);
  }
}