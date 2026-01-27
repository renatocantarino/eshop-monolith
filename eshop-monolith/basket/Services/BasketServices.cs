using Basket.Data;
using Basket.Model;
using Microsoft.EntityFrameworkCore;

namespace Basket.Services;

public class BasketServices(BasketDbContext dbContext)
{
    public async Task<List<ShoppingCart>> GetAllAsync()
    {
        return await dbContext.ShoppingCarts.Include(c => c.Items).ToListAsync();
    }

    public async Task<ShoppingCart?> GetByIdAsync(int id)
    {
        return await dbContext.ShoppingCarts.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ShoppingCart?> GetByUserNameAsync(string userName)
    {
        return await dbContext.ShoppingCarts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserName == userName);
    }

    public async Task CreateAsync(ShoppingCart cart)
    {
        await dbContext.ShoppingCarts.AddAsync(cart);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(ShoppingCart toUpdate, ShoppingCart input)
    {
        toUpdate.UserName = input.UserName;
        toUpdate.Items = input.Items;

        dbContext.ShoppingCarts.Update(toUpdate);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(ShoppingCart cart)
    {
        dbContext.ShoppingCarts.Remove(cart);
        await dbContext.SaveChangesAsync();
    }

    public async Task AddItemAsync(int cartId, ShoppingCartItem item)
    {
        var cart = await GetByIdAsync(cartId);
        if (cart is null) throw new InvalidOperationException("Shopping cart not found");

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == item.ProductId && i.Color == item.Color);
        if (existingItem is not null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            cart.Items.Add(item);
        }

        dbContext.ShoppingCarts.Update(cart);
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(int cartId, int itemId)
    {
        var cart = await GetByIdAsync(cartId);
        if (cart is null) throw new InvalidOperationException("Shopping cart not found");

        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is not null)
        {
            cart.Items.Remove(item);
            dbContext.ShoppingCarts.Update(cart);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task ClearAsync(int cartId)
    {
        var cart = await GetByIdAsync(cartId);
        if (cart is null) throw new InvalidOperationException("Shopping cart not found");

        cart.Items.Clear();
        dbContext.ShoppingCarts.Update(cart);
        await dbContext.SaveChangesAsync();
    }
}
