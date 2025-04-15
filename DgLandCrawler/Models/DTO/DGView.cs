using DgLandCrawler.Abstraction;

namespace DgLandCrawler.Models.DTO
{
    public record struct DGView : ICachedQuery<DGView>
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Supplier { get; set; }
        public string? Url { get; set; }
        public string? Category { get; set; }
        public int? RegularPrice { get; set; }
        public string? SKU { get; set; }
        public int? GoogleId { get; set; }

        public string Key => $"dg-view-{Id}";

        public TimeSpan? Expiration => null;

    }
}
