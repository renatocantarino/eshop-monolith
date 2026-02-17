using AppShared.IntegrationEvents;
using BasketApi.Application.Models;

namespace BasketApi.Application.Mappers;

public static class BasketMappingExtensions
{
    public static OrderCreatedEvent ToOrderCreatedEvent(this BasketCheckout cart)
    {
        return new OrderCreatedEvent(
            CustomerId: cart.UserName,
            BasketId: cart.ShoppingCartId,
            Items: cart.Items.Select(item => new OrderItemDto(
                item.ProductId,
                item.ProductName,
                item.Quantity,
                item.Price,
                item.Color
            )).ToList(),
            TotalPrice: cart.TotalPrice,
            FirstName: cart.FirstName,
            LastName: cart.LastName,
            EmailAddress: cart.EmailAddress,
            AddressLine: cart.AddressLine
        );
    }
}