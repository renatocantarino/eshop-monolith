using AppShared.Dtos;
using Catalog.Model;

namespace Catalog.Helpers;

public static class ProductMapping
{
    public static ProductDTO ToDTO(this Product product)
    {
        return new ProductDTO
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl
        };
    }

    public static Product ToModel(this ProductDTO productDTO)
    {
        return new Product
        {
            Id = productDTO.Id,
            Name = productDTO.Name,
            Description = productDTO.Description,
            Price = productDTO.Price,
            ImageUrl = productDTO.ImageUrl
        };
    }
}