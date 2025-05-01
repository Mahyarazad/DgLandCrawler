using DgLandCrawler.Data.Repository;
using DgLandCrawler.Services.GptClient;
using Microsoft.Extensions.Logging;
using DgLandCrawler.Models;
using DgLandCrawler.Models.DTO;
using System.Text;
using Serilog;

namespace DgLandCrawler.Services.SiteCrawler
{
    public class ChatGPTService : IChatGPTService
    {
        private readonly IGptClient _gptClient;
        private readonly IDGProductRepository _dGProductRepository;

        public ChatGPTService(IGptClient gptClient, IDGProductRepository dGProductRepository)
        {
            _gptClient = gptClient;
            _dGProductRepository = dGProductRepository;
        }

        public async Task<List<WordpressProduct>> GetProductAttributes(List<WordpressProduct> wordpressProducts)
        {
            foreach (var dg in wordpressProducts)
            {
                try
                {
                    string query = $"Just give me additional information for {dg.Name}." +
                        $"List them with Key Value pairs, Use the Manufacturer websites or use TechRadar, AnandTech, or Tom’s Hardware to get the information" +
                        $", include all the technical specifications. Just Use markdown language and separate the technical specifications from other infomration" +
                        $" and ONLY use HEADING LEVEL 3 for separation. What's in The Box comes first, Key Features comes sceond, " +
                        $"Technical Specifications comes third, and lastly Additional Features." +
                        $"DON'T PUT THE MODEL in the Technical Specifications, " +
                        $"All the sub-items should start with - **";
                        

                    var respone = await _gptClient.GetResultFromGPT(query);
                    await Update_WordpressProducts(dg, respone);

                }
                catch (Exception ex)
                {
                    Log.Error("Exception >> GetProductAttributes >> {Message}", new { Message = ex.Message });
                }
            }

            return wordpressProducts;
        }

        private Task<WordpressProduct> Update_WordpressProducts(WordpressProduct dg, Root? respone)
        {
            if (respone != null && respone.choices.Count > 0)
            {
                var input = respone.choices[0].message.content;
                var specifications = new Dictionary<string, string>();
                var features = new Dictionary<string, string>();

                Log.Information("GetProductAttributes >> Product Name >>  {Input}", new { Input = dg.Name });

                Log.Information("GetProductAttributes >> GPT Response >>  {Input}", new { Input = input });

                // Split the input into specifications and features sections
                string[] sections = input.Split("###", StringSplitOptions.None);

                sections = [.. sections.Where(x => !string.IsNullOrEmpty(x))];

                foreach (var section in sections[2..])
                {
                    Dictionary<string, string> keyValues = GetValuesFromMarkdownResult(section);

                    if(keyValues.Count == 0)
                    {

                    }

                    foreach (KeyValuePair<string, string> kv in keyValues.Take(33))
                    {
                        if (!string.IsNullOrEmpty(kv.Key)
                            && !kv.Key.Contains("Warranty") 
                            && !kv.Key.Contains("Color"))
                        {
                            dg.Attributes.Add(new ProductAttribute()
                            {
                                Name = kv.Key,
                                Values = kv.Value,
                                Global = "0",
                                Visible = "1"
                            });
                        }
                    }
                }

                Update_ProductDescription_And_ShortDescription(dg, sections);

            }

            return Task.FromResult(dg);
        }

        public async Task GetProductSearchKeywords()
        {
            var productList = await _dGProductRepository.GetList();

            foreach (var dg in productList)
            {
                try
                {
                    string query = $"I just need the user search keywords (please consider irregular words as well) for this product {dg.Name}, " +
                        $"please separate your results using dash and space remove your explnation from your respone.";

                    var respone = await _gptClient.GetResultFromGPT(query);

                    var firstResult = respone!.choices.FirstOrDefault();

                    var result = firstResult?.message.content!.Split("- ");

                    dg.Keywords ??= [];

                    foreach (var item in result!.Where(x => !string.IsNullOrEmpty(x)))
                    {
                        dg.Keywords.Add(new Keyword { value = item.Replace("\n", "").Trim() });
                    }

                    await _dGProductRepository.AddAsync(dg);

                    Log.Information(dg.Name);

                }
                catch (Exception ex)
                {
                    Log.Error("Exception >> GetProductSearchKeywords >> {Message}", new { Message = ex.Message });
                }
            }
        }

        private static void Update_ProductDescription_And_ShortDescription(WordpressProduct dg, string[] sections)
        {
            var result = BuildHtml(sections[0], true);

            dg.Description = result;

            var _result = BuildHtml(sections[1]);

            dg.ShortDescription = _result;
        }

        private static string BuildHtml(string stringValue, bool productDescription = false)
        {
            StringBuilder sb = new();

            if (productDescription)
            {
                sb.Append("<h4>What’s in The Box?</h4>");
            }

            sb.Append("<ul>");

            Dictionary<string, string> dic = new();

            if (productDescription)
            {
                dic = GetValuesFromMarkdownResult(stringValue, true);
            }
            else
            {
                dic.Clear();
                dic = GetValuesFromMarkdownResult(stringValue);
            }
                

            foreach (KeyValuePair<string, string> kv in dic)
            {
                if (productDescription)
                {
                    if(!string.IsNullOrEmpty(kv.Value))
                        sb.Append($"<li><strong>{kv.Value}</strong></li>");
                }
                else
                {
                    sb.Append($"<li><strong>{kv.Key}</strong>: <span>{kv.Value}</span></li>");
                }
            }

            sb.Append("</ul>");

            return sb.ToString();
        }

        private static Dictionary<string, string> GetValuesFromMarkdownResult(string section, bool boxSection = false)
        {
            string[] lines = section.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            Dictionary<string, string> keyValues = new();
            string currentKey = null;
            List<string> multiLineValue = new();

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();

                if (line.StartsWith("- **"))
                {
                    // Save previous key-value if multi-line
                    if (line.StartsWith("- **") && line.EndsWith("**"))
                    {
                        //keyValues[currentKey] = string.Join("\n", multiLineValue);
                        //multiLineValue[currentKey] = true;
                    }

                    int start = line.IndexOf("**") + 2;
                    int end = line.IndexOf("**", start);
                    string key = line.Substring(start, end - start).Trim();
                    string value = line.Substring(end + 2).TrimStart(':', ' ');

                    keyValues[key] = value;

                    currentKey = key;
                }
                


                if (boxSection)
                {
                    if(!line.StartsWith("What's"))
                        keyValues[line] = line.Replace("- **", "").Replace("**", ""); 
                }
            }

            return keyValues;
        }
    }

    public interface IChatGPTService
    {
        Task GetProductSearchKeywords();

        Task<List<WordpressProduct>> GetProductAttributes(List<WordpressProduct> wordpressProducts);
    }
}
