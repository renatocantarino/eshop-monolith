using AppShared.Events;
using MassTransit;

namespace BasketApi.Application.EventHandler;

public class ProductDeletedEventHandler(IBasketServiceApp basketServiceApp,
    ILogger<ProductDeletedEventHandler> logger) : IConsumer<ProductDeletedEvent>
{
    public Task Consume(ConsumeContext<ProductDeletedEvent> context)
    {
        logger.LogInformation("Handling ProductDeletedEvent for ProductId: {ProductId}",
            context.Message.ProductId);

        return Task.CompletedTask;
    }
}