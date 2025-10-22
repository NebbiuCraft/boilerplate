# Swagger Documentation Enhancements

This document explains the comprehensive Swagger/OpenAPI documentation that has been added to the Boilerplate Order Management API.

## Overview

The API now includes full Swagger/OpenAPI 3.0.4 documentation with detailed descriptions, examples, and response schemas for all endpoints.

## Enhanced Features

### 📋 **API Information**

-   **Title**: Boilerplate Order Management API
-   **Version**: v1
-   **Description**: A Clean Architecture implementation with DDD principles for order management
-   **Contact**: API Support (support@example.com)

### 🏷️ **Controller Tags**

-   **Orders**: Order management including CRUD operations and payment processing
-   **Health**: Application health monitoring and diagnostics
-   **Payments**: Payment processing operations

### 📚 **Endpoint Documentation**

#### **Order Management Endpoints**

1. **POST /api/Order** - Create Order

    - **Summary**: Create a new order
    - **Description**: Creates a new order for the specified customer email address
    - **Request Body**: CreateOrderDto with customer email
    - **Responses**:
        - 200: Order created successfully (returns order ID)
        - 400: Invalid request data

2. **GET /api/Order/{id}** - Get Order by ID

    - **Summary**: Get order by ID
    - **Description**: Retrieves a specific order by its unique identifier
    - **Parameters**: id (path parameter)
    - **Responses**:
        - 200: Order found and returned (OrderDto)
        - 404: Order not found

3. **GET /api/Order** - List Orders with Pagination

    - **Summary**: Get orders with pagination
    - **Description**: Retrieves a paginated list of orders with optional filtering by customer email and amount range, plus sorting capabilities
    - **Query Parameters**:
        - CustomerEmail: Filter by customer email (string)
        - MinTotal: Minimum order amount (decimal)
        - MaxTotal: Maximum order amount (decimal)
        - PageNumber: Page number (1-based, min: 1)
        - PageSize: Items per page (1-100)
        - SortBy: Field to sort by (string)
        - SortOrder: Sort direction - "asc" or "desc"
    - **Responses**:
        - 200: Orders retrieved successfully (PaginatedResult<OrderDto>)
        - 400: Invalid query parameters

4. **PUT /api/Order/{id}** - Update Order

    - **Summary**: Update an order
    - **Description**: Updates an existing order with new details
    - **Parameters**: id (path), UpdateOrderDto (body)
    - **Responses**:
        - 204: Order updated successfully
        - 404: Order not found
        - 400: Invalid request data

5. **DELETE /api/Order/{id}** - Delete Order
    - **Summary**: Delete an order
    - **Description**: Soft deletes an order (sets Active = false). The order is not physically removed from the database
    - **Parameters**: id (path parameter)
    - **Responses**:
        - 204: Order deleted successfully
        - 404: Order not found

#### **Payment Processing Endpoints**

1. **POST /api/Order/{id}/payment** - Process Order Payment
    - **Summary**: Process order payment
    - **Description**: Processes payment for a specific order. Amounts over $10,000 will fail for demonstration purposes
    - **Parameters**: id (path), ProcessPaymentDto (body)
    - **Responses**:
        - 200: Payment processed successfully (PaymentResult)
        - 400: Payment failed or invalid request (PaymentResult)
        - 404: Order not found

#### **Health Check Endpoints**

1. **GET /api/Health** - Comprehensive Health Status

    - **Summary**: Get application health status
    - **Description**: Returns comprehensive health information including database connectivity, external service status, and overall system health
    - **Responses**:
        - 200: Health status retrieved successfully
        - 503: One or more health checks failed

2. **GET /api/Health/live** - Liveness Probe

    - **Summary**: Liveness probe
    - **Description**: Simple endpoint to verify the application is running. Used by container orchestrators like Kubernetes for liveness probes
    - **Responses**:
        - 200: Application is alive and responding

3. **GET /api/Health/ready** - Readiness Probe
    - **Summary**: Readiness probe
    - **Description**: Endpoint that checks if the application is ready to serve requests by verifying all dependencies are healthy. Used by container orchestrators for readiness probes
    - **Responses**:
        - 200: Application is ready to serve requests
        - 503: Application is not ready (dependencies unavailable)

## Configuration

### **Swagger Generation Setup**

```xml
<Target Name="PostBuild" AfterTargets="PostBuildEvent">
  <Exec Command="dotnet tool restore"
        Condition="'$(ASPNETCORE_ENVIRONMENT)' == 'Localhost'"/>
  <Exec Command="dotnet swagger tofile --output swagger.json $(OutputPath)$(AssemblyName).dll v1"
        Condition="'$(ASPNETCORE_ENVIRONMENT)' == 'Localhost'"/>
</Target>
```

### **Program.cs Configuration**

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Boilerplate Order Management API",
        Version = "v1",
        Description = "A Clean Architecture implementation with DDD principles for order management",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com"
        }
    });

    // Enable annotations
    options.EnableAnnotations();

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});
```

## Packages Added

-   **Swashbuckle.AspNetCore.Annotations** (v9.0.4): Enables advanced Swagger annotations

## Benefits

✅ **Comprehensive Documentation**: Every endpoint is fully documented with descriptions, parameters, and response types  
✅ **Type Safety**: All DTOs and response types are properly referenced  
✅ **Developer Experience**: Clear operation IDs, summaries, and descriptions  
✅ **API Discoverability**: Proper tagging and categorization  
✅ **Client Generation**: Rich metadata for generating client SDKs  
✅ **Testing Support**: Complete parameter documentation for API testing tools  
✅ **Container Orchestration**: Proper health check endpoint documentation

## Usage

1. **Development**: Access Swagger UI at `/swagger` when running locally
2. **Documentation**: Generated `swagger.json` can be imported into:
    - Postman collections
    - API documentation tools
    - Client SDK generators
    - API management platforms

## Auto-Generation

The `swagger.json` file is automatically generated when building with `ASPNETCORE_ENVIRONMENT=Localhost`, ensuring documentation stays in sync with code changes.

The enhanced documentation provides a professional, comprehensive API specification that improves developer experience and supports automated tooling workflows.
