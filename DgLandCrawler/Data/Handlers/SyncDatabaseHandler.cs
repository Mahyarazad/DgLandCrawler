using DgLandCrawler.Models.DTO;
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
            await _siteCrawlerService.DownloadDGLandProducts(request.Credential);

            switch (request.SyncRequest)
            {
                case SyncRequest.UpdateProducts:
                    await _dbUpdater.UpdateProducts();
                    break;
                case SyncRequest.AddMissingProducts:
                    await _dbUpdater.UpdateMissingProducts();
                    break;
            }
        }
    }


    public record struct SyncDatabaseRequest(AdminPanelCredential credential, SyncRequest syncRequest) : IRequest
    {
        public AdminPanelCredential Credential { get; set; } = credential;
        public SyncRequest SyncRequest { get; set; } = syncRequest;
    }

    public enum SyncRequest
    {
        UpdateProducts = 1,
        AddMissingProducts = 2,
    }
}
