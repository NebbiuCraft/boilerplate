using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Application.Common;
using Application.Common.Extensions;

namespace Infrastructure.Extensions;

public static class QueryableAsyncExtensions
{
  public static async Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
      this IQueryable<T> query,
      PaginatedQuery paginatedQuery,
      CancellationToken cancellationToken = default)
  {
    var totalCount = await query.CountAsync(cancellationToken);

    var items = await query
        .ApplySorting(paginatedQuery.SortBy ?? string.Empty, paginatedQuery.IsDescending)
        .ApplyPagination(paginatedQuery)
        .ToListAsync(cancellationToken);

    return new PaginatedResult<T>(
        items,
        totalCount,
        paginatedQuery.PageNumber,
        paginatedQuery.PageSize);
  }
}