using AppShared.Dtos;
using Basket.Model;

namespace Basket.Helpers;

public static class ShoppingCartMapping
{
    public static ShoppingCartDTO ToDTO(this ShoppingCart cart)
    {
        return new ShoppingCartDTO
        {
            Id = cart.Id,
            UserName = cart.UserName,
            Items = cart.Items.Select(i => i.ToDTO()).ToList()
        };
    }

    public static ShoppingCartItemDTO ToDTO(this ShoppingCartItem item)
    {
        return new ShoppingCartItemDTO
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

    public static ShoppingCart ToModel(this ShoppingCartDTO dto)
    {
        return new ShoppingCart
        {
            Id = dto.Id,
            UserName = dto.UserName,
            Items = dto.Items.Select(i => i.ToModel()).ToList()
        };
    }

    public static ShoppingCartItem ToModel(this ShoppingCartItemDTO dto)
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
}
