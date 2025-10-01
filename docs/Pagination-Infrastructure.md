# Pagination Infrastructure

This document explains the pagination infrastructure that supports listing entities with pagination, sorting, and filtering capabilities.

## Base Classes

### PaginatedQuery

Base class for all paginated queries with built-in validation:

-   **PageNumber**: 1-based page number (minimum 1, auto-corrected)
-   **PageSize**: Items per page (1-100, auto-clamped)
-   **SortBy**: Field name to sort by (optional)
-   **SortOrder**: "asc" or "desc" (default: "asc")

### PaginatedQuery<TEntity>

Generic version that adds type-safe sorting:

-   **GetValidSortFields()**: Abstract method to define allowed sort fields
-   **IsValidSortField()**: Validates if the current SortBy field is allowed

### PaginatedResult<T>

Contains paginated results with metadata:

-   **Items**: The actual data items for the current page
-   **PageNumber**: Current page number
-   **PageSize**: Items per page
-   **TotalCount**: Total items across all pages
-   **TotalPages**: Calculated total number of pages
-   **HasPreviousPage**: Whether there's a previous page
-   **HasNextPage**: Whether there's a next page

## Extension Methods

### Application Layer Extensions (QueryableExtensions)

Synchronous query operations for use in Application layer:

-   **ApplyPagination()**: Applies Skip/Take based on page and page size
-   **ApplySorting()**: Applies ordering using LINQ expressions
-   **ToPaginatedResult()**: Converts IQueryable to PaginatedResult synchronously

### Infrastructure Layer Extensions (QueryableAsyncExtensions)

Async query operations for Entity Framework:

-   **ToPaginatedResultAsync()**: Converts IQueryable to PaginatedResult asynchronously using EF Core

## Repository Pattern Integration

### Updated IRepository Interface

```csharp
public interface IRepository<T> where T : Entity
{
    // ... existing methods
    IQueryable<T> GetQueryable(); // New method for pagination support
}
```

### EfRepository Implementation

```csharp
public virtual IQueryable<T> GetQueryable()
{
    return _db.Set<T>().Where(x => x.Active); // Excludes soft-deleted entities
}
```

## Usage Example

### 1. Create Query Class

```csharp
public class GetOrdersQuery : PaginatedQuery<Order>, IRequest<PaginatedResult<OrderDto>>
{
    public string CustomerEmail { get; set; }
    public decimal? MinTotal { get; set; }
    public decimal? MaxTotal { get; set; }

    public override string[] GetValidSortFields()
    {
        return new[]
        {
            nameof(Order.Id),
            nameof(Order.CustomerEmail),
            nameof(Order.TotalAmount),
            nameof(Order.IsPaid),
            nameof(Order.PaymentDate)
        };
    }
}
```

### 2. Create Handler

```csharp
public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, PaginatedResult<OrderDto>>
{
    public Task<PaginatedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        // Validate sort field
        if (!request.IsValidSortField())
        {
            throw new ArgumentException($"Invalid sort field: {request.SortBy}");
        }

        // Start with base query
        var query = _orderRepository.GetQueryable();

        // Apply filtering
        if (!string.IsNullOrEmpty(request.CustomerEmail))
        {
            query = query.Where(o => o.CustomerEmail.Contains(request.CustomerEmail));
        }

        // Get paginated result
        var paginatedOrders = query.ToPaginatedResult(request);

        // Map to DTOs
        var orderDtos = _mapper.Map<List<OrderDto>>(paginatedOrders.Items);

        return Task.FromResult(new PaginatedResult<OrderDto>(
            orderDtos,
            paginatedOrders.TotalCount,
            paginatedOrders.PageNumber,
            paginatedOrders.PageSize));
    }
}
```

### 3. Controller Action

```csharp
[HttpGet]
public async Task<IActionResult> GetOrders([FromQuery] GetOrdersQuery query)
{
    var result = await _mediator.Send(query);
    return Ok(result);
}
```

## API Usage

### Request Examples

```
GET /api/order?pageNumber=1&pageSize=10
GET /api/order?pageNumber=2&pageSize=5&sortBy=TotalAmount&sortOrder=desc
GET /api/order?customerEmail=john&minTotal=100&sortBy=CustomerEmail
```

### Response Format

```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 95,
  "totalPages": 10,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

## Key Features

1. **Type Safety**: Generic constraints ensure only valid sort fields
2. **Validation**: Built-in validation for page numbers and sizes
3. **Clean Architecture**: Respects layer boundaries (no EF in Application)
4. **Extensible**: Easy to add custom filtering per entity
5. **Performance**: Uses IQueryable for efficient database queries
6. **Consistent**: Same pattern for all list operations

## Adding New Paginated Queries

1. Create query class inheriting from `PaginatedQuery<TEntity>`
2. Implement `GetValidSortFields()` with allowed sort properties
3. Add custom filter properties as needed
4. Create handler using the repository's `GetQueryable()` method
5. Apply filters, then call `.ToPaginatedResult()`
6. Map to DTOs and return
