using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Common;

/// <summary>
/// Represents a paginated result with metadata
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public class PaginatedResult<T>
{
  /// <summary>
  /// The items for the current page
  /// </summary>
  public IReadOnlyList<T> Items { get; }

  /// <summary>
  /// Current page number (1-based)
  /// </summary>
  public int PageNumber { get; }

  /// <summary>
  /// Number of items per page
  /// </summary>
  public int PageSize { get; }

  /// <summary>
  /// Total number of items across all pages
  /// </summary>
  public int TotalCount { get; }

  /// <summary>
  /// Total number of pages
  /// </summary>
  public int TotalPages { get; }

  /// <summary>
  /// Whether there is a previous page
  /// </summary>
  public bool HasPrevious => PageNumber > 1;

  /// <summary>
  /// Whether there is a next page
  /// </summary>
  public bool HasNext => PageNumber < TotalPages;

  /// <summary>
  /// Field used for sorting
  /// </summary>
  public string? SortBy { get; }

  /// <summary>
  /// Sort direction
  /// </summary>
  public string SortOrder { get; }

  public PaginatedResult(
      IEnumerable<T> items,
      int pageNumber,
      int pageSize,
      int totalCount,
      string? sortBy = null,
      string sortOrder = "asc")
  {
    Items = items?.ToList().AsReadOnly() ?? new List<T>().AsReadOnly();
    PageNumber = pageNumber;
    PageSize = pageSize;
    TotalCount = totalCount;
    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    SortBy = sortBy;
    SortOrder = sortOrder;
  }

  /// <summary>
  /// Creates an empty result
  /// </summary>
  public static PaginatedResult<T> Empty(int pageNumber = 1, int pageSize = 10)
  {
    return new PaginatedResult<T>(
        Enumerable.Empty<T>(),
        pageNumber,
        pageSize,
        0);
  }

  /// <summary>
  /// Creates a result from a full list (in-memory pagination)
  /// </summary>
  public static PaginatedResult<T> Create(
      IEnumerable<T> source,
      int pageNumber,
      int pageSize,
      string? sortBy = null,
      string sortOrder = "asc")
  {
    var sourceList = source?.ToList() ?? new List<T>();
    var totalCount = sourceList.Count;
    var skip = (pageNumber - 1) * pageSize;
    var items = sourceList.Skip(skip).Take(pageSize);

    return new PaginatedResult<T>(items, pageNumber, pageSize, totalCount, sortBy, sortOrder);
  }
}