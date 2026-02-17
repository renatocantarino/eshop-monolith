using AppShared.Dtos;
using AppShared.IntegrationEvents;
using BasketApi.ApiClients;
using BasketApi.Application.Mappers;
using BasketApi.Application.Models;
using BasketApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using System.Text.Json;

namespace BasketApi.Application;

public interface IBasketServiceApp
{
    Task<ShoppingCartResponse?> GetBasketAsync(string userName, CancellationToken ct);

    Task UpdateBasket(ShoppingCart basket, CancellationToken ct);

    Task<BasketCheckout> CheckoutBasket(BasketCheckout basketCheckout, CancellationToken ct);

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

    public async Task<BasketCheckout> CheckoutBasket(BasketCheckout basketCheckout, CancellationToken ct)
    {
        // Criamos uma variável para armazenar o carrinho e retorná-lo após a transação
        ShoppingCart shoppingCart = null!;

        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            // Importante: A transação deve ser aberta DENTRO da ExecutionStrategy para suportar retentativas
            await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

            try
            {
                shoppingCart = await dbContext.ShoppingCarts
                    .TagWith("Checkout_GetCartWithItems")
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.UserName == basketCheckout.EmailAddress, ct);

                if (shoppingCart is null)
                {
                    throw new InvalidOperationException($"Shopping cart for user '{basketCheckout.UserName}' not found.");
                }

                // 1. Atualiza o TotalPrice no evento
                basketCheckout.TotalPrice = shoppingCart.TotalPrice;
                basketCheckout.Items = shoppingCart.Items;
                basketCheckout.ShoppingCartId = shoppingCart.Id;

                // 2. Adiciona ao Outbox
                var outboxMessage = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = typeof(OrderCreatedEvent).Name!,
                    Content = JsonSerializer.Serialize(basketCheckout, (JsonSerializerOptions?)null),
                    OccuredOn = DateTime.UtcNow
                };

                dbContext.OutboxMessages.Add(outboxMessage);

                // 3. Remove o carrinho do banco
                dbContext.ShoppingCarts.Remove(shoppingCart);

                // 4. Salva e Commita
                await dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                // 5. Invalida o Cache (após o commit para evitar inconsistência se o DB falhar)
                await cache.RemoveAsync(basketCheckout.UserName, ct);
            }
            catch
            {
                // O Rollback é automático ao dar dispose no transaction se o Commit não foi chamado,
                // mas mantê-lo explicitamente é uma boa prática.
                await transaction.RollbackAsync(ct);
                throw; // Nunca "engula" a exceção em uma Strategy, senão ela não saberá se deve tentar novamente.
            }
        });

        return basketCheckout;
    }

    public async Task DeleteBasket(string userName, CancellationToken ct)
    {
        await dbContext.ShoppingCarts
            .Where(x => x.UserName == userName)
            .ExecuteDeleteAsync(ct);

        await cache.RemoveAsync(userName, ct);
    }
}