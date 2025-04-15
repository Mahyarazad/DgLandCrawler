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
        Task CrawlThreeMainSupplier();

        Task GetProductSearchKeywords();
        void Dispose();
    }
}
