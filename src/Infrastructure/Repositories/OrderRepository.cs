using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OrderRepository : EfRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext db) : base(db)
    {
    }
    
    public override async Task<Order> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Orders
                        .Include(b => b.Items)
                        .FirstOrDefaultAsync(b => b.Id == id, cancellationToken: cancellationToken);
    }
}