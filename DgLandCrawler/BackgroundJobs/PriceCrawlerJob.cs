using DgLandCrawler.Services;
using DgLandCrawler.Services.SiteCrawler;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DgLandCrawler.BackgroundJobs
{
    public class PriceCrawlerJob
    {
        private readonly IServiceProvider _serviceProvider;

        public PriceCrawlerJob(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task ExecuteAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var siteCrawlerService = scope.ServiceProvider.GetRequiredService<ISiteCrawlerService>();
            var jobControlService = scope.ServiceProvider.GetRequiredService<JobControlService>();

            try
            {
                if (!jobControlService.Should_Run)
                {
                    Log.Information("PriceCrawlerJob >> Job execution skipped because it was stopped.");
                    return;
                }

                await siteCrawlerService.CrawlSuppliers();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PriceCrawlerJob >> Exception occurred during execution.");
            }
            finally
            {
                Log.Information("PriceCrawlerJob >> End");
            }
        }
    }
}
