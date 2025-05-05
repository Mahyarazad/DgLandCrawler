using DgLandCrawler.Abstraction;

namespace DgLandCrawler.Models.DTO
{
    public record struct PriceViewModel
    {
        public int DgLandId { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string? Category { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? BaseUrl { get; set; }
        public string? Supplier { get; set; }
        public string? Price { get; set; }
        public int? RegularPrice { get; set; }
        public int? SalePrice { get; set; }
        public int? PriceGap { get; set; }

    }
}
