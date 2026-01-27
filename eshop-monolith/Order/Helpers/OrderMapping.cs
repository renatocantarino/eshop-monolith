using AppShared.Dtos;

namespace Orders.Helpers;

public static class OrderMapping
{
    public static OrderDTO ToDTO(this Orders.Model.Order order)
    {
        return new OrderDTO
        {
            Id = order.Id,
            UserName = order.UserName,
            TotalPrice = order.TotalPrice,
            FirstName = order.FirstName,
            LastName = order.LastName,
            EmailAddress = order.EmailAddress,
            AddressLine = order.AddressLine
        };
    }

    public static Orders.Model.Order ToModel(this OrderDTO orderDTO)
    {
        return new Orders.Model.Order
        {
            Id = orderDTO.Id,
            UserName = orderDTO.UserName,
            TotalPrice = orderDTO.TotalPrice,
            FirstName = orderDTO.FirstName,
            LastName = orderDTO.LastName,
            EmailAddress = orderDTO.EmailAddress,
            AddressLine = orderDTO.AddressLine
        };
    }
}