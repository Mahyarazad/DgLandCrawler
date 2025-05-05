using DgLandCrawler.Models;
using Microsoft.Extensions.Options;
using Serilog;
using System.Text;

namespace DgLandCrawler.Services.GptClient
{
    public class GptClient : IGptClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppConfig _appConfig;

        public GptClient(IHttpClientFactory httpClientFactory , IOptions<AppConfig> appConfig)
        {
            _appConfig = appConfig.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Root?> GetResultFromGPT(string prompt)
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");

            request.Headers.Add("Authorization", $"Bearer {_appConfig.ChatGPT.Key}");

            var requestObject = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0.7
            };

            var jsonString = System.Text.Json.JsonSerializer.Serialize(requestObject);

            request.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            Log.Information("GptClient >> GetResultFromGPT >> {Message}", new { Message = responseString });

            return System.Text.Json.JsonSerializer.Deserialize<Root>(responseString);
        }
    }
}
