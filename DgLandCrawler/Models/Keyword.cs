namespace DgLandCrawler.Models
{
    public class Keyword
    {
        public int Id { get; set; }
        public string value { get; set; }   

        public int DGProductId { get; set; }
        public DGProductData DGProduct { get; set; }
    }
}