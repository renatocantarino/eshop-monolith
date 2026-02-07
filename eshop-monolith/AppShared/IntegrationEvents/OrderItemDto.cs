namespace AppShared.IntegrationEvents;

public record OrderItemDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal Price,
    string Color
);
