using DgLandCrawler.BackgroundJobs;
using DgLandCrawler.Services;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;


namespace DgLandCrawler.Endpoints
{
    public static class BackgroundJobEndpoints
    {
        public static void MapBackgroundJobEndpoints(this IEndpointRouteBuilder app)
        {

            var backgroundJobs = app.MapGroup("backgroundjobs");

            backgroundJobs.WithTags("Background Jobs");



            backgroundJobs.MapGet("start-fetching-prices",
                (IRecurringJobManager recurringJobManager, JobControlService jobControlService) =>
            {
                jobControlService.StartJob();

                recurringJobManager.AddOrUpdate<PriceCrawlerJob>(
                    "price-crawler-job",
                    job => job.ExecuteAsync(),
                    Cron.Minutely
                );

                return Results.Ok("Scheduled PriceCrawlerJob");
            });

            backgroundJobs.MapGet("stop-fetching-prices", (JobControlService jobControlService) =>
            {
                jobControlService.StopJob();
                return Results.Ok("Job stopped!");
            });
        }

    }
}
