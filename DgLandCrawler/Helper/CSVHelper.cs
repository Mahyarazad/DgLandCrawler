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

    }
}
