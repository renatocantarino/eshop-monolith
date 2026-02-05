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
        var cacheKey = $"basket:{userName}";

        byte[]? basketBytes = await cache.GetAsync(cacheKey);

        if (basketBytes is null || basketBytes.Length == 0)
            return null;

        return JsonSerializer.Deserialize<ShoppingCart>(basketBytes);
    }

    public async Task UpdateBasket(ShoppingCart basket)
    {
        //N+1 problem
        //foreach (var item in basket.Items)
        //{
        //    var product = await catalogApiClient.GetProductById(item.ProductId);
        //    if (product is not null)
        //    {
        //        item.ProductName = product.Name;
        //        item.Price = product.Price;
        //    }
        //}

        //refact
        var tasks = basket.Items.Select(async item =>
        {
            var product = await catalogApiClient.GetProductById(item.ProductId);
            if (product is not null)
            {
                item.ProductName = product.Name;
                item.Price = product.Price;
            }
            else
            {
                item.ProductName = "toRemove";
            }
        });

        await Task.WhenAll(tasks);

        basket.Items.RemoveAll(i => i.ProductName == "toRemove");

        var cacheKey = $"basket:{basket.UserName}";

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
        byte[] basketBytes = JsonSerializer.SerializeToUtf8Bytes(basket);
        await cache.SetAsync(cacheKey, basketBytes, options);
    }

    public async Task CheckoutBasket(BasketCheckout basketCheckout)
    {
        var cacheKey = $"basket:{basketCheckout.UserName}";
        var shoppingCart = await GetBasket(cacheKey);
        if (shoppingCart is null)
        {
            throw new InvalidOperationException($"Shopping cart for user '{basketCheckout.UserName}' not found.");
        }

        // Set total price on basket checkout event message
        basketCheckout.TotalPrice = shoppingCart.TotalPrice;

        var desconto = await discountGrpc.GetDiscountAsync(1);

        basketCheckout.TotalPrice -= Math.Round(desconto.Amount * 100, 2);

        var checkoutEvent = new AppShared.Events.BasketCheckoutEvent
        {
            BasketId = cacheKey,
            UserName = basketCheckout.UserName,
            TotalPrice = basketCheckout.TotalPrice,
            FirstName = basketCheckout.FirstName,
            LastName = basketCheckout.LastName,
            EmailAddress = basketCheckout.EmailAddress,
            AddressLine = basketCheckout.AddressLine
        };

        await bus.Publish(checkoutEvent);

        // delete the basket
        await DeleteBasket(basketCheckout.UserName);
    }

    public async Task DeleteBasket(string userName)
    {
        await cache.RemoveAsync(userName);
    }

    public async Task DeleteItemInBasket(string userName, int productId)
    {
        var cacheKey = $"basket:{userName}";

        var basket = await GetBasket(userName);

        if (basket is null) return;

        int removedCount = basket.Items.RemoveAll(i => i.ProductId == productId);

        if (removedCount == 0) return;

        if (basket.Items.Count > 0)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };

            byte[] basketBytes = JsonSerializer.SerializeToUtf8Bytes(basket);
            await cache.SetAsync(cacheKey, basketBytes, options);
        }
        else
        {
            await cache.RemoveAsync(cacheKey);
        }
    }
}