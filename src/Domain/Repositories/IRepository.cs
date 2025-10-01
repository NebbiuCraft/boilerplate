using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Common;

namespace Domain.Repositories;

public interface IRepository<T> where T : class, IAggregateRoot
{
  Task<T> GetByIdAsync(int id, CancellationToken cancellationToken);
  Task AddAsync(T entity, CancellationToken cancellationToken);
  Task SaveAsync(CancellationToken cancellationToken);
  Task SoftDeleteAsync(T entity, CancellationToken cancellationToken);
  Task DeleteAsync(T entity, CancellationToken cancellationToken);

  /// <summary>
  /// Gets a queryable for advanced querying (use with caution - prefer specific query methods)
  /// </summary>
  IQueryable<T> GetQueryable();
}
