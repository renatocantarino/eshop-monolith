using AppShared.Cqrs.Abstractions;
using AppShared.Dtos;
using CatalogApi.Application.Mappers;

namespace CatalogApi.Application.useCases;

public record GetByIdQuery(int Id) : IQuery<ProductResponse>;

public class GetByIdQueryHandler(ICatalogServiceApp catalogServiceApp) : IQueryHandler<GetByIdQuery, ProductResponse>
{
    private readonly ICatalogServiceApp _catalogServiceApp = catalogServiceApp ?? throw new ArgumentNullException(nameof(catalogServiceApp));

    public async Task<ProductResponse> HandleAsync(GetByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        var products = await _catalogServiceApp.GetById(query.Id, cancellationToken);

        return products!.ToDto();
    }
}