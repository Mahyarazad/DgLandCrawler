using DgLandCrawler.Services.SiteCrawler;
using MediatR;

namespace DgLandCrawler.Data.Handlers
{
    public class CrawlerHandler(ISiteCrawlerService siteCrawlerService) : IRequestHandler<CrawlerQuery>
    {
        
        private readonly ISiteCrawlerService _siteCrawlerService = siteCrawlerService;
        public async Task Handle(CrawlerQuery request, CancellationToken cancellationToken)
        {
            switch (request.Request)
            {
                case CrawlRequest.CacheProducts:
                    await _siteCrawlerService.StartCaching();
                    break;
                case CrawlRequest.FetchNoonLinks:
                    await _siteCrawlerService.FetchNoonLinks();
                    break;
                case CrawlRequest.FetchSharafDGLinks:
                    await _siteCrawlerService.FetchSharafDGLinks();
                    break;
                case CrawlRequest.CrawlSuppliers:
                    await _siteCrawlerService.CrawlSuppliers();
                    break;
            }
        }
    }


    public record struct CrawlerQuery : IRequest
    {
        public CrawlRequest Request { get; set; }

        public CrawlerQuery(CrawlRequest request)
        {
            Request = request;
        }
    }


    public enum CrawlRequest{
        FetchNoonLinks = 1,
        FetchSharafDGLinks = 2,
        CrawlSuppliers = 3,
        CacheProducts = 4,
    }

}
