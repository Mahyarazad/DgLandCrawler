using HtmlAgilityPack;

namespace DgLandCrawler.Helper
{
    public static class LinkParser
    {
        public static List<string> ExtractLinks(string pageSource)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(pageSource);

            // Extract all links that start with 'https://dgland.ae'
            return doc.DocumentNode.Descendants("a")
                                        .Where(a => a.Attributes["href"] != null &&
                                                    a.Attributes["href"].Value.StartsWith("https://dgland.ae/product"))
                                        .Select(a => a.Attributes["href"].Value)
                                        .ToList();
        }

        public static List<string> GetSiteUrl(string pageSource)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(pageSource);

            // Extract all links that start with 'https://dgland.ae'
            return doc.DocumentNode.Descendants("a")
                                        .Where(a => a.Attributes["href"] != null &&
                                                    a.Attributes["href"].Value.StartsWith("https://dgland.ae/"))
                                        .Select(a => a.Attributes["href"].Value)
                                        .ToList();


        }
    }
}
