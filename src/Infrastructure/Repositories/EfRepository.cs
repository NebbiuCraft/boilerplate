using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Common;
using Domain.Repositories;
using Infrastructure.Persistence;
namespace Infrastructure.Repositories;

public abstract class EfRepository<T> : IRepository<T> where T : class, IAggregateRoot
{
    protected readonly AppDbContext _db;

    protected EfRepository(AppDbContext db)
    {
        _db = db;
    }

    public virtual async Task<T> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Set<T>().FindAsync(id, cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken)
    {
        await _db.Set<T>().AddAsync(entity, cancellationToken);
    }

    public virtual async Task SaveAsync(CancellationToken cancellationToken)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken)
    {
        _db.Set<T>().Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task SoftDeleteAsync(T entity, CancellationToken cancellationToken)
    {
        entity.Active = false;
        return SaveAsync(cancellationToken);
    }

    public virtual IQueryable<T> GetQueryable()
    {
        return _db.Set<T>().Where(x => x.Active);
    }
}