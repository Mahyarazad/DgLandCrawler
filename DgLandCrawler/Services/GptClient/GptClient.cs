using DgLandCrawler.Models;
using System.Text;

namespace DgLandCrawler.Services.GptClient
{
    public class GptClient : IGptClient
    {
        private const string key = "";
        private readonly IHttpClientFactory _httpClientFactory;
        public GptClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Root?> GetResultFromGPT(string prompt)
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");

            request.Headers.Add("Authorization", $"Bearer {key}");

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

            return System.Text.Json.JsonSerializer.Deserialize<Root>(responseString);
        }
    }
}
