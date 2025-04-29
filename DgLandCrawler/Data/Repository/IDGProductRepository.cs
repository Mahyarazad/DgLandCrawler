using DgLandCrawler.Models;
using DgLandCrawler.Models.DTO;

namespace DgLandCrawler.Data.Repository
{
    public interface IDGProductRepository
    {
        Task AddAsync(DGProductData value, CancellationToken cancellationToken = default);
        Task BulkInsertAsync(ICollection<DGProductData> list, CancellationToken cancellationToken = default);
        Task UpdateGoogleSearchResultsAsync(int id, ICollection<GoogleSearchResult> searchResults, CancellationToken cancellationToken = default);
        Task<List<DGView>> GetViewlist(CancellationToken cancellationToken = default);

        Task<DGView> GetById(int id, CancellationToken cancellationToken = default);
        Task<IList<DGProductData>> GetList(CancellationToken cancellationToken = default);
        Task<IList<DGProductData>> GetUnCrawledGoolgeList(CancellationToken cancellationToken = default);
        Task<DGProductData> GetByIdAsync(int id);
        Task<IEnumerable<DGView>> GetUnlistedGoogleResult(IEnumerable<DGProductData> data);
        Task BulkUpdate(IEnumerable<DGProductData> data);
        Task Update(DGProductData data);

        IQueryable<DGProductData> GetListExlcudeMeta();

        Task<IList<PriceViewModel>> GetUpdatedCrawledPriceList(CancellationToken cancellationToken = default);
    }
}
