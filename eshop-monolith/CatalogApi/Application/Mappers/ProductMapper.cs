using AppShared.Dtos;

namespace CatalogApi.Application.Mappers;

public static class ProductMapper
{
    public static ProductResponse ToDto(this Models.Product product)
        => new()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl
        };

    public static IReadOnlyCollection<ProductResponse> ToDto(this IReadOnlyCollection<Models.Product> products)
        => [.. products.Select(p => p.ToDto()).ToList().AsReadOnly()];

    public static Models.Product ToDomain(this ProductResponse productDto)
        => new()
        {
            Id = productDto.Id,
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            ImageUrl = productDto.ImageUrl
        };
}