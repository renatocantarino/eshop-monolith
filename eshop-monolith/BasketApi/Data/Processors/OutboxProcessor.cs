using AppShared.IntegrationEvents;
using AppShared.Messaging;
using BasketApi.Application.Mappers;
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
                        switch (message.Type)
                        {
                            case nameof(OrderCreatedEvent):
                                var eventData = JsonSerializer.Deserialize<OrderCreatedEvent>(message.Content);

                                if (eventData is null)
                                {
                                    logger.LogWarning("Mensagem {Id} resultou em conteúdo nulo após desserialização.", message.Id);
                                    continue; // Ou trate conforme sua lógica de erro
                                }

                                await eventBus.PublishAsync(options.Value.OrdersStreamName, eventData, stoppingToken);
                                break;

                            default:
                                throw new NotSupportedException($"Tipo de evento não suportado: {message.Type}");
                        }

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