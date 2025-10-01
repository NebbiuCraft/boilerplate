namespace Application.Dtos;

public class OrderItemDto
{
  public int Id { get; set; }
  public string ProductName { get; set; } = string.Empty;
  public int Quantity { get; set; }
  public bool Active { get; set; }
}