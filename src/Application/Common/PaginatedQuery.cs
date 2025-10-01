using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Common;

/// <summary>
/// Base class for paginated queries
/// </summary>
public abstract class PaginatedQuery
{
  private int _pageNumber = 1;
  private int _pageSize = 10;

  /// <summary>
  /// Page number (1-based)
  /// </summary>
  [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
  public int PageNumber
  {
    get => _pageNumber;
    set => _pageNumber = value < 1 ? 1 : value;
  }

  /// <summary>
  /// Number of items per page
  /// </summary>
  [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
  public int PageSize
  {
    get => _pageSize;
    set => _pageSize = Math.Clamp(value, 1, 100);
  }

  /// <summary>
  /// Field to sort by
  /// </summary>
  public string? SortBy { get; set; }

  /// <summary>
  /// Sort direction (asc/desc)
  /// </summary>
  public string SortOrder { get; set; } = "asc";

  /// <summary>
  /// Gets the number of items to skip
  /// </summary>
  public int Skip => (PageNumber - 1) * PageSize;

  /// <summary>
  /// Validates if sort order is valid
  /// </summary>
  public bool IsDescending => SortOrder?.ToLowerInvariant() == "desc";
}

/// <summary>
/// Generic paginated query with type-safe sorting
/// </summary>
/// <typeparam name="TEntity">Entity type for sorting</typeparam>
public abstract class PaginatedQuery<TEntity> : PaginatedQuery
{
  /// <summary>
  /// Available sort fields for this entity
  /// </summary>
  public abstract string[] GetValidSortFields();

  /// <summary>
  /// Validates if the sort field is allowed
  /// </summary>
  public bool IsValidSortField()
  {
    if (string.IsNullOrEmpty(SortBy))
      return true;

    var validFields = GetValidSortFields();
    return Array.Exists(validFields, field =>
        string.Equals(field, SortBy, StringComparison.OrdinalIgnoreCase));
  }
}