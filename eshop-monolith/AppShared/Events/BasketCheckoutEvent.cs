namespace AppShared.Events;

public record BasketCheckoutEvent : BaseEvent
{
    public string UserName { get; init; } = default!;
    public decimal TotalPrice { get; init; }

    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string EmailAddress { get; init; }
    public string AddressLine { get; init; }

    public string BasketId { get; init; }
}