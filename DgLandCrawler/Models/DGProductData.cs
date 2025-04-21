using System.Xml.Linq;

namespace DgLandCrawler.Models
{
    public class DGProductData
    {
        public DGProductData()
        {
            
        }
        public DGProductData(int id, string category, string name,string sku ,int regularPrice, int salePrice)
        {
            DgLandId = id;
            Category = category;
            Name = name;
            SKU = sku;
            RegularPrice = regularPrice;
            SalePrice = salePrice;
            GoogleResult = [];
            Keywords = [];
        }
        public int Id { get; set; }
        public int DgLandId { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public string Category { get; set; }
        public int RegularPrice { get; set; }
        public int SalePrice { get; set; }
        public virtual ICollection<GoogleSearchResult> GoogleResult { get; set; }
        public virtual ICollection<Keyword> Keywords { get; set; }
        public DateTime CrawlDateTime { get; set; }
    }
}
