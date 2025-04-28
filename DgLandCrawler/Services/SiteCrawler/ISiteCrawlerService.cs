using DgLandCrawler.Models.DTO;
using static DgLandCrawler.Services.SiteCrawler.SiteCrawlerService;

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
        Task FetchSupplierLinks(Supplier supplier);
        Task GenerateCSVFile();
    }
}
