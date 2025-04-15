namespace DgLandCrawler.Models
{
    public class GptReview
    {
        public string Message { get; set; }
        public string PersonName { get; set; }
        public string Email { get; set; }
        public int Score { get; set; }

    }

    public class DGProduct
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public List<string> ImageUrls { get; set; } = new();

    }
}
