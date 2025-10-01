namespace Domain.Common;

public interface IEntity
{
  int Id { get; set; }
  bool Active { get; set; }
}
