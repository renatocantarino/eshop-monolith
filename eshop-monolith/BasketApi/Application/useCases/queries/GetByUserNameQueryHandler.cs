using AppShared.Cqrs.Abstractions;
using AppShared.Dtos;

namespace BasketApi.Application.useCases.queries;

/// <summary>
/// Query to get a shopping cart by user name.
/// </summary>
/// <param name="UserName">The user name to search for.</param>
public record GetByUserName(string UserName) : IQuery<ShoppingCartResponse>;

/// <summary>
/// Handler for GetByUserName query.
/// </summary>
public class GetByUserNameQueryHandler(IBasketServiceApp basketServiceApp) : IQueryHandler<GetByUserName, ShoppingCartResponse>
{
    private readonly IBasketServiceApp _basketServiceApp = basketServiceApp ?? throw new ArgumentNullException(nameof(basketServiceApp));

    public async Task<ShoppingCartResponse?> HandleAsync(GetByUserName query, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        ArgumentException.ThrowIfNullOrWhiteSpace(query.UserName, nameof(query.UserName));

        var cart = await _basketServiceApp.GetBasketAsync(query.UserName, ct);

        return cart;
    }
}