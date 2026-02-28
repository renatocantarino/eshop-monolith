using AppShared.Events;
using CatalogApi.Data;
using CatalogApi.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CatalogApi.Application;

public interface ICatalogServiceApp
{
    Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Product?> GetById(int id, CancellationToken cancellationToken = default);

    Task CreateAsync(Product product, CancellationToken cancellationToken = default);

    Task UpdateAsync(Product productInput, CancellationToken cancellationToken = default);

    Task DeleteAsync(int productId, CancellationToken cancellationToken = default);
}

public class CatalogServiceApp(CatalogDbContext catalogDbContext, IBus bus) : ICatalogServiceApp
{
    private readonly CatalogDbContext _catalogContext = catalogDbContext;

    public async Task CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _catalogContext.Products.Add(product);
        await _catalogContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int productId, CancellationToken cancellationToken = default)
    {
        int rowsAffected = await _catalogContext.Products
        .Where(p => p.Id == productId)
        .ExecuteDeleteAsync(cancellationToken);

        if (rowsAffected == 0) return;

        // Nota:ler Transactional Outbox Pattern
        var productDeleted = new ProductDeletedEvent { ProductId = productId };

        await bus.Publish(productDeleted, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken ct = default)
    {
        return await _catalogContext.Products
           .AsNoTracking()
           .OrderBy(p => p.Name)
           .ToListAsync(ct);
    }

    public async Task<Product?> GetById(int id, CancellationToken cancellationToken = default)
    {
        return await _catalogContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(Product productInput, CancellationToken cancellationToken = default)
    {
        var updated = await _catalogContext.Products
         .FirstOrDefaultAsync(p => p.Id == productInput.Id, cancellationToken);

        if (updated is null) return;

        var oldPrice = updated.Price;
        var priceChanged = oldPrice != productInput.Price;

        updated.Name = productInput.Name;
        updated.Description = productInput.Description;
        updated.ImageUrl = productInput.ImageUrl;
        updated.Price = productInput.Price;

        await _catalogContext.SaveChangesAsync(cancellationToken);

        if (priceChanged)
        {
            var priceEvent = new ProductPriceChangeEvent
            {
                ProductId = updated.Id,
                NewPrice = productInput.Price,
            };

            await bus.Publish(priceEvent, cancellationToken);
        }
    }
}