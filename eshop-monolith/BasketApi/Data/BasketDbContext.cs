using BasketApi.Application.Models;
using Microsoft.EntityFrameworkCore;
using static Grpc.Core.Metadata;

namespace BasketApi.Data;

public class BasketDbContext(DbContextOptions<BasketDbContext> options) : DbContext(options)
{
    public DbSet<ShoppingCart> ShoppingCarts { get; set; } = default!;
    public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; } = default!;

    public DbSet<OutboxMessage> OutboxMessages { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<ShoppingCart>()
        .HasKey(s => s.Id);

        builder.Entity<ShoppingCart>(entity =>
        {
            entity.HasIndex(s => s.UserName)
                  .IsUnique();

            entity.Property(s => s.UserName)
                  .HasMaxLength(200)
                  .IsRequired();
        });

        builder.Entity<ShoppingCart>()
         .HasMany(s => s.Items)
         .WithOne()
         .HasForeignKey(si => si.ShoppingCartId);
    }
}