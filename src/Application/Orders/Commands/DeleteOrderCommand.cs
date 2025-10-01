using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories;
using MediatR;
namespace Application.Orders.Commands;

public record DeleteOrderCommand(int OrderId) : IRequest<bool>;

public class DeleteOrderHandler : IRequestHandler<DeleteOrderCommand, bool>
{
    private readonly IOrderRepository _repo;

    public DeleteOrderHandler(IOrderRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _repo.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null) return false;
        await _repo.SoftDeleteAsync(order, cancellationToken);
        await _repo.SaveAsync(cancellationToken);
        return true;
    }
}
