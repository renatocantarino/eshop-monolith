using Microsoft.EntityFrameworkCore;
using Orders.Data;

namespace Orders.Services;

public class OrderServices(OrderDbContext dbContext)
{
    public async Task<List<Orders.Model.Order>> GetAllAsync()
    {
        return await dbContext.Orders.ToListAsync();
    }

    public async Task<Orders.Model.Order?> GetByIdAsync(int id)
    {
        return await dbContext.Orders.FindAsync(id);
    }

    public async Task<List<Orders.Model.Order>> GetByUserNameAsync(string userName)
    {
        return await dbContext.Orders.Where(o => o.UserName == userName).ToListAsync();
    }

    public async Task CreateAsync(Orders.Model.Order order)
    {
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Orders.Model.Order toUpdate, Orders.Model.Order input)
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

    public async Task DeleteAsync(Orders.Model.Order order)
    {
        dbContext.Orders.Remove(order);
        await dbContext.SaveChangesAsync();
    }
}