using Microsoft.EntityFrameworkCore;

namespace Ordering.Data;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("ordering");
        base.OnModelCreating(builder);
    }
}