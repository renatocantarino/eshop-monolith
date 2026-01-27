using AppShared.Dtos;
using Basket.Helpers;
using Basket.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Basket.Endpoints;

public static class EndpointsBasket
{
    public static void MapEndpointsBasket(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/carts");

        group.MapGet("/", async (BasketServices services) =>
        {
            var carts = await services.GetAllAsync();
            var dtos = carts.Select(c => c.ToDTO());
            return Results.Ok(dtos);
        })
        .WithName("GetAllCarts")
        .Produces<List<ShoppingCartDTO>>(StatusCodes.Status200OK);

        group.MapGet("/{id}", async (int id, BasketServices services) =>
        {
            var cart = await services.GetByIdAsync(id);
            return cart is not null ? Results.Ok(cart.ToDTO()) : Results.NotFound();
        })
        .WithName("GetCartById")
        .Produces<ShoppingCartDTO>(StatusCodes.Status200OK);

        group.MapGet("/user/{userName}", async (string userName, BasketServices services) =>
        {
            var cart = await services.GetByUserNameAsync(userName);
            return cart is not null ? Results.Ok(cart.ToDTO()) : Results.NotFound();
        })
        .WithName("GetCartByUserName")
        .Produces<ShoppingCartDTO>(StatusCodes.Status200OK);

        group.MapPost("/", async (ShoppingCartDTO cartDTO, BasketServices services) =>
        {
            var model = cartDTO.ToModel();
            await services.CreateAsync(model);
            return Results.Created($"/carts/{model.Id}", model.ToDTO());
        })
        .WithName("CreateCart")
        .Produces<ShoppingCartDTO>(StatusCodes.Status201Created);

        group.MapPut("/{id}", async (int id, ShoppingCartDTO updatedCartDTO, BasketServices services) =>
        {
            var persisted = await services.GetByIdAsync(id);
            if (persisted is null) return Results.NotFound();

            var updatedCart = updatedCartDTO.ToModel();
            await services.UpdateAsync(persisted, updatedCart);
            return Results.Ok(persisted.ToDTO());
        })
        .WithName("UpdateCart")
        .Produces<ShoppingCartDTO>(StatusCodes.Status200OK);

        group.MapDelete("/{id}", async (int id, BasketServices services) =>
        {
            var cart = await services.GetByIdAsync(id);
            if (cart is null) return Results.NotFound();

            await services.DeleteAsync(cart);
            return Results.NoContent();
        })
        .WithName("DeleteCart")
        .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/{cartId}/items", async (int cartId, ShoppingCartItemDTO itemDTO, BasketServices services) =>
        {
            var item = itemDTO.ToModel();
            await services.AddItemAsync(cartId, item);
            var cart = await services.GetByIdAsync(cartId);
            return Results.Ok(cart!.ToDTO());
        })
        .WithName("AddItemToCart")
        .Produces<ShoppingCartDTO>(StatusCodes.Status200OK);

        group.MapDelete("/{cartId}/items/{itemId}", async (int cartId, int itemId, BasketServices services) =>
        {
            await services.RemoveItemAsync(cartId, itemId);
            var cart = await services.GetByIdAsync(cartId);
            return Results.Ok(cart!.ToDTO());
        })
        .WithName("RemoveItemFromCart")
        .Produces<ShoppingCartDTO>(StatusCodes.Status200OK);

        group.MapDelete("/{cartId}/items", async (int cartId, BasketServices services) =>
        {
            await services.ClearAsync(cartId);
            var cart = await services.GetByIdAsync(cartId);
            return Results.Ok(cart!.ToDTO());
        })
        .WithName("ClearCart")
        .Produces<ShoppingCartDTO>(StatusCodes.Status200OK);
    }
}
