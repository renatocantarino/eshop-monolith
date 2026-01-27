using Microsoft.EntityFrameworkCore;

namespace Orders.Data;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Model.Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("orders");
        base.OnModelCreating(builder);
    }
}