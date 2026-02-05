using AppShared.Events;
using MassTransit;

namespace BasketApi.Application.EventHandler
{
    public class ProductPriceChangeEventHandler(IBasketServiceApp basketServiceApp,
    ILogger<ProductDeletedEventHandler> logger) : IConsumer<ProductPriceChangeEvent>
    {
        public Task Consume(ConsumeContext<ProductPriceChangeEvent> context)
        {
            logger.LogInformation("Handling ProductPriceChangeEvent for ProductId: {ProductId},{NewPrice}",
             context.Message.ProductId, context.Message.NewPrice);

            return Task.CompletedTask;
        }
    }
}