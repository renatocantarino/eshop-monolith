using AppShared.Cqrs.Abstractions;
using AppShared.Dtos;
using BasketApi.Application.Mappers;

namespace BasketApi.Application.useCases.commands;

public class CheckoutBasketCommandHandler(IBasketServiceApp basketServiceApp) : ICommandHandler<CheckoutBasketCommand, Unit>
{
    private readonly IBasketServiceApp _basketServiceApp = basketServiceApp ?? throw new ArgumentNullException(nameof(basketServiceApp));

    public async Task<Unit> HandleAsync(CheckoutBasketCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        await _basketServiceApp.CheckoutBasket(command.response.ToModel());

        return Unit.Value;
    }
}

public record CheckoutBasketCommand(BasketCheckoutResponse response) : ICommand<Unit>;