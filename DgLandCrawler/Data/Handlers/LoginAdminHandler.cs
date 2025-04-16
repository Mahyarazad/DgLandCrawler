using DgLandCrawler.Models.DTO;
using DgLandCrawler.Services.DbUpdater;
using DgLandCrawler.Services.SiteCrawler;
using MediatR;

namespace DgLandCrawler.Data.Handlers
{

    public class LoginAdminHadnler(ISiteCrawlerService siteCrawlerService, IDbUpdater dbUpdater) : IRequestHandler<AdminPanelCredential>
    {
        private readonly ISiteCrawlerService _siteCrawlerService = siteCrawlerService;

        public async Task Handle(AdminPanelCredential request, CancellationToken cancellationToken)
        {
            await _siteCrawlerService.AdminLogin(request);
        }
    }


}
