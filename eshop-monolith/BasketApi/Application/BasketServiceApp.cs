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
        //N+1 problem
        foreach (var item in basket.Items)
        {
            var product = await catalogApiClient.GetProductById(item.ProductId);
            if (product is not null)
            {
                item.ProductName = product.Name;
                item.Price = product.Price;
            }
        }

        //refact
        //var tasks = basket.Items.Select(async item =>
        //{
        //    var product = await catalogApiClient.GetProductById(item.ProductId);
        //    if (product != null)
        //    {
        //        item.Price = product.Price;
        //        item.ProductName = product.Name;
        //    }
        //}).ToList();

        //await Task.WhenAll(tasks);

        /*

         get in batch
        var products = await catalogApiClient.GetProductsByIds(productIds);

        foreach (var item in basket.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product != null)
            {
                item.Price = product.Price;
                item.ProductName = product.Name;
            }
        }

        await cache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket));

         */

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