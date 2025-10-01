using System.Collections.Generic;

namespace Application.Dtos;

public class OrderDto
{
  public int Id { get; set; }
  public string CustomerEmail { get; set; } = string.Empty;
  public bool IsPaid { get; set; }
  public bool Active { get; set; }
  public ICollection<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
}