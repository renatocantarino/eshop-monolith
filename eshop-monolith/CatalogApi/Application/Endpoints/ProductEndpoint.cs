using AppShared.Cqrs.Mediator;
using CatalogApi.Application.useCases;

namespace CatalogApi.Application.Endpoints;

public static class ProductEndpoint
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products");

        group.MapGet("/", GettAllRoute)
            .WithName("GetAllProducts");
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