using CatalogApi.Data;
using CatalogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogApi.Application;

public interface ICatalogServiceApp
{
    Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default);
}

public class CatalogServiceApp(CatalogDbContext catalogDbContext) : ICatalogServiceApp
{
    private readonly CatalogDbContext _catalogContext = catalogDbContext;

    public async Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken ct = default)
    {
        return await _catalogContext.Products
           .AsNoTracking()
           .OrderBy(p => p.Name)
           .ToListAsync(ct);
    }
}