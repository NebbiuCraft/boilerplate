using AutoMapper;
using Application.Dtos;
using FakePaymentService;
using Domain.Entities;
using Application.Orders.Commands;

namespace Application.Mappings;

public class OrderMappingProfile : Profile
{
  public OrderMappingProfile()
  {
    CreateMap<Order, OrderDto>();
    CreateMap<OrderItem, OrderItemDto>();

    CreateMap<UpdateOrderCommand, Order>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.Items, opt => opt.Ignore());

    CreateMap<PaymentResult, PaymentResultDto>();
    CreateMap<PaymentResultDto, PaymentResult>();
  }
}
