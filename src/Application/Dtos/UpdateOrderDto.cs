namespace Application.Dtos;

public class UpdateOrderDto
{
  public string CustomerEmail { get; set; } = string.Empty;
  public bool IsPaid { get; set; }
}