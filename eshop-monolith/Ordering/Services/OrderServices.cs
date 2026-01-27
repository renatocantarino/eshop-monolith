using Microsoft.EntityFrameworkCore;

namespace Ordering.Services;

public class OrderServices(OrderDbContext dbContext)
{
    public async Task<List<Order>> GetAllAsync()
    {
        return await dbContext.Orders.ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await dbContext.Orders.FindAsync(id);
    }

    public async Task<List<Order>> GetByUserNameAsync(string userName)
    {
        return await dbContext.Orders.Where(o => o.UserName == userName).ToListAsync();
    }

    public async Task CreateAsync(Order order)
    {
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Order toUpdate, Order input)
    {
        toUpdate.UserName = input.UserName;
        toUpdate.TotalPrice = input.TotalPrice;
        toUpdate.FirstName = input.FirstName;
        toUpdate.LastName = input.LastName;
        toUpdate.EmailAddress = input.EmailAddress;
        toUpdate.AddressLine = input.AddressLine;

        dbContext.Orders.Update(toUpdate);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Order order)
    {
        dbContext.Orders.Remove(order);
        await dbContext.SaveChangesAsync();
    }
}