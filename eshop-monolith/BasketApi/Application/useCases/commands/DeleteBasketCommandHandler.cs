using AppShared.Cqrs.Abstractions;

namespace BasketApi.Application.useCases.commands;

public class DeleteBasketCommandHandler(IBasketServiceApp basketServiceApp) : ICommandHandler<DeleteBasketCommand, Unit>
{
    private readonly IBasketServiceApp _basketServiceApp = basketServiceApp ?? throw new ArgumentNullException(nameof(basketServiceApp));

    public async Task<Unit> HandleAsync(DeleteBasketCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        await _basketServiceApp.DeleteBasket(command.userName, ct);

        return Unit.Value;
    }
}

public record DeleteBasketCommand(string userName) : ICommand<Unit>;