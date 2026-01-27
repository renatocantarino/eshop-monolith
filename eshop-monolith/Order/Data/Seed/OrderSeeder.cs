using AppShared.Data;
using Microsoft.EntityFrameworkCore;

namespace Orders.Data.Seed;

public class OrderSeeder(OrderDbContext dbContext) : ISeeder
{
    public async Task ExecuteAsync()
    {
        if (!await dbContext.Orders.AnyAsync())
        {
            await dbContext.Orders.AddRangeAsync(GetSampleOrders());
            await dbContext.SaveChangesAsync();
        }
    }

    public static IEnumerable<Model.Order> GetSampleOrders() =>
    [
        new Orders.Model.Order
        {
            UserName = "john.doe",
            TotalPrice = 89.97m,
            FirstName = "John",
            LastName = "Doe",
            EmailAddress = "john.doe@email.com",
            AddressLine = "123 Main St, New York, NY 10001"
        },
        new Orders.Model.Order
        {
            UserName = "jane.smith",
            TotalPrice = 39.99m,
            FirstName = "Jane",
            LastName = "Smith",
            EmailAddress = "jane.smith@email.com",
            AddressLine = "456 Oak Ave, Los Angeles, CA 90001"
        }
    ];
}