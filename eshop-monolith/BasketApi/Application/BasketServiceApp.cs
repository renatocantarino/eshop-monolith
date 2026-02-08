using AppShared.Dtos;
using BasketApi.ApiClients;
using BasketApi.Application.Data;
using BasketApi.Application.Mappers;
using BasketApi.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace BasketApi.Application;

public interface IBasketServiceApp
{
    Task<ShoppingCartResponse?> GetBasketAsync(string userName, CancellationToken ct);

    Task UpdateBasket(ShoppingCart basket, CancellationToken ct);

    Task CheckoutBasket(BasketCheckout basketCheckout, CancellationToken ct);

    Task DeleteBasket(string userName, CancellationToken ct);
}

public class BasketServiceApp(HybridCache cache, DiscountGrpcService discountGrpc, BasketDbContext dbContext) : IBasketServiceApp
{
    public async Task<ShoppingCartResponse?> GetBasketAsync(string userName, CancellationToken ct)
    {
        var basket = await cache.GetOrCreateAsync(userName, async token =>
        {
            return await dbContext.ShoppingCarts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.UserName == userName, token);
        },
        options: new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(15),
            LocalCacheExpiration = TimeSpan.FromMinutes(5)
        },
        cancellationToken: ct);

        return basket?.ToDTO();
    }

    public async Task UpdateBasket(ShoppingCart basket, CancellationToken ct)
    {
        var existingBasket = await dbContext.ShoppingCarts
             .Include(x => x.Items)
             .FirstOrDefaultAsync(x => x.UserName == basket.UserName, ct);

        if (existingBasket == null)
        {
            dbContext.ShoppingCarts.Add(basket);
        }
        else
        {
            existingBasket.Items.Clear();
            existingBasket.Items.AddRange(basket.Items);

            dbContext.ShoppingCarts.Update(existingBasket);
        }

        await dbContext.SaveChangesAsync();

        await cache.SetAsync(
        key: basket.UserName,
        value: basket,
        options: new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(30)
        },
        tags: new[] { $"cart:{basket.UserName}" });
    }

    public async Task CheckoutBasket(BasketCheckout basketCheckout, CancellationToken ct)
    {
        var shoppingCart = await GetBasketAsync(basketCheckout.UserName, ct);
        if (shoppingCart is null)
        {
            throw new InvalidOperationException($"Shopping cart for user '{basketCheckout.UserName}' not found.");
        }

        var desconto = await discountGrpc.GetDiscountAsync(1);

        var finalPrice = shoppingCart.TotalPrice - desconto.Amount;
        basketCheckout.TotalPrice = Math.Max(0, finalPrice);

        // delete the basket
        await DeleteBasket(basketCheckout.UserName, ct);
    }

    public async Task DeleteBasket(string userName, CancellationToken ct)
    {
        await dbContext.ShoppingCarts
            .Where(x => x.UserName == userName)
            .ExecuteDeleteAsync(ct);

        await cache.RemoveAsync(userName, ct);
    }
}