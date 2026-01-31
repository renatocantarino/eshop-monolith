using AppShared.Dtos;
using OrderingApi.Models;

namespace OrderingApi.Application.Mappers;

public static class OrderMapper
{
    public static OrderResponse ToDto(this Order order)
    {
        return new()
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

    public static Order ToDomain(this OrderResponse orderDto)
    {
        return new()
        {
            Id = orderDto.Id,
            UserName = orderDto.UserName,
            TotalPrice = orderDto.TotalPrice,
            FirstName = orderDto.FirstName,
            LastName = orderDto.LastName,
            EmailAddress = orderDto.EmailAddress,
            AddressLine = orderDto.AddressLine
        };
    }
}