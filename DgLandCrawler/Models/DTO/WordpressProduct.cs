

using CsvHelper.Configuration.Attributes;

namespace DgLandCrawler.Models.DTO
{
    public class WordpressProduct
    {
        public string? ID { get; set; }
        public string? Type { get; set; }
        public string? SKU { get; set; }
        public string? GTIN_UPC_EAN_ISBN { get; set; }
        public string? Name { get; set; }
        public string? Published { get; set; }
        public string? IsFeatured { get; set; }
        public string? VisibilityInCatalog { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string? DateSalePriceStarts { get; set; }
        public string? DateSalePriceEnds { get; set; }
        public string? TaxStatus { get; set; }
        public string? TaxClass { get; set; }
        public string? InStock { get; set; }
        public string? Stock { get; set; }
        public string? LowStockAmount { get; set; }
        public string? BackordersAllowed { get; set; }
        public string? SoldIndividually { get; set; }
        public string? WeightKg { get; set; }
        public string? LengthCm { get; set; }
        public string? WidthCm { get; set; }
        public string? HeightCm { get; set; }
        public string? AllowCustomerReviews { get; set; }
        public string? PurchaseNote { get; set; }
        public string? SalePrice { get; set; }
        public string? RegularPrice { get; set; }
        public string? Categories { get; set; }
        public string? Tags { get; set; }
        public string? ShippingClass { get; set; }
        public string? Images { get; set; }
        public string? DownloadLimit { get; set; }
        public string? DownloadExpiryDays { get; set; }
        public string? Parent { get; set; }
        public string? GroupedProducts { get; set; }
        public string? Upsells { get; set; }
        public string? CrossSells { get; set; }
        public string? ExternalURL { get; set; }
        public string? ButtonText { get; set; }
        public string? Position { get; set; }
        public string? Brands { get; set; }
        // Attributes 1 to 35
        [Ignore]
        public List<ProductAttribute> Attributes { get; set; } = new();
    }

    public class ProductAttribute
    {
        public string? Name { get; set; }
        public string? Values { get; set; }
        public string? Visible { get; set; }
        public string? Global { get; set; }
        public string? Default { get; set; }
    }
}
