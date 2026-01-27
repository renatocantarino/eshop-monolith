using AppShared.Dtos;

namespace Ordering.Helpers;

public static class OrderMapping
{
    public static OrderDTO ToDTO(this Order order)
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

    public static Order ToModel(this OrderDTO orderDTO)
    {
        return new Order
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