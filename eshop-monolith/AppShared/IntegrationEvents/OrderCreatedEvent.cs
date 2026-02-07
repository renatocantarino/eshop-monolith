namespace AppShared.IntegrationEvents;

public record OrderCreatedEvent(
    string CustomerId,
    int BasketId,
    List<OrderItemDto> Items,
    decimal TotalPrice,
    string FirstName,
    string LastName,
    string EmailAddress,
    string AddressLine
);
