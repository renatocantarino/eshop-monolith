using AppShared.Dtos;
using Catalog.Helpers;
using Catalog.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Endpoints;

public static class EndpointsCatalog
{
    public static void MapEndpointsCatalog(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products");

        group.MapGet("/", async (CatalogServices services) =>
        {
            var products = await services.GetAllAsync();
            var dtos = products.Select(p => p.ToDTO());
            return Results.Ok(dtos);
        })
   .WithName("GetAllProducts")
   .Produces<List<ProductDTO>>(StatusCodes.Status200OK)
   .CacheOutput(x => x.Tag("products"));

        group.MapGet("/{id}", async (int id, CatalogServices services) =>
        {
            var product = await services.GetByIdAsync(id);
            return product is not null ? Results.Ok(product.ToDTO()) : Results.NotFound();
        })
     .WithName("GetProductById")
     .Produces<ProductDTO>(StatusCodes.Status200OK)
     .CacheOutput(x => x.Tag("products"));

        group.MapPost("/", async (ProductDTO productDTO, CatalogServices services, IOutputCacheStore cache, CancellationToken ct) =>
        {
            var model = productDTO.ToModel();

            await services.CreateAsync(model);
            await cache.EvictByTagAsync("products", ct);
            return Results.Created($"/products/{model.Id}", model.ToDTO());
        })
       .WithName("CreateProduct")
       .Produces<ProductDTO>(StatusCodes.Status201Created);

        group.MapPut("/{id}", async (int id, ProductDTO updatedProductDTO, CatalogServices services, IOutputCacheStore cache, CancellationToken ct) =>
        {
            var persited = await services.GetByIdAsync(id);
            if (persited is null) return Results.NotFound();

            var updatedProduct = updatedProductDTO.ToModel();

            await services.UpdateAsync(persited, updatedProduct);
            await cache.EvictByTagAsync("products", ct);

            return Results.NoContent();
        })
        .WithName("UpdateProduct")
        .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/{id}", async (int id, CatalogServices services, IOutputCacheStore cache, CancellationToken ct) =>
        {
            var product = await services.GetByIdAsync(id);
            if (product is null) return Results.NotFound();

            await services.DeleteAsync(product);
            await cache.EvictByTagAsync("products", ct);
            return Results.NoContent();
        })
     .WithName("DeleteProduct")
     .Produces(StatusCodes.Status204NoContent);
    }
}