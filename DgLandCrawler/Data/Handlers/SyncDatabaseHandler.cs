using DgLandCrawler.Models.DTO;
using DgLandCrawler.Services.DbUpdater;
using DgLandCrawler.Services.SiteCrawler;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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


    public record struct SyncDatabaseRequest(AdminPanelCredential Credential, SyncRequest SyncRequest) : IRequest
    {
        public AdminPanelCredential Credential { get; set; } = Credential;
        public SyncRequest SyncRequest { get; set; } = SyncRequest;
    }

    public enum SyncRequest
    {
        UpdateProducts = 1,
        AddMissingProducts = 2,
    }

    public class FileUploadRequest
    {
        [FromForm(Name = "file")]
        public IFormFile File { get; set; }

    }
}
