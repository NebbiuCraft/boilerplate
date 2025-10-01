using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Application.Common;
using Application.Dtos;
using Application.Common.Extensions;
using Domain.Repositories;
using AutoMapper;

namespace Application.Orders.Queries;


public class GetOrdersQuery : PaginatedQuery<Domain.Entities.Order>, IRequest<PaginatedResult<OrderDto>>
{
  public string CustomerEmail { get; set; }
  public decimal? MinTotal { get; set; }
  public decimal? MaxTotal { get; set; }

  public override string[] GetValidSortFields()
  {
    return
    [
      nameof(Domain.Entities.Order.Id),
      nameof(Domain.Entities.Order.CustomerEmail),
      nameof(Domain.Entities.Order.TotalAmount),
      nameof(Domain.Entities.Order.IsPaid),
      nameof(Domain.Entities.Order.PaymentDate)
    ];
  }
}

public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, PaginatedResult<OrderDto>>
{
  private readonly IOrderRepository _orderRepository;
  private readonly IMapper _mapper;

  public GetOrdersHandler(IOrderRepository orderRepository, IMapper mapper)
  {
    _orderRepository = orderRepository;
    _mapper = mapper;
  }

  public Task<PaginatedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
  {
    // Validate sort field
    if (!request.IsValidSortField())
    {
      throw new ArgumentException($"Invalid sort field: {request.SortBy}");
    }

    // Start with base query
    var query = _orderRepository.GetQueryable();

    // Apply filtering
    if (!string.IsNullOrEmpty(request.CustomerEmail))
    {
      query = query.Where(o => o.CustomerEmail.Contains(request.CustomerEmail));
    }

    if (request.MinTotal.HasValue)
    {
      query = query.Where(o => o.TotalAmount >= request.MinTotal.Value);
    }

    if (request.MaxTotal.HasValue)
    {
      query = query.Where(o => o.TotalAmount <= request.MaxTotal.Value);
    }

    // Get paginated result
    var paginatedOrders = query.ToPaginatedResult(request);

    // Map to DTOs
    var orderDtos = _mapper.Map<List<OrderDto>>(paginatedOrders.Items);

    var result = new PaginatedResult<OrderDto>(
        orderDtos,
        paginatedOrders.TotalCount,
        paginatedOrders.PageNumber,
        paginatedOrders.PageSize);

    return Task.FromResult(result);
  }
}