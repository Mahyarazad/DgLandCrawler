using DgLandCrawler.Services.SiteCrawler;
using MediatR;

namespace DgLandCrawler.Data.Handlers
{
    public class CrawlerHandler(ISiteCrawlerService siteCrawlerService) : IRequestHandler<CrawlerQuery>
    {
        private readonly ISiteCrawlerService _siteCrawlerService = siteCrawlerService;
        public async Task Handle(CrawlerQuery request, CancellationToken cancellationToken)
        {
            await _siteCrawlerService.CrawlThreeMainSupplier();
        }
    }


    public record struct CrawlerQuery() : IRequest 
    {
        
    }
}
