using AppShared.Dtos;

namespace AppShared.Events;

public record BasketCheckoutEvent : BaseEvent
{
    public required string Buyer { get; init; }
    public required List<ShoppingCartItemResponse> Items { get; init; }
    public required decimal TotalPrice { get; init; }

    // Address info
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string EmailAddress { get; init; }
    public required string AddressLine { get; init; }
}