using Basket.Model;
using Microsoft.EntityFrameworkCore;

namespace Basket.Data;

public class BasketDbContext(DbContextOptions<BasketDbContext> options) : DbContext(options)
{
    public DbSet<ShoppingCart> ShoppingCarts { get; set; }
    public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("basket");

        builder.Entity<ShoppingCart>()
            .HasKey(s => s.Id);

        builder.Entity<ShoppingCart>()
            .HasIndex(s => s.UserName)
            .IsUnique();

        builder.Entity<ShoppingCart>()
            .HasMany(s => s.Items)
            .WithOne(si => si.ShoppingCart)
            .HasForeignKey(si => si.ShoppingCartId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(builder);
    }
}