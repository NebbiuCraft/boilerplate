using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Infrastructure.Repositories;

/// <summary>
/// Base repository for legacy database entities (read-only)
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class LegacyRepository<T> where T : class
{
  protected readonly LegacyDbContext _context;
  protected readonly DbSet<T> _dbSet;

  public LegacyRepository(LegacyDbContext context)
  {
    _context = context;
    _dbSet = context.Set<T>();
  }

  /// <summary>
  /// Get all entities
  /// </summary>
  public virtual IQueryable<T> GetAll()
  {
    return _dbSet.AsNoTracking();
  }

  /// <summary>
  /// Get entity by ID
  /// </summary>
  public virtual async Task<T?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default)
  {
    return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
  }

  /// <summary>
  /// Get entities with custom filter
  /// </summary>
  public virtual IQueryable<T> Where(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
  {
    return _dbSet.AsNoTracking().Where(predicate);
  }

  /// <summary>
  /// Check if any entity matches the predicate
  /// </summary>
  public virtual async Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
  {
    return await _dbSet.AsNoTracking().AnyAsync(predicate, cancellationToken);
  }

  /// <summary>
  /// Count entities matching the predicate
  /// </summary>
  public virtual async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
  {
    if (predicate == null)
      return await _dbSet.AsNoTracking().CountAsync(cancellationToken);

    return await _dbSet.AsNoTracking().CountAsync(predicate, cancellationToken);
  }
}