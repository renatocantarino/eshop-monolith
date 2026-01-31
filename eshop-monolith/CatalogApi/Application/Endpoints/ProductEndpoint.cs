using AppShared.Cqrs.Mediator;
using AppShared.Dtos;
using CatalogApi.Application.useCases;

namespace CatalogApi.Application.Endpoints;

public static class ProductEndpoint
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/products")
            .MapGet("/", GettAllRoute)
            .WithName("GetAllProducts")
            .Produces<List<ProductResponse>>(StatusCodes.Status200OK);
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