using AppShared.Dtos;
using OrderingApi.Application.Mappers;
using OrderingApi.Models;

namespace OrderingApi.Application.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders");

        // GET all
        group.MapGet("/", async (OrderService service) =>
        {
            var orders = await service.GetAllOrdersAsync();
            var orderDto = orders.Select(x => x.ToDto()).ToList();
            return Results.Ok(orderDto);
        })
        .WithName("GetAllOrders")
        .Produces<List<OrderResponse>>(StatusCodes.Status200OK);

        // GET Order by userName
        group.MapGet("/{userName}", async (string userName, OrderService service) =>
        {
            var orders = await service.GetOrdersByUserNameAsync(userName);
            if (orders is null || !orders.Any()) return Results.NotFound($"No orders found for user '{userName}'.");

            var orderDto = orders.Select(x => x.ToDto()).ToList();
            return Results.Ok(orderDto);
        })
        .WithName("GetOrderByUserName")
        .Produces<List<OrderResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST (Create)
        group.MapPost("/", async (Order order, OrderService service) =>
        {
            await service.CreateOrderAsync(order);
            var orderDto = order.ToDto();
            return Results.Created($"/orders/{orderDto.Id}", orderDto);
        })
        .WithName("CreateOrder")
        .Produces<OrderResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}