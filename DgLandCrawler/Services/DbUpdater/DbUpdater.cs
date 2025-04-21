

using AutoMapper;
using DgLandCrawler.Data.Repository;
using DgLandCrawler.Helper;
using DgLandCrawler.Models;
using Microsoft.EntityFrameworkCore;
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

        public async Task UpdateProducts() 
        {
            var list = GetSiteProducts().ToList();
            var db_products = await _productRepository.GetListExlcudeMeta().ToListAsync();
            foreach(var wordpress_pd in list)
            {
                var product_from_db = db_products.FirstOrDefault(x=>x.SKU == wordpress_pd.SKU);
                if(product_from_db != null)
                {
                    wordpress_pd.Name = product_from_db.Name;
                    wordpress_pd.SKU = product_from_db.SKU;  
                    wordpress_pd.SalePrice = product_from_db.SalePrice;
                    wordpress_pd.RegularPrice = product_from_db.RegularPrice;
                    wordpress_pd.DgLandId = product_from_db.DgLandId;
                }
                else
                {
                    var newProduct = new DGProductData
                    {
                        DgLandId = wordpress_pd.DgLandId,
                        Category = wordpress_pd.Category,
                        Name = wordpress_pd.Name,
                        SKU = wordpress_pd.SKU,
                        RegularPrice = wordpress_pd.RegularPrice,
                        SalePrice = wordpress_pd.SalePrice,
                    };

                    db_products.Add(newProduct);
                }
            };


            await _productRepository.BulkUpdate(db_products);
        }
    }

    public interface IDbUpdater
    {
        Task UpdateMissingProducts();
        Task UpdateProducts();

        IEnumerable<DGProductData> GetSiteProducts();
    }
}
