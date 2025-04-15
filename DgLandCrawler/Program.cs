using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using DgLandCrawler.Services.SiteCrawler;
using DgLandCrawler.Services.GptClient;
using Microsoft.EntityFrameworkCore;
using DgLandCrawler.Data;
using DgLandCrawler.Services.DbUpdater;
using DgLandCrawler.Models;
using DgLandCrawler.Services.LinkCrawler;
using DgLandCrawler.Abstraction.Behaviour;
using DgLandCrawler.Data.Repository;
using Microsoft.AspNetCore.Builder;
using MediatR;
using Microsoft.AspNetCore.Http;
using DgLandCrawler.Data.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Reflection;
internal class Program
{
    private static void Main(string[] args)
    {
        var cgfbuilder = new ConfigurationBuilder();
        var directory = new DirectoryInfo(Environment.CurrentDirectory);

        cgfbuilder.SetBasePath(directory.Parent.Parent.Parent.FullName)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(cgfbuilder.Build())
            .Enrich.FromLogContext()
            .WriteTo.Console(
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpClient();
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<ICacheService, CacheService>();
            var connectionString = builder.Configuration.GetConnectionString("MSSqlServer");

            builder.Services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(typeof(Program).Assembly);
                config.AddOpenBehavior(typeof(QueryCachingPipelineBehaviour<,>));
            });

            builder.Services.AddSingleton<IGptClient, GptClient>();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            builder.Services.Configure<AppConfig>(builder.Configuration);
            builder.Services.AddTransient<IDGProductRepository, DGProductRepository>();
            builder.Services.AddTransient<ISiteCrawlerService, SiteCrawlerService>();
            builder.Services.AddTransient<IDbUpdater, DbUpdater>();
            builder.Services.AddTransient<ILinkCrawler, LinkCrawler>();

            // Add Swagger services
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
            });

            var app = builder.Build();


            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API v1");
                options.RoutePrefix = string.Empty; // Set the Swagger UI at the root URL
            });

            app.MapGet("/product/{id:int}", async ([FromServices] IMediator mediator, int id) =>
            {
                try
                {
                    var product = await mediator.Send(new DGProductQuery(id));
                    return Results.Ok(product);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            });

            app.MapGet("/product/run-crawler", async ([FromServices] IMediator mediator) =>
            {
                try
                {
                    await mediator.Send(new CrawlerQuery());
                    return Results.Ok();
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            });

            app.MapGet("/product/sync-database", async ([FromServices] IMediator mediator) =>
            {
                try
                {
                    await mediator.Send(new SyncDatabaseRequest());
                    return Results.Ok();
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            });

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start"); // Log fatal errors
        }
        finally
        {
            Log.CloseAndFlush(); // Ensure logs are flushed
        }
    }
}
