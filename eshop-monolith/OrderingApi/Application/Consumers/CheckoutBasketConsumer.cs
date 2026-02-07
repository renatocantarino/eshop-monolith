using AppShared.Cqrs.Abstractions;
using AppShared.Events;
using OrderingApi.Models;

namespace OrderingApi.Application.Consumers;

public class CheckoutBasketConsumer(OrderService orderService, ILogger<CheckoutBasketConsumer> logger) : IEventHandler<BasketCheckoutEvent>
{
    private readonly OrderService _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
    private readonly ILogger<CheckoutBasketConsumer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task HandleAsync(BasketCheckoutEvent @event, CancellationToken ct)
    {
        _logger.LogInformation("Iniciando processamento de checkout para o comprador: {Buyer}", @event.Buyer);

        var order = new Order
        {
            UserName = @event.Buyer,
            TotalPrice = @event.TotalPrice,
            FirstName = @event.FirstName,
            LastName = @event.LastName,
            EmailAddress = @event.EmailAddress,
            AddressLine = @event.AddressLine
        };

        await _orderService.CreateOrderAsync(order);

        _logger.LogInformation("Pedido processado com sucesso para {Buyer}", @event.Buyer);
    }
}