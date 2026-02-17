using AppShared.IntegrationEvents;
using AppShared.Messaging;
using BasketApi.Application.Models;
using BasketApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BasketApi.Data.Processors;

public class OutboxProcessor(IServiceProvider serviceProvider, IEventBus eventBus, ILogger<OutboxProcessor> logger, IOptions<RedisMessagingOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<BasketDbContext>();

                // 1. Pegamos apenas o necessário e limitamos o lote (Batching)
                var outboxMessages = await dbContext.OutboxMessages
                    .Where(m => m.ProcessedOn == null)
                    .OrderBy(m => m.OccuredOn) // Garante ordem cronológica
                    .Take(20)
                    .ToListAsync(stoppingToken);

                if (outboxMessages.Count == 0)
                {
                    await Task.Delay(5, stoppingToken);
                    continue;
                }

                foreach (var message in outboxMessages)
                {
                    try
                    {
                        var eventType = Type.GetType(message.Type);
                        if (eventType == null)
                        {
                            logger.LogWarning("Tipo de evento desconhecido: {Type}", message.Type);
                            continue;
                        }

                        var eventData = JsonSerializer.Deserialize(message.Content, eventType);

                        if (eventData is not BasketCheckout cart)
                        {
                            logger.LogWarning("O conteúdo da mensagem não pôde ser convertido para BasketCheckout.");
                            continue;
                        }

                        var orderCreatedEvent = new OrderCreatedEvent(
                            CustomerId: cart.UserName,
                            BasketId: cart.ShoppingCartId,
                            Items: cart.Items.Select(item => new OrderItemDto(
                                item.ProductId,
                                item.ProductName,
                                item.Quantity,
                                item.Price,
                                item.Color
                            )).ToList(),
                            TotalPrice: cart.TotalPrice,
                            FirstName: cart.FirstName,
                            LastName: cart.LastName,
                            EmailAddress: cart.EmailAddress,
                            AddressLine: cart.AddressLine
                        );

                        await eventBus.PublishAsync(options.Value.OrdersStreamName, eventData, stoppingToken);

                        message.ProcessedOn = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to process message {Id}", message.Id);
                    }
                }

                await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Critical error in Outbox Worker");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}