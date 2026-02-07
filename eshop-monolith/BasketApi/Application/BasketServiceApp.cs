using AppShared.Events;
using BasketApi.ApiClients;
using BasketApi.Application.Mappers;
using BasketApi.Model;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BasketApi.Application;

public interface IBasketServiceApp
{
    Task<ShoppingCart?> GetBasket(string userName);

    Task UpdateBasket(ShoppingCart basket);

    Task CheckoutBasket(BasketCheckout basketCheckout);

    Task DeleteBasket(string userName);
}

public class BasketServiceApp(IDistributedCache cache, DiscountGrpcService discountGrpc, IEventBus publishEndpoint) : IBasketServiceApp
{
    public async Task<ShoppingCart?> GetBasket(string userName)
    {
        var basket = await cache.GetStringAsync(userName);
        return string.IsNullOrEmpty(basket) ? null :
            JsonSerializer.Deserialize<ShoppingCart>(basket);
    }

    public async Task UpdateBasket(ShoppingCart basket)
    {
        // Next Section:
        // Before update(Add/remove Item) into SC, we should call Catalog ms GetProductById method
        // Get latest product information and set Price and ProductName when adding item into SC

        await cache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket));
    }

    public async Task CheckoutBasket(BasketCheckout basketCheckout)
    {
        var shoppingCart = await GetBasket(basketCheckout.UserName);
        if (shoppingCart is null)
        {
            throw new InvalidOperationException($"Shopping cart for user '{basketCheckout.UserName}' not found.");
        }

        // Set total price on basket checkout event message
        basketCheckout.TotalPrice = shoppingCart.TotalPrice;

        var desconto = await discountGrpc.GetDiscountAsync(1);

        basketCheckout.TotalPrice -= Math.Round(desconto.Amount * 100, 2);

        var eventMessage = new BasketCheckoutEvent
        {
            Buyer = basketCheckout.UserName,
            TotalPrice = basketCheckout.TotalPrice,
            Items = shoppingCart.Items.Select(i => i.ToDTO()).ToList(),
            FirstName = basketCheckout.FirstName,
            LastName = basketCheckout.LastName,
            EmailAddress = basketCheckout.EmailAddress,
            AddressLine = basketCheckout.AddressLine
        };

        await publishEndpoint.PublishAsync("order_stream", eventMessage);

        // delete the basket
        await DeleteBasket(basketCheckout.UserName);
    }

    public async Task DeleteBasket(string userName)
    {
        await cache.RemoveAsync(userName);
    }
}