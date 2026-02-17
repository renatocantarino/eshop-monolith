using AppShared.Cqrs.Abstractions;
using AppShared.Dtos;
using AppShared.IntegrationEvents;
using AppShared.Messaging;
using BasketApi.Application.Mappers;
using Microsoft.Extensions.Options;

namespace BasketApi.Application.useCases.commands;

public class CheckoutBasketCommandHandler : ICommandHandler<CheckoutBasketCommand, Unit>
{
    private readonly IBasketServiceApp _basketServiceApp;
    private readonly IEventBus _eventBus;
    private readonly RedisMessagingOptions _options;
    private readonly ILogger<CheckoutBasketCommandHandler> _logger;

    public CheckoutBasketCommandHandler(
        IBasketServiceApp basketServiceApp,
        IEventBus eventBus,
        IOptions<RedisMessagingOptions> options,
        ILogger<CheckoutBasketCommandHandler> logger)
    {
        _basketServiceApp = basketServiceApp ?? throw new ArgumentNullException(nameof(basketServiceApp));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> HandleAsync(CheckoutBasketCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));

        var basketCheckout = command.response;

        var basket = await _basketServiceApp.CheckoutBasket(basketCheckout.ToModel(), ct);

        // Create integration event
        var orderCreatedEvent = new OrderCreatedEvent(
            CustomerId: basketCheckout.UserName,
            BasketId: basket.ShoppingCartId,
            Items: basket.Items.Select(item => new OrderItemDto(
                ProductId: item.ProductId,
                ProductName: item.ProductName,
                Quantity: item.Quantity,
                Price: item.Price,
                Color: item.Color
            )).ToList(),
            TotalPrice: basket.TotalPrice,
            FirstName: basketCheckout.FirstName,
            LastName: basketCheckout.LastName,
            EmailAddress: basketCheckout.EmailAddress,
            AddressLine: basketCheckout.AddressLine
        );

        // Publish event to Redis Streams
        var messageId = await _eventBus.PublishAsync(
            _options.OrdersStreamName,
            orderCreatedEvent,
            ct);

        _logger.LogInformation(
            "Published OrderCreatedEvent for user {UserName} with message ID {MessageId}",
            basketCheckout.UserName,
            messageId);

        return Unit.Value;
    }
}

public record CheckoutBasketCommand(BasketCheckoutResponse response) : ICommand<Unit>;