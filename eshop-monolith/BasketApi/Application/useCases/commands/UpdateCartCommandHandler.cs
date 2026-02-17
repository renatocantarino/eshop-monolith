using AppShared.Cqrs.Abstractions;
using AppShared.Dtos;
using BasketApi.Application.Mappers;

namespace BasketApi.Application.useCases.commands;

public class UpdateCartCommandHandler(IBasketServiceApp basketServiceApp) : ICommandHandler<UpdateCartCommand, ShoppingCartResponse>
{
    private readonly IBasketServiceApp _basketServiceApp = basketServiceApp ?? throw new ArgumentNullException(nameof(basketServiceApp));

    public async Task<ShoppingCartResponse> HandleAsync(UpdateCartCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        await _basketServiceApp.UpdateBasket(command.response.ToModel(), ct);

        return command.response;
    }
}

public record UpdateCartCommand(ShoppingCartResponse response) : ICommand<ShoppingCartResponse>;