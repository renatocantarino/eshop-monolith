using ApiServices.Models;
using AppShared.Dtos;

namespace ApiServices.Mappings;

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

    public static ShoppingCart ToModel(this ShoppingCartDTO cartDTO)
    {
        return new ShoppingCart
        {
            Id = cartDTO.Id,
            UserName = cartDTO.UserName,
            Items = cartDTO.Items.Select(i => i.ToModel()).ToList()
        };
    }
}
