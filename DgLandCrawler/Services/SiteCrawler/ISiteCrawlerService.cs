using DgLandCrawler.Models.DTO;

namespace DgLandCrawler.Services.SiteCrawler
{
    public interface ISiteCrawlerService
    {
        Task UpdateAltImages();
        Task StartCaching();
        Task PostProductReview();
        Task PostPDP();
        Task DownloadDGLandProducts(AdminPanelCredential credential);
        Task CrawlSuppliers();
        Task FetchNoonLinks();
        Task FetchSharafDGLinks();
        Task GenerateCSVFile();
    }
}
