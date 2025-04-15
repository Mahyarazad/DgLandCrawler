using DgLandCrawler.Models;

namespace DgLandCrawler.Services.GptClient
{
    public interface IGptClient
    {
        Task<Root?> GetResultFromGPT(string prompt);
    }
}