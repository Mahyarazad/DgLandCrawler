using CsvHelper;
using System.Data;
using System.Globalization;
using CsvHelper.Configuration;
using DgLandCrawler.Models;

namespace DgLandCrawler.Helper
{
    public static partial class DataTableParser
    {
        public static DataTable ReadCsvToDataTable(string filePath)
        {
            var dataTable = new DataTable();
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null
                };
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, config);
                using var dr = new CsvDataReader(csv);
                dataTable.Load(dr);

            }
            catch(Exception ex)
            {

            }

            return dataTable;
        }


        public static IEnumerable<DGProductData> OurCustomData(DataTable datatable)
        {
            try
            {
                return datatable .AsEnumerable()
                                .Where(row => Convert.ToInt32(row["Published"].ToString()) != -1)
                                .Select(row => new DGProductData(
                                    Convert.ToInt32(row["ID"].ToString()),
                                    string.IsNullOrEmpty(row["Categories"].ToString()) ? "" : row["Categories"].ToString(),
                                    string.IsNullOrEmpty(row["Name"].ToString()) ? "" : row["Name"].ToString(),
                                    string.IsNullOrEmpty(row["SKU"].ToString()) ? "" : row["SKU"].ToString(),
                                    string.IsNullOrEmpty(row["Regular price"].ToString()) ? 0 : Convert.ToInt32(row["Regular price"].ToString()),
                                    string.IsNullOrEmpty(row["Sale price"].ToString()) ? 0 : Convert.ToInt32(row["Sale price"].ToString())
                                ));

            }catch(Exception ex)
            {

            }

            return Enumerable.Empty<DGProductData>();
        }
    }
}
