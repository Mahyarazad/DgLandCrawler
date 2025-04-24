using DgLandCrawler.Data.Handlers;
using DgLandCrawler.Models.DTO;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

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
        }).WithSummary("Search Noon and Fetch Related Links")
            .WithDescription("Use product name to fetch product information from the SharafDG (Product Title, Url, and the Price)");


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
        }).WithSummary("Search ShrafDG and Fetch Related Links")
            .WithDescription("Use product name to fetch product information from the Noon (Product Title, Url, and the Price)");

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
        }).WithSummary("Crawl Noon and SharafDG")
            .WithDescription("Use database links to fetch each product by crawling the page, and update the prices in the database. If the driver cannot find the price element it will be set to 0 (out of stock)");

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
        }).WithSummary("Update Products")
            .WithDescription("Updates missing products in the database and also Update the SKU, Name, Prices, and the DgLandId. Requires admin panel credentials.This service downloads csv file from Wordpress and update the database");

        app.MapPost("/product/update-missing-products", async ([FromServices] IMediator mediator, AdminPanelCredential credential) =>
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
        }).WithSummary("Update Missing Products")
            .WithDescription("Updates missing products in the database. Requires admin panel credentials. This service downloads csv file from Wordpress and find the missing products in the database and add them");

        app.MapGet("/product/cache-products", async ([FromServices] IMediator mediator) =>
        {
            try
            {
                await mediator.Send(new CrawlerQuery(CrawlRequest.CacheProducts));
                return Results.NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
        });

        app.MapPost("/product/add-product-attribute", async ([FromServices] IMediator mediator,HttpRequest request) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest("Form content type is required.");

            var form = await request.ReadFormAsync();

            var file = form.Files.GetFile("file");

            if (file == null || file.Length == 0)
                return Results.BadRequest("CSV file is required.");

            var response = await mediator.Send(new GetProductAttributeRequest(file));

            return Results.File(response.MemoryStream, "text/csv", "wordpress-products.csv");

        }).Accepts<IFormFile>("multipart/form-data", "file")
        .WithSummary("Add Additional Information, Short Product Description and Product Discription(What's in The Box)")
        .WithDescription("Use Postman and attach your csv file (Should be the Wordpress template with full columns, and Post it. This service sends requests to ChatGPT API so for each record it could takes up to 20 seconds)");
    }

    
}
