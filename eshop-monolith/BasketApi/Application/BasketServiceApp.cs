using AppShared.Events;
using BasketApi.ApiClients;
using BasketApi.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BasketApi.Application;

public interface IBasketServiceApp
{
    Task<ShoppingCart?> GetBasket(string userName);

    Task UpdateBasket(ShoppingCart basket);

    Task CheckoutBasket(BasketCheckout basketCheckout);

    Task DeleteBasket(string userName);

    Task DeleteItemInBasket(string userName, int productId);
}

public class BasketServiceApp(IDistributedCache cache, CatalogApiClient catalogApiClient, DiscountGrpcService discountGrpc, IBus bus) : IBasketServiceApp
{
    public async Task<ShoppingCart?> GetBasket(string userName)
    {
        var basket = await cache.GetStringAsync(userName);

        return string.IsNullOrEmpty(basket) ? null : JsonSerializer.Deserialize<ShoppingCart>(basket);
    }

    public async Task UpdateBasket(ShoppingCart basket)
    {
        // Batch fetch: single HTTP call instead of N+1
        var productIds = basket.Items.Select(i => i.ProductId).Distinct();
        var products = await catalogApiClient.GetProductsByIds(productIds);
        var productLookup = products.ToDictionary(p => p.Id);

        foreach (var item in basket.Items)
        {
            if (productLookup.TryGetValue(item.ProductId, out var product))
            {
                item.ProductName = product.Name;
                item.Price = product.Price;
            }
        }

        var jsonData = JsonSerializer.SerializeToUtf8Bytes(basket);
        await cache.SetAsync(basket.UserName, jsonData);
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

        // Send basket checkout event to rabbitmq using masstransit
        var integrationEvent = new BasketCheckoutEvent
        {
            UserName = basketCheckout.UserName,
            TotalPrice = shoppingCart.TotalPrice,
            FirstName = basketCheckout.FirstName,
            LastName = basketCheckout.LastName,
            EmailAddress = basketCheckout.EmailAddress,
            AddressLine = basketCheckout.AddressLine
        };
        // Publish checkout basket event and create order
        await bus.Publish(integrationEvent);

        // Delete the basket
        await DeleteBasket(basketCheckout.UserName);
    }

    public async Task DeleteBasket(string userName)
    {
        await cache.RemoveAsync(userName);
    }

    public async Task DeleteItemInBasket(string userName, int productId)
    {
        var basket = await GetBasket("swn");

        if (basket == null) return;

        var item = basket!.Items.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
        {
            await cache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket));
        }
    }
}