

using AutoMapper;
using DgLandCrawler.Data.Repository;
using DgLandCrawler.Helper;
using DgLandCrawler.Models;
using Microsoft.Extensions.Options;

namespace DgLandCrawler.Services.DbUpdater
{
    public class DbUpdater : IDbUpdater
    {
        private readonly AppConfig _config;
        private readonly IDGProductRepository _productRepository;
        private readonly IMapper _mapper;
        public DbUpdater(IDGProductRepository productRepository, IOptions<AppConfig> _appConfig, IMapper mapper)
        {
            _productRepository = productRepository;
            _config = _appConfig.Value;
            _mapper = mapper;
        }
        public IEnumerable<DGProductData> GetSiteProducts()
        {
            var files = Directory.GetFiles(_config.DownloadPath);

            var dataTable = DataTableParser.ReadCsvToDataTable(files.LastOrDefault()!);

            return DataTableParser.OurCustomData(dataTable);
        }

        public async Task UpdateMissingProducts()
        {
            var list = await _productRepository.GetUnlistedGoogleResult(GetSiteProducts());

            var missingList = _mapper.Map<List<DGProductData>>(list.ToList());

            if(missingList.Count != 0)
            {
                await _productRepository.BulkInsertAsync(missingList);
            }
        }

        public async Task UpdateProducts() => await _productRepository.BulkInsertAsync(GetSiteProducts().ToList());
    }
        public interface IDbUpdater
        {
            Task UpdateMissingProducts();
            Task UpdateProducts();

            IEnumerable<DGProductData> GetSiteProducts();
        }
}
