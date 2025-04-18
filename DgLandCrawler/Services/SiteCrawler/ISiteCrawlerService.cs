using DgLandCrawler.Models.DTO;

namespace DgLandCrawler.Services.SiteCrawler
{
    public interface ISiteCrawlerService
    {
        Task AdminLogin(AdminPanelCredential credential);
        Task UpdateAltImages();
        Task StartCaching();
        Task PostProductReview();
        Task PostPDP();
        Task DownloadDGLandProducts();
        Task CrawlSuppliers();
        Task FetchNoonLinks();
        Task FetchSharafDGLinks();
        Task GetProductSearchKeywords();
    }
}
