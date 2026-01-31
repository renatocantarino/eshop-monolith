using AppShared.Cqrs.Abstractions;
using AppShared.Dtos;
using CatalogApi.Application.Mappers;

namespace CatalogApi.Application.useCases;

public record GetAllQuery : IQuery<IReadOnlyCollection<ProductResponse>>;

public class GetAllQueryHandler(ICatalogServiceApp catalogServiceApp) : IQueryHandler<GetAllQuery, IReadOnlyCollection<ProductResponse>>
{
    private readonly ICatalogServiceApp _catalogServiceApp = catalogServiceApp ?? throw new ArgumentNullException(nameof(catalogServiceApp));

    public async Task<IReadOnlyCollection<ProductResponse>> HandleAsync(GetAllQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        var products = await _catalogServiceApp.GetAllAsync(cancellationToken);

        return products.ToDto();
    }
}