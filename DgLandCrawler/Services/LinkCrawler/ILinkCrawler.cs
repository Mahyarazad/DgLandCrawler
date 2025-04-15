namespace DgLandCrawler.Services.LinkCrawler
{
    public interface ILinkCrawler
    {
        void Dispose();
        Task GetGoogleProductLinks();

    }
}
