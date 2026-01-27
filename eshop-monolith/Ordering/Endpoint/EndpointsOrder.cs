using AppShared.Dtos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Ordering.Endpoint;

public static class EndpointsOrder
{
    public static void MapEndpointsOrder(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders");

        group.MapGet("/", async (OrderServices services) =>
        {
            var orders = await services.GetAllAsync();
            var dtos = orders.Select(o => o.ToDTO());
            return Results.Ok(dtos);
        })
        .WithName("GetAllOrders")
        .Produces<List<OrderDTO>>(StatusCodes.Status200OK);

        group.MapGet("/{id}", async (int id, OrderServices services) =>
        {
            var order = await services.GetByIdAsync(id);
            return order is not null ? Results.Ok(order.ToDTO()) : Results.NotFound();
        })
        .WithName("GetOrderById")
        .Produces<OrderDTO>(StatusCodes.Status200OK);

        group.MapGet("/user/{userName}", async (string userName, OrderServices services) =>
        {
            var orders = await services.GetByUserNameAsync(userName);
            return orders.Any() ? Results.Ok(orders.Select(o => o.ToDTO())) : Results.NotFound();
        })
        .WithName("GetOrdersByUserName")
        .Produces<List<OrderDTO>>(StatusCodes.Status200OK);

        group.MapPost("/", async (OrderDTO orderDTO, OrderServices services) =>
        {
            var model = orderDTO.ToModel();
            await services.CreateAsync(model);
            return Results.Created($"/orders/{model.Id}", model.ToDTO());
        })
        .WithName("CreateOrder")
        .Produces<OrderDTO>(StatusCodes.Status201Created);

        group.MapPut("/{id}", async (int id, OrderDTO updatedOrderDTO, OrderServices services) =>
        {
            var persisted = await services.GetByIdAsync(id);
            if (persisted is null) return Results.NotFound();

            var updatedOrder = updatedOrderDTO.ToModel();
            await services.UpdateAsync(persisted, updatedOrder);
            return Results.Ok(persisted.ToDTO());
        })
        .WithName("UpdateOrder")
        .Produces<OrderDTO>(StatusCodes.Status200OK);

        group.MapDelete("/{id}", async (int id, OrderServices services) =>
        {
            var order = await services.GetByIdAsync(id);
            if (order is null) return Results.NotFound();

            await services.DeleteAsync(order);
            return Results.NoContent();
        })
        .WithName("DeleteOrder")
        .Produces(StatusCodes.Status204NoContent);
    }
}

/*

 # Basket
dotnet ef migrations add InitialBasketMigration -p basket -s ApiServices --context BasketDbContext

# Order
dotnet ef migrations add InitialOrderMigration -p Ordering -s ApiServices --context OrderDbContext

# Catalog
dotnet ef migrations add InitialCatalogMigration -p catalog -s ApiServices --context CatalogDbContext
 */