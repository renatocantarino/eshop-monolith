using AppShared.Cqrs.Mediator;
using AppShared.Dtos;
using CatalogApi.Application.useCases;
using CatalogApi.Models;

namespace CatalogApi.Application.Endpoints;

public static class ProductEndpoint
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products");

        group
            .MapGet("/", GettAllRoute)
            .WithName("GetAllProducts")
            .Produces<List<ProductResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id}", GetByIdRoute)
               .Produces<ProductResponse>(StatusCodes.Status200OK)
               .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetByIdRoute(int id, IMediator raptor, CancellationToken ct)
    {
        try
        {
            var query = new GetByIdQuery(id);
            var result = await raptor.ExecuteQueryAsync(query, ct);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> GettAllRoute(IMediator raptor, CancellationToken ct)
    {
        try
        {
            var query = new GetAllQuery();
            var result = await raptor.ExecuteQueryAsync(query, ct);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}