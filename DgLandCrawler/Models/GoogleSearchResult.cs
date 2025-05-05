namespace DgLandCrawler.Models
{
    public class GoogleSearchResult
    {
        public int GoogleId { get; set; }
        public string Title { get; set; }
        public string Price { get; set; }
        public string? PreviousPrice { get; set; }
        public string? BaseUrl { get; set; }
        public string Supplier { get; set; }
        public DateTime CreationTime { get; set; }

        public DateTime UpdateTime { get; set; }

        // Navigation Property
        public DGProductData DGProduct { get; set; }
        public int DGProductId { get; set; }
    }
}
