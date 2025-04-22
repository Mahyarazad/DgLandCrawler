using DgLandCrawler.Data.Handlers;
using DgLandCrawler.Models.DTO;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using CsvHelper;
using static OpenQA.Selenium.BiDi.Modules.BrowsingContext.Locator;
using System.Globalization;
using DgLandCrawler.Services.SiteCrawler;
using DgLandCrawler.Helper;

namespace Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
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

        app.MapGet("/product/fetch-noon-links", async ([FromServices] IMediator mediator) =>
        {
            try
            {
                await mediator.Send(new CrawlerQuery(CrawlRequest.FetchNoonLinks));
                return Results.Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
        });


        app.MapGet("/product/fetch-sharafdg-links", async ([FromServices] IMediator mediator) =>
        {
            try
            {
                await mediator.Send(new CrawlerQuery(CrawlRequest.FetchSharafDGLinks));
                return Results.Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
        });

        app.MapGet("/product/crawl-suppliers", async ([FromServices] IMediator mediator) =>
        {
            try
            {
                await mediator.Send(new CrawlerQuery(CrawlRequest.CrawlSuppliers));
                return Results.Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
        });

        app.MapPost("/product/update-products", async ([FromServices] IMediator mediator, AdminPanelCredential credential) =>
        {
            try
            {
                await mediator.Send(new SyncDatabaseRequest(credential, SyncRequest.UpdateProducts));
                return Results.Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
        });

        app.MapPost("/product/add-new-products", async ([FromServices] IMediator mediator, AdminPanelCredential credential) =>
        {
            try
            {
                await mediator.Send(new SyncDatabaseRequest(credential, SyncRequest.AddMissingProducts));
                return Results.Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
        });

        app.MapPost("/product/add-product-attribute", async ([FromServices] IChatGPTService gptService, HttpRequest request) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest("Form content type is required.");

            var form = await request.ReadFormAsync();

            var file = form.Files.GetFile("file");

            if (file == null || file.Length == 0)
                return Results.BadRequest("CSV file is required.");

            using var stream = file.OpenReadStream();

            using var reader = new StreamReader(stream);

            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            List<WordpressProduct> records = await CSVHelper.GetWordPressProducts(csv);

            List<WordpressProduct> updatedRecords = await gptService.GetProductAttributes(records);

            MemoryStream resultstream = await CSVHelper.ExportWordpressProducts(updatedRecords);

            resultstream.Position = 0;

            return Results.File(resultstream, "text/csv", "wordpress-products.csv");


        }).Accepts<IFormFile>("multipart/form-data", "file");
    }

    
}
