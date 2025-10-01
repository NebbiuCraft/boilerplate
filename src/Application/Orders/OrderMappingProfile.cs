using AutoMapper;
using Application.Dtos;
using Application.Services;
using Domain.Entities;

namespace Application.Mappings;

public class OrderMappingProfile : Profile
{
  public OrderMappingProfile()
  {
    CreateMap<Order, OrderDto>();
    CreateMap<OrderItem, OrderItemDto>();

    CreateMap<CreateOrderDto, Order>()
        .ConstructUsing(dto => new Order(dto.CustomerEmail));

    CreateMap<UpdateOrderDto, Order>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.Items, opt => opt.Ignore());

    CreateMap<PaymentResult, PaymentResultDto>();
    CreateMap<PaymentResultDto, PaymentResult>();
  }
}
