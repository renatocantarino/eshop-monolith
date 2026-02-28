using AppShared.Cqrs.Abstractions;
using AppShared.Dtos;
using CatalogApi.Application.Mappers;

namespace CatalogApi.Application.useCases;

public record GetByIdsQuery(IEnumerable<int> Ids) : IQuery<IReadOnlyCollection<ProductResponse>>;

public class GetByIdsQueryHandler(ICatalogServiceApp catalogServiceApp) : IQueryHandler<GetByIdsQuery, IReadOnlyCollection<ProductResponse>>
{
    private readonly ICatalogServiceApp _catalogServiceApp = catalogServiceApp ?? throw new ArgumentNullException(nameof(catalogServiceApp));

    public async Task<IReadOnlyCollection<ProductResponse>> HandleAsync(GetByIdsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        var products = await _catalogServiceApp.GetByIds(query.Ids, cancellationToken);

        return products.ToDto();
    }
}
