using DgLandCrawler.Abstraction;
using DgLandCrawler.Data.Repository;
using DgLandCrawler.Models.DTO;
using MediatR;

namespace DgLandCrawler.Data.Handlers
{
    public class DGProductHandler : IRequestHandler<DGProductQuery, DGView>
    {
        private readonly IDGProductRepository _repository;

        public DGProductHandler(IDGProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<DGView> Handle(DGProductQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetById(request.id, cancellationToken);
        }
    }
    public record struct DGProductQuery(int id) : ICachedQuery<DGView>
    {
        public string Key => $"product-{id}";

        public TimeSpan? Expiration => null;
    }
}
