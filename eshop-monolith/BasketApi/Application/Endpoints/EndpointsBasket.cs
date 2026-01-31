using AppShared.Cqrs.Mediator;
using AppShared.Dtos;
using BasketApi.Application.useCases.commands;
using BasketApi.Application.useCases.queries;

namespace BasketApi.Endpoints;

public static class EndpointsBasket
{
    public static void MapEndpointsBasket(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("basket");

        // GET by userName
        group.MapGet("/{userName}", async (string userName, IMediator raptor) =>
        {
            var shoppingCart = await raptor.ExecuteQueryAsync(new GetByUserName(userName));

            if (shoppingCart is null)
            {
                return Results.NotFound($"Shopping cart for user '{userName}' not found.");
            }

            return Results.Ok(shoppingCart);
        })
        .WithName("GetBasket")
        .Produces<ShoppingCartResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST (Upsert)
        group.MapPost("/", async (ShoppingCartResponse shoppingCart, IMediator raptor) =>
        {
            var command = new UpdateCartCommand(shoppingCart);

            await raptor.ExecuteCommandAsync(command);
            return Results.Created("GetBasket", command.response);
        })
        .WithName("UpdateBasket")
        .Produces<ShoppingCartResponse>(StatusCodes.Status201Created);

        // POST Checkout
        group.MapPost("/checkout", async (BasketCheckoutResponse basketCheckout, IMediator raptor) =>
        {
            var command = new CheckoutBasketCommand(basketCheckout);

            await raptor.ExecuteCommandAsync(command);
            return Results.NoContent();
        })
        .WithName("CheckoutBasket")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest);

        // DELETE
        group.MapDelete("/{userName}", async (string userName, IMediator raptor) =>
        {
            var command = new DeleteBasketCommand(userName);

            await raptor.ExecuteCommandAsync(command);
            return Results.NoContent();
        })
        .WithName("DeleteBasket")
        .Produces(StatusCodes.Status204NoContent);
    }
}