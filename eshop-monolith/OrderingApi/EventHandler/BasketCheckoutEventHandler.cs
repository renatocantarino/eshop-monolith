using AppShared.Events;
using MassTransit;
using OrderingApi.Application;
using OrderingApi.Models;

namespace OrderingApi.EventHandler;

public class BasketCheckoutEventHandler(OrderService orderService,
    ILogger<BasketCheckoutEventHandler> logger) : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        logger.LogInformation("BasketCheckoutEvent received: {EventId}-{BasketId}-{UserName}-{TotalPrice}",
            context.Message.EventId, context.Message.BasketId, context.Message.UserName, context.Message.TotalPrice);

        var order = new Order
        {
            UserName = context.Message.UserName,
            TotalPrice = context.Message.TotalPrice,
            FirstName = context.Message.FirstName,
            LastName = context.Message.LastName,
            EmailAddress = context.Message.EmailAddress,
            AddressLine = context.Message.AddressLine
        };

        await orderService.CreateOrderAsync(order);
    }
}