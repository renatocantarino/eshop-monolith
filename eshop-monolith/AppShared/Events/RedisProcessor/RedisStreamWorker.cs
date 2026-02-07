using AppShared.Cqrs.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace AppShared.Events.RedisProcessor;

public class RedisStreamWorker : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RedisStreamWorker> _logger;
    private const string StreamName = "order_stream";
    private const string GroupName = "infra_group";

    public RedisStreamWorker(
        IConnectionMultiplexer redis,
        IServiceScopeFactory scopeFactory,
        ILogger<RedisStreamWorker> logger)
    {
        _redis = redis;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var db = _redis.GetDatabase();

        // Garante que o grupo de consumidores existe
        try
        {
            await db.StreamCreateConsumerGroupAsync(StreamName, GroupName, "0-0");
        }
        catch (RedisServerException ex) when (ex.Message.Contains("already exists"))
        {
            // Grupo já existe, podemos ignorar o erro
        }

        _logger.LogInformation("Worker iniciado: Monitorando Stream {Stream} no Grupo {Group}", StreamName, GroupName);

        while (!stoppingToken.IsCancellationRequested)
        {
            // 1. Busca mensagens pendentes ou novas
            // ">" busca mensagens que ainda não foram entregues para nenhum consumidor do grupo
            var messages = await db.StreamReadGroupAsync(StreamName, GroupName, "worker_instancia_1", ">", count: 1);

            if (messages.Length == 0)
            {
                await Task.Delay(500, stoppingToken);
                continue;
            }

            foreach (var msg in messages)
            {
                try
                {
                    // 2. Criar escopo para resolver o IMediator (RaptorMediator)
                    using var scope = _scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    var json = msg.Values.FirstOrDefault(v => v.Name == "data").Value;

                    if (!json.IsNull)
                    {
                        // 3. Desserialização explícita para evitar ambiguidade (string cast)
                        var evento = JsonSerializer.Deserialize<BasketCheckoutEvent>((string)json!);

                        if (evento != null)
                        {
                            // 4. CHAMA O SEU RAPTOR MEDIATOR (Dispara todos os IEventHandler registrados)
                            // Note que enviamos 'evento', que herda de BaseEvent (IEvent)
                            await mediator.PublishAsync(evento, stoppingToken);

                            // 5. ACK: Somente após o sucesso do Mediator confirmamos no Redis
                            await db.StreamAcknowledgeAsync(StreamName, GroupName, msg.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha crítica ao processar mensagem {Id}. Mensagem permanecerá no Stream para reprocessamento.", msg.Id);
                    // Não damos o ACK em caso de erro. A mensagem fica na PEL (Pending Entries List)
                }
            }
        }
    }
}