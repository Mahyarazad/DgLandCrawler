﻿using CsvHelper;
using DgLandCrawler.Helper;
using DgLandCrawler.Models.DTO;
using DgLandCrawler.Services.SiteCrawler;
using MediatR;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace DgLandCrawler.Data.Handlers
{
    public class GetProductAttributeDescriptionHandler : 
        IRequestHandler<GetProductAttributeRequest, MemoryStream>
    {
        private readonly IChatGPTService _gptService;

        public GetProductAttributeDescriptionHandler(IChatGPTService gptService)
        {
            _gptService = gptService;
        }


        public async Task<MemoryStream> Handle(GetProductAttributeRequest request, CancellationToken cancellationToken)
        {
            using var stream = request.FormFile.OpenReadStream();

            using var reader = new StreamReader(stream);

            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            List<WordpressProduct> records = await CSVHelper.GetWordPressProducts(csv);

            List<WordpressProduct> updatedRecords = await _gptService.GetProductAttributes(records);

            return await CSVHelper.ExportWordpressProducts(updatedRecords);
        }
    }

    public record struct GetProductAttributeRequest(IFormFile FormFile) : IRequest<MemoryStream>
    {
        public IFormFile FormFile = FormFile;
    }

}
