using CsvHelper;
using CsvHelper.Configuration;
using DgLandCrawler.Models.DTO;
using System.Globalization;
using System.Text;

namespace DgLandCrawler.Helper
{
    public static class CSVHelper
    {
        public static string GenerateCsv<T>(IEnumerable<T> items)
        {
            var sb = new StringBuilder();
            var properties = typeof(T).GetProperties();

            // Header
            sb.AppendLine(string.Join(",", properties.Select(p => p.Name)));

            // Rows
            foreach (var item in items)
            {
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(item, null);
                    // Escape commas and quotes
                    var stringValue = value?.ToString()?.Replace("\"", "\"\"") ?? "";
                    return $"\"{stringValue}\"";
                });

                sb.AppendLine(string.Join(",", values));
            }

            return sb.ToString();
        }

        public static async Task<List<WordpressProduct>> GetWordPressProducts(CsvReader csv)
        {
            var records = new List<WordpressProduct>();
            await foreach (var row in csv.GetRecordsAsync<dynamic>())
            {
                var dict = (IDictionary<string, object>)row;

                var product = new WordpressProduct
                {
                    ID = dict["ID"] is null ? string.Empty : dict["ID"].ToString(),
                    Type = dict["Type"] is null ? string.Empty : dict["Type"].ToString(),
                    SKU = dict["SKU"] is null ? string.Empty : dict["SKU"].ToString(),
                    GTIN_UPC_EAN_ISBN = dict["GTIN, UPC, EAN, or ISBN"] is null ? string.Empty : dict["GTIN, UPC, EAN, or ISBN"].ToString(),
                    Name = dict["Name"] is null ? string.Empty : dict["Name"].ToString(),
                    Published = dict["Published"] is null ? string.Empty : dict["Published"].ToString(),
                    IsFeatured = dict["Is featured?"] is null ? string.Empty : dict["Is featured?"].ToString(),
                    VisibilityInCatalog = dict["Visibility in catalog"] is null ? string.Empty : dict["Visibility in catalog"].ToString(),
                    ShortDescription = dict["Short description"] is null ? string.Empty : dict["Short description"].ToString(),
                    Description = dict["Description"] is null ? string.Empty : dict["Description"].ToString(),
                    DateSalePriceStarts = dict["Date sale price starts"] is null ? string.Empty : dict["Date sale price starts"].ToString(),
                    DateSalePriceEnds = dict["Date sale price ends"] is null ? string.Empty : dict["Date sale price ends"].ToString(),
                    TaxStatus = dict["Tax status"] is null ? string.Empty : dict["Tax status"].ToString(),
                    TaxClass = dict["Tax class"] is null ? string.Empty : dict["Tax class"].ToString(),
                    InStock = dict["In stock?"] is null ? string.Empty : dict["In stock?"].ToString(),
                    Stock = dict["Stock"] is null ? string.Empty : dict["Stock"].ToString(),
                    LowStockAmount = dict["Low stock amount"] is null ? string.Empty : dict["Low stock amount"].ToString(),
                    BackordersAllowed = dict["Backorders allowed?"] is null ? string.Empty : dict["Backorders allowed?"].ToString(),
                    SoldIndividually = dict["Sold individually?"] is null ? string.Empty : dict["Sold individually?"].ToString(),
                    WeightKg = dict["Weight (kg)"] is null ? string.Empty : dict["Weight (kg)"].ToString(),
                    LengthCm = dict["Length (cm)"] is null ? string.Empty : dict["Length (cm)"].ToString(),
                    WidthCm = dict["Width (cm)"] is null ? string.Empty : dict["Width (cm)"].ToString(),
                    HeightCm = dict["Height (cm)"] is null ? string.Empty : dict["Height (cm)"].ToString(),
                    AllowCustomerReviews = dict["Allow customer reviews?"] is null ? string.Empty : dict["Allow customer reviews?"].ToString(),
                    PurchaseNote = dict["Purchase note"] is null ? string.Empty : dict["Purchase note"].ToString(),
                    SalePrice = dict["Sale price"] is null ? string.Empty : dict["Sale price"].ToString(),
                    RegularPrice = dict["Regular price"] is null ? string.Empty : dict["Regular price"].ToString(),
                    Categories = dict["Categories"] is null ? string.Empty : dict["Categories"].ToString(),
                    Tags = dict["Tags"] is null ? string.Empty : dict["Tags"].ToString(),
                    ShippingClass = dict["Shipping class"] is null ? string.Empty : dict["Shipping class"].ToString(),
                    Images = dict["Images"] is null ? string.Empty : dict["Images"].ToString(),
                    DownloadLimit = dict["Download limit"] is null ? string.Empty : dict["Download limit"].ToString(),
                    DownloadExpiryDays = dict["Download expiry days"] is null ? string.Empty : dict["Download expiry days"].ToString(),
                    Parent = dict["Parent"] is null ? string.Empty : dict["Parent"].ToString(),
                    GroupedProducts = dict["Grouped products"] is null ? string.Empty : dict["Grouped products"].ToString(),
                    Upsells = dict["Upsells"] is null ? string.Empty : dict["Upsells"].ToString(),
                    CrossSells = dict["Cross-sells"] is null ? string.Empty : dict["Cross-sells"].ToString(),
                    ExternalURL = dict["External URL"] is null ? string.Empty : dict["External URL"].ToString(),
                    ButtonText = dict["Button text"] is null ? string.Empty : dict["Button text"].ToString(),
                    Position = dict["Position"] is null ? string.Empty : dict["Position"].ToString(),
                    Brands = dict["Brands"] is null ? string.Empty : dict["Brands"].ToString()

                };


                for (int i = 1; i <= 2; i++)
                {
                    var attr = new ProductAttribute();
                    if (i == 1 || i == 2)
                    {
                        attr.Name = dict[$"Attribute {i} name"] == null ? string.Empty : dict[$"Attribute {i} name"].ToString();
                        attr.Values = dict[$"Attribute {i} value(s)"] == null ? string.Empty : dict[$"Attribute {i} value(s)"].ToString();
                        attr.Visible = dict[$"Attribute {i} visible"] == null ? string.Empty : dict[$"Attribute {i} visible"].ToString();
                        attr.Global = dict[$"Attribute {i} global"] == null ? string.Empty : dict[$"Attribute {i} global"].ToString();
                        attr.Default = dict[$"Attribute {i} default"] == null ? string.Empty : dict[$"Attribute {i} default"].ToString();
                    }
                    else
                    {
                        attr.Name = dict[$"Attribute {i} name"] == null ? string.Empty : dict[$"Attribute {i} name"].ToString();
                        attr.Values = dict[$"Attribute {i} value(s)"] == null ? string.Empty : dict[$"Attribute {i} value(s)"].ToString();
                        attr.Visible = dict[$"Attribute {i} visible"] == null ? string.Empty : dict[$"Attribute {i} visible"].ToString();
                        attr.Global = dict[$"Attribute {i} global"] == null ? string.Empty : dict[$"Attribute {i} global"].ToString();
                    }

                    product.Attributes.Add(attr);
                }

                records.Add(product);
            }

            return records;
        }

        public static async Task<MemoryStream> ExportWordpressProducts(List<WordpressProduct> products)
        {
            var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream, leaveOpen: true);
            await using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) {
                Delimiter = "\t"
            });

            var headers = new List<string>
            {
                "ID", "Type", "SKU", "GTIN, UPC, EAN, or ISBN", "Name", "Published", "Is featured?", "Visibility in catalog",
                "Short description", "Description", "Date sale price starts", "Date sale price ends", "Tax status", "Tax class",
                "In stock?", "Stock", "Low stock amount", "Backorders allowed?", "Sold individually?", "Weight (kg)", "Length (cm)",
                "Width (cm)", "Height (cm)", "Allow customer reviews?", "Purchase note", "Sale price", "Regular price",
                "Categories", "Tags", "Shipping class", "Images", "Download limit", "Download expiry days", "Parent",
                "Grouped products", "Upsells", "Cross-sells", "External URL", "Button text", "Position", "Brands"
            };

            for (int i = 1; i <= 35; i++)
            {
                headers.Add($"Attribute {i} name");
                headers.Add($"Attribute {i} value(s)");
                headers.Add($"Attribute {i} visible");
                headers.Add($"Attribute {i} global");
                if(i == 1 || i == 2)
                {
                    headers.Add($"Attribute {i} default");
                }
            }

            foreach (var header in headers)
                csv.WriteField(header);
            await csv.NextRecordAsync();

            foreach (var p in products)
            {
                var row = new List<string?>
                {
                    p.ID, p.Type, p.SKU, p.GTIN_UPC_EAN_ISBN, p.Name, p.Published, p.IsFeatured, p.VisibilityInCatalog,
                    p.ShortDescription, p.Description, p.DateSalePriceStarts, p.DateSalePriceEnds, p.TaxStatus, p.TaxClass,
                    p.InStock, p.Stock, p.LowStockAmount, p.BackordersAllowed, p.SoldIndividually, p.WeightKg, p.LengthCm,
                    p.WidthCm, p.HeightCm, p.AllowCustomerReviews, p.PurchaseNote, p.SalePrice, p.RegularPrice,
                    p.Categories, p.Tags, p.ShippingClass, p.Images, p.DownloadLimit, p.DownloadExpiryDays, p.Parent,
                    p.GroupedProducts, p.Upsells, p.CrossSells, p.ExternalURL, p.ButtonText, p.Position, p.Brands
                };

                for (int i = 0; i < 35; i++)
                {
                    if (p.Attributes.Count > i)
                    {
                        var attr = p.Attributes[i];
                        row.Add(attr.Name ?? "");
                        row.Add(attr.Values ?? "");
                        row.Add(attr.Visible ?? "");
                        row.Add(attr.Global ?? "");
                        if (i == 1 || i == 2)
                        {
                            row.Add(attr.Default ?? "");
                        }
                            
                    }
                }

                foreach (var field in row)
                    csv.WriteField(field);

                await csv.NextRecordAsync();
            }

            await writer.FlushAsync();
            stream.Position = 0; // Reset stream position for reading/download
            return stream;
        }

        private static string? SanitizeForCsv(string? input)
        {
            if (input == null) return null;
            return input.Replace("\"", "\"\"");
        }
    }
}
