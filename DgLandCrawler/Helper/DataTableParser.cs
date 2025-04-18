using CsvHelper;
using System.Data;
using System.Globalization;
using CsvHelper.Configuration;
using System.Linq;
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
                var pivotTable = new DataTable();
                    pivotTable.Columns.Add("Name", typeof(string));
                    pivotTable.Columns.Add("AvgRegularPrice", typeof(decimal));
                    pivotTable.Columns.Add("AvgSalesPrice", typeof(decimal));

                // Group by Name and calculate average of RegularPrice
                var res =  datatable.AsEnumerable()
                     .Where(row => !string.IsNullOrWhiteSpace(row["Regular price"].ToString())) // Filter out rows with null or empty prices
                     .GroupBy(row => new {
                         Category = row.Field<string>("Categories"),
                         Name = row.Field<string>("Name"),
                         SKU = row.Field<string>("SKU")
                     })
                     .Select(group => new DGProductData(group.Key.Category!, group.Key.Name!, group.Key.SKU! ,Convert.ToInt32(group.Average(row => Convert.ToInt32(row["Regular price"]))), Convert.ToInt32(group.Average(row => Convert.ToInt32(row["Sales price"])))));

                return res;

            }catch(Exception ex)
            {

            }

            return Enumerable.Empty<DGProductData>();
        }
    }
}
