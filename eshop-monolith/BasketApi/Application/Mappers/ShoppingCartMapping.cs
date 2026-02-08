using AppShared.Dtos;
using BasketApi.Application.Models;

namespace BasketApi.Application.Mappers;

public static class ShoppingCartMapping
{
    public static ShoppingCartResponse ToDTO(this ShoppingCart cart)
    {
        return new ShoppingCartResponse
        {
            Id = cart.Id,
            UserName = cart.UserName,
            Items = [.. cart.Items.Select(i => i.ToDTO())]
        };
    }

    public static ShoppingCartItemResponse ToDTO(this ShoppingCartItem item)
    {
        return new ShoppingCartItemResponse
        {
            Id = item.Id,
            ShoppingCartId = item.ShoppingCartId,
            Quantity = item.Quantity,
            Color = item.Color,
            ProductId = item.ProductId,
            Price = item.Price,
            ProductName = item.ProductName
        };
    }

    public static ShoppingCart ToModel(this ShoppingCartResponse dto)
    {
        return new ShoppingCart
        {
            Id = dto.Id,
            UserName = dto.UserName,
            Items = dto.Items.Select(i => i.ToModel()).ToList()
        };
    }

    public static ShoppingCartItem ToModel(this ShoppingCartItemResponse dto)
    {
        return new ShoppingCartItem
        {
            Id = dto.Id,
            ShoppingCartId = dto.ShoppingCartId,
            Quantity = dto.Quantity,
            Color = dto.Color,
            ProductId = dto.ProductId,
            Price = dto.Price,
            ProductName = dto.ProductName
        };
    }

    public static BasketCheckout ToModel(this BasketCheckoutResponse dto)
    {
        return new BasketCheckout
        {
            AddressLine = dto.AddressLine,
            EmailAddress = dto.EmailAddress,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            TotalPrice = dto.TotalPrice,
            UserName = dto.UserName
        };
    }
}