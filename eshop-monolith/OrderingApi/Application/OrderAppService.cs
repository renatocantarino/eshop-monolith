using AppShared.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using OrderingApi.Data;
using OrderingApi.Models;

namespace OrderingApi.Application;

public class OrderService(OrderDbContext dbContext, ILogger<OrderService> logger)
{
    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        return await dbContext.Orders.ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserNameAsync(string userName)
    {
        return await dbContext.Orders
            .Where(o => o.UserName == userName)
            .ToListAsync();
    }

    // for basket checkout operation
    public async Task CreateOrderAsync(Order order)
    {
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Creates an order from an integration event received via Redis Streams.
    /// </summary>
    public async Task<Order> CreateOrderFromEventAsync(OrderCreatedEvent @event, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(@event, nameof(@event));

        logger.LogInformation(
            "Creating order from event for customer {CustomerId}, basket {BasketId}",
            @event.CustomerId,
            @event.BasketId);

        var order = new Order
        {
            UserName = @event.CustomerId,
            TotalPrice = @event.TotalPrice,
            FirstName = @event.FirstName,
            LastName = @event.LastName,
            EmailAddress = @event.EmailAddress,
            AddressLine = @event.AddressLine
        };

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "Successfully created order {OrderId} from event for customer {CustomerId}",
            order.Id,
            @event.CustomerId);

        return order;
    }
}
