using Catalog.Data;
using Catalog.Model;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Services;

public class CatalogServices(CatalogDbContext dbContext)
{
    public async Task<List<Product>> GetAllAsync()
    {
        return await dbContext.Products.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int idProduct)
    {
        return await dbContext.Products.FindAsync(idProduct);
    }

    public async Task CreateAsync(Product product)
    {
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product toUpdate, Product input)
    {
        toUpdate.Name = input.Name;
        toUpdate.Description = input.Description;
        toUpdate.Price = input.Price;
        toUpdate.ImageUrl = input.ImageUrl;

        dbContext.Products.Update(toUpdate);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync();
    }
}