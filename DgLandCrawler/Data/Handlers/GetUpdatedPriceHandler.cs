using DgLandCrawler.Data.Repository;
using DgLandCrawler.Helper;
using MediatR;

namespace DgLandCrawler.Data.Handlers
{
    public class GetUpdatedPriceHandler(IDGProductRepository productRepository) : IRequestHandler<GetUpdatedPriceRequest, MemoryStream>
    {
        private readonly IDGProductRepository _productRepository = productRepository;

        public async Task<MemoryStream> Handle(GetUpdatedPriceRequest request, CancellationToken cancellationToken)
        {
            return await CSVHelper.GetMemoryStreamAsync(await _productRepository.GetUpdatedCrawledPriceList(cancellationToken));
        }
    }

    public record struct GetUpdatedPriceRequest : IRequest<MemoryStream>
    {
    }
}
