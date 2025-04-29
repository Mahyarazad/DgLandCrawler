using DgLandCrawler.Models;
using DgLandCrawler.Models.DTO;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace DgLandCrawler.Data.Repository
{
    public class DGProductRepository(ApplicationDbContext context) : IDGProductRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task AddAsync(DGProductData value, CancellationToken cancellationToken = default)
        {
            var entity = await _context.DGProducts.FirstOrDefaultAsync(x => x.Name.Contains(value.Name));
            if(entity != null)
            {
                entity.GoogleResult = value.GoogleResult;
                _context.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                await _context.AddAsync(value, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<DGProductData> GetByIdAsync(int id) => await _context.DGProducts.Include(x => x.GoogleResult).FirstOrDefaultAsync(x => x.Id == id);

        async Task<List<DGView>> IDGProductRepository.GetViewlist(CancellationToken cancellationToken)
        {
            var result = await (from product in _context.DGProducts
                                join google in _context.GoogleSearchResults
                                on product.Id equals google.DGProductId
                                //into temp from google in temp.DefaultIfEmpty()
                                select new DGView
                                {
                                    Id = product.Id,
                                    GoogleId = google.GoogleId,
                                    Name = product.Name,
                                    Supplier = google.Supplier,
                                    Url = string.IsNullOrEmpty(google.BaseUrl) ? string.Empty : google.BaseUrl
                                }).ToListAsync(cancellationToken);

            return result;
        }

        public async Task<IList<DGProductData>> GetList(CancellationToken cancellationToken)
        {
            return await _context.DGProducts.Include(x => x.GoogleResult).ToListAsync();
        }

        public IQueryable<DGProductData> GetListExlcudeMeta()
        {
            return  _context.DGProducts;
        }

        public async Task UpdateGoogleSearchResultsAsync(int id, ICollection<GoogleSearchResult> searchResults, CancellationToken cancellationToken = default)
        {
            var product = await _context.DGProducts.FindAsync([id], cancellationToken);
            if(product != null)
            {
                _context.DGProducts.Attach(product);
                product.GoogleResult = searchResults;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<DGView>> GetUnlistedGoogleResult(IEnumerable<DGProductData> data)
        {
            var list = await _context.DGProducts.Include(_ => _.GoogleResult)
               .ToListAsync();

            var dbNamelist = list.Select(x => new DGView { Name = x.Name, Id = x.Id });

            var incomingData = data.Select(x => new DGView { Name = x.Name, Id = x.Id, Category = x.Category, RegularPrice = x.RegularPrice, SKU = x.SKU });

            return incomingData.Where(x => !dbNamelist.Any(i => i.Name == x.Name));
        }

        public async Task BulkUpdate(IEnumerable<DGProductData> data) => await _context.BulkUpdateAsync(data);

        public async Task BulkInsertAsync(ICollection<DGProductData> list, CancellationToken cancellationToken = default)
        {
            await _context.BulkInsertAsync(list, new BulkConfig { BatchSize = 100 }, cancellationToken: cancellationToken);
        }

        public async Task<IList<DGProductData>> GetUnCrawledGoolgeList(CancellationToken cancellationToken = default)
        {
            return await _context.Set<DGProductData>()
                    .Include(x => x.GoogleResult)
                    .Where(x => x.GoogleResult.Count == 0).ToListAsync(cancellationToken: cancellationToken);
        }

        public async Task Update(DGProductData data)
        {
            await _context.AddAsync(data);

            await _context.SaveChangesAsync();
        }

        public async Task<DGView> GetById(int id, CancellationToken cancellationToken = default)
        {
            var result =  await _context.Set<DGProductData>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if(result != null)
            {
                return new DGView
                {
                    Id = id,
                    Name = result.Name,
                    Category = result.Category
                };
            }

            return new DGView();
        }


        public async Task<IList<PriceViewModel>> GetUpdatedCrawledPriceList(CancellationToken token = default)
        {
            using var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "dbo.GetUpdatedCrawledPriceList";

            await _context.Database.OpenConnectionAsync(token);
            var products = new List<PriceViewModel>();

            using var reader = await cmd.ExecuteReaderAsync(token);
            while (await reader.ReadAsync(token)) {
                int i = 0;
                products.Add(new PriceViewModel
                {
                    DgLandId = reader.GetInt32(0),
                    CreationTime = reader.GetDateTime(1),
                    Category = reader.GetString(2),
                    Name = reader.GetString(3),
                    Title = reader.GetString(4),
                    BaseUrl = reader.GetString(5),
                    Supplier = reader.GetString(6),
                    Price = reader.GetString(7),
                    RegularPrice = reader.GetInt32(8),
                    SalePrice = reader.GetInt32(9)
                });
            }

            return products;

        }
    }
}
