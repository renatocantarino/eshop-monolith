using ApiServices.Data;
using ApiServices.Mappings;
using ApiServices.Models;
using AppShared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ApiServices.Endpoints;

public static class ApiServiceEndpoints
{
    public static void MapApiServiceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/apiservice");
        group.MapGet("/products", async (EShopDbContext dbContext) =>
        {
            var products = await dbContext.Products.ToListAsync();
            var dtos = products.Select(p => p.ToDTO());
            return Results.Ok(dtos);
        })
     .WithName("GetAllProducts")
     .Produces<List<ProductDTO>>(StatusCodes.Status200OK);

        group.MapGet("/products/{id}", async (int id, EShopDbContext dbContext) =>
        {
            var product = await dbContext.Products.FindAsync(id);
            return product is not null ? Results.Ok(product.ToDTO()) : Results.NotFound();
        })
     .WithName("GetProductById")
     .Produces<ProductDTO>(StatusCodes.Status200OK);

        app.MapPost("/products", async (ProductDTO productDTO, EShopDbContext dbContext) =>
        {
            var model = productDTO.ToModel();

            dbContext.Products.Add(model);
            await dbContext.SaveChangesAsync();
            return Results.Created($"/products/{model.Id}", model.ToDTO());
        })
       .WithName("CreateProduct")
       .Produces<ProductDTO>(StatusCodes.Status201Created);

        app.MapPut("/products/{id}", async (int id, ProductDTO updatedProductDTO, EShopDbContext dbContext) =>
        {
            var product = await dbContext.Products.FindAsync(id);
            if (product is null) return Results.NotFound();
            
            var updatedProduct = updatedProductDTO.ToModel();
            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.ImageUrl = updatedProduct.ImageUrl;
            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("UpdateProduct")
        .Produces(StatusCodes.Status204NoContent);

        app.MapDelete("/products/{id}", async (int id, EShopDbContext dbContext) =>
        {
            var product = await dbContext.Products.FindAsync(id);
            if (product is null) return Results.NotFound();
            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        })
     .WithName("DeleteProduct")
     .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/basket/{userName}", async (string userName, EShopDbContext dbContext) =>
        {
            var shoppingCart = await dbContext.ShoppingCarts
                .Include(cart => cart.Items)
                .FirstOrDefaultAsync(cart => cart.UserName == userName);

            if (shoppingCart is null)
            {
                return Results.NotFound($"Shopping cart for user '{userName}' not found.");
            }

            return Results.Ok(shoppingCart.ToDTO());
        })
      .WithName("GetBasketByUserName")
      .Produces<ShoppingCartDTO>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status404NotFound);

        // POST (Upsert) Basket
        group.MapPost("/basket", async (ShoppingCartDTO shoppingCartDTO, EShopDbContext dbContext) =>
        {
            var existingCart = await dbContext.ShoppingCarts
                .Include(cart => cart.Items)
                .FirstOrDefaultAsync(cart => cart.UserName == shoppingCartDTO.UserName);

            var cartModel = shoppingCartDTO.ToModel();

            if (existingCart is null)
            {
                dbContext.ShoppingCarts.Add(cartModel);
            }
            else
            {
                existingCart.Items = cartModel.Items;
                dbContext.ShoppingCarts.Update(existingCart);
            }
            await dbContext.SaveChangesAsync();
            return Results.Ok(cartModel.ToDTO());
        })
        .WithName("UpdateBasket")
        .Produces<ShoppingCartDTO>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST Checkout Basket
        group.MapPost("/basket/checkout", async (OrderDTO checkoutOrderDTO, EShopDbContext dbContext) =>
        {
            var shoppingCart = await dbContext.ShoppingCarts
                .Include(cart => cart.Items)
                .FirstOrDefaultAsync(cart => cart.UserName == checkoutOrderDTO.UserName);

            if (shoppingCart is null)
            {
                return Results.NotFound($"Shopping cart for user '{checkoutOrderDTO.UserName}' not found.");
            }

            var order = checkoutOrderDTO.ToModel();

            dbContext.Orders.Add(order);
            dbContext.ShoppingCarts.Remove(shoppingCart);
            await dbContext.SaveChangesAsync();

            return Results.Created($"/orders/{order.Id}", order.ToDTO());
        })
        .WithName("CheckoutBasket")
        .Produces<OrderDTO>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status404NotFound);

        // DELETE Basket by userName
        group.MapDelete("/basket/{userName}", async (string userName, EShopDbContext dbContext) =>
        {
            var shoppingCart = await dbContext.ShoppingCarts
                .Include(cart => cart.Items)
                .FirstOrDefaultAsync(cart => cart.UserName == userName);

            if (shoppingCart is null)
            {
                return Results.NotFound($"Shopping cart for user '{userName}' not found.");
            }

            dbContext.ShoppingCarts.Remove(shoppingCart);
            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteBasket")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        ////////// Order Endpoints

        // GET All Orders
        group.MapGet("/orders", async (EShopDbContext dbContext) =>
        {
            var orders = await dbContext.Orders.ToListAsync();
            var dtos = orders.Select(o => o.ToDTO()).ToList();
            return Results.Ok(dtos);
        })
        .WithName("GetAllOrders")
        .Produces<List<OrderDTO>>(StatusCodes.Status200OK);

        // GET Order by userName
        group.MapGet("/orders/{userName}", async (string userName, EShopDbContext dbContext) =>
        {
            var orders = await dbContext.Orders
                .Where(o => o.UserName == userName)
                .ToListAsync();

            if (orders is null || !orders.Any())
            {
                return Results.NotFound($"No orders found for user '{userName}'.");
            }

            var dtos = orders.Select(o => o.ToDTO()).ToList();
            return Results.Ok(dtos);
        })
        .WithName("GetOrderByUserName")
        .Produces<List<OrderDTO>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}