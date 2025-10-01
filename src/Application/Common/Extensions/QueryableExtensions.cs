using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Application.Common;

namespace Application.Common.Extensions;

public static class QueryableExtensions
{
  public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, PaginatedQuery paginatedQuery)
  {
    return query.Skip(paginatedQuery.Skip).Take(paginatedQuery.PageSize);
  }

  public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string sortBy, bool sortDescending = false)
  {
    if (string.IsNullOrEmpty(sortBy))
      return query;

    var parameter = Expression.Parameter(typeof(T), "x");
    var property = Expression.Property(parameter, sortBy);
    var lambda = Expression.Lambda(property, parameter);

    var methodName = sortDescending ? "OrderByDescending" : "OrderBy";
    var resultExpression = Expression.Call(
        typeof(Queryable),
        methodName,
        new[] { typeof(T), property.Type },
        query.Expression,
        Expression.Quote(lambda));

    return query.Provider.CreateQuery<T>(resultExpression);
  }

  public static PaginatedResult<T> ToPaginatedResult<T>(
      this IQueryable<T> query,
      PaginatedQuery paginatedQuery)
  {
    var totalCount = query.Count();

    var items = query
        .ApplySorting(paginatedQuery.SortBy ?? string.Empty, paginatedQuery.IsDescending)
        .ApplyPagination(paginatedQuery)
        .ToList();

    return new PaginatedResult<T>(
        items,
        totalCount,
        paginatedQuery.PageNumber,
        paginatedQuery.PageSize);
  }
}