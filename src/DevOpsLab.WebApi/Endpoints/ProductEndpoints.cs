using DevOpsLab.Application.Products;

namespace DevOpsLab.WebApi.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", async (
            string? search,
            int? page,
            int? pageSize,
            ProductService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ListAsync(search, page ?? 1, pageSize ?? 20, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("ListProducts");

        group.MapGet("/{id:guid}", async (
            Guid id,
            ProductService service,
            CancellationToken cancellationToken) =>
        {
            var product = await service.GetAsync(id, cancellationToken);
            return Results.Ok(product);
        })
        .WithName("GetProductById");

        group.MapPost("/", async (
            CreateProductRequest request,
            ProductService service,
            CancellationToken cancellationToken) =>
        {
            var created = await service.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/products/{created.Id}", created);
        })
        .WithName("CreateProduct");

        group.MapPost("/{id:guid}/price", async (
            Guid id,
            ChangePriceRequest request,
            ProductService service,
            CancellationToken cancellationToken) =>
        {
            var updated = await service.ChangePriceAsync(id, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("ChangeProductPrice");

        group.MapPost("/{id:guid}/reserve", async (
            Guid id,
            ReserveStockRequest request,
            ProductService service,
            CancellationToken cancellationToken) =>
        {
            var updated = await service.ReserveStockAsync(id, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("ReserveProductStock");

        return endpoints;
    }
}
