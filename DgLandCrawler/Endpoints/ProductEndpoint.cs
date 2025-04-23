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

        }).Accepts<IFormFile>("multipart/form-data", "file");
    }

    
}
