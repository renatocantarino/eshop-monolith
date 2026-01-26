using ApiServices.Models;
using AppShared.Dtos;

namespace ApiServices.Mappings;

public static class ShoppingCartItemMapping
{
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

    public static ShoppingCartItem ToModel(this ShoppingCartItemDTO itemDTO)
    {
        return new ShoppingCartItem
        {
            Id = itemDTO.Id,
            ShoppingCartId = itemDTO.ShoppingCartId,
            Quantity = itemDTO.Quantity,
            Color = itemDTO.Color,
            ProductId = itemDTO.ProductId,
            Price = itemDTO.Price,
            ProductName = itemDTO.ProductName
        };
    }
}
