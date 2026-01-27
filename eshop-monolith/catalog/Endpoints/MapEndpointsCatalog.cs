using AppShared.Dtos;
using Catalog.Helpers;
using Catalog.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

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
   .Produces<List<ProductDTO>>(StatusCodes.Status200OK);

        group.MapGet("/{id}", async (int id, CatalogServices services) =>
        {
            var product = await services.GetByIdAsync(id);
            return product is not null ? Results.Ok(product.ToDTO()) : Results.NotFound();
        })
     .WithName("GetProductById")
     .Produces<ProductDTO>(StatusCodes.Status200OK);

        app.MapPost("/", async (ProductDTO productDTO, CatalogServices services) =>
        {
            var model = productDTO.ToModel();

            await services.CreateAsync(model);
            return Results.Created($"/products/{model.Id}", model.ToDTO());
        })
       .WithName("CreateProduct")
       .Produces<ProductDTO>(StatusCodes.Status201Created);

        app.MapPut("/{id}", async (int id, ProductDTO updatedProductDTO, CatalogServices services) =>
        {
            var persited = await services.GetByIdAsync(id);
            if (persited is null) return Results.NotFound();

            var updatedProduct = updatedProductDTO.ToModel();

            await services.UpdateAsync(persited, updatedProduct);

            return Results.NoContent();
        })
        .WithName("UpdateProduct")
        .Produces(StatusCodes.Status204NoContent);

        app.MapDelete("/{id}", async (int id, CatalogServices services) =>
        {
            var product = await services.GetByIdAsync(id);
            if (product is null) return Results.NotFound();

            await services.DeleteAsync(product);
            return Results.NoContent();
        })
     .WithName("DeleteProduct")
     .Produces(StatusCodes.Status204NoContent);
    }
}