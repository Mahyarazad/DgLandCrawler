using DgLandCrawler.Services.DbUpdater;
using DgLandCrawler.Services.SiteCrawler;
using MediatR;

namespace DgLandCrawler.Data.Handlers
{

    public class SyncDatabaseHandler(ISiteCrawlerService siteCrawlerService, IDbUpdater dbUpdater) : IRequestHandler<SyncDatabaseRequest>
    {
        private readonly ISiteCrawlerService _siteCrawlerService = siteCrawlerService;
        private readonly IDbUpdater _dbUpdater = dbUpdater;
        public async Task Handle(SyncDatabaseRequest request, CancellationToken cancellationToken)
        {
            await _siteCrawlerService.DownloadDGLandProducts();
            await _dbUpdater.UpdateMissingProducts();
        }
    }


    public record struct SyncDatabaseRequest() : IRequest
    {

    }
}
