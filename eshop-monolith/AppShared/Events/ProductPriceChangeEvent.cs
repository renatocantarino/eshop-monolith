namespace AppShared.Events;

public record ProductPriceChangeEvent : BaseEvent
{
    public int ProductId { get; init; }

    public decimal NewPrice { get; init; }
}

public record ProductDeletedEvent : BaseEvent
{
    public int ProductId { get; init; }
}