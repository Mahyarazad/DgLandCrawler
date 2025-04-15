using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using DgLandCrawler.Models;
using OpenQA.Selenium.Interactions;
using DgLandCrawler.Services.GptClient;
using System.Data;
using DgLandCrawler.Helper;
using OpenQA.Selenium.Support.Extensions;
using System.Diagnostics;
using DgLandCrawler.Services.DbUpdater;
using DgLandCrawler.Data.Repository;
using DgLandCrawler.Models.DTO;


namespace DgLandCrawler.Services.SiteCrawler
{
    public class SiteCrawlerService : ISiteCrawlerService, IDisposable
    {
        private readonly ILogger<SiteCrawlerService> _logger;
        private readonly ChromeOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDbUpdater _dbUpdater;
        private readonly IWebDriver _driver;
        private readonly IGptClient _gptClient;
        private readonly IDGProductRepository _dGProductRepository;

        private readonly DevToolsSession _devtools;

        public SiteCrawlerService(ILogger<SiteCrawlerService> logger, IHttpClientFactory httpClientFactory, IGptClient gptClient, IDGProductRepository dGProductRepository, IDbUpdater dbUpdater)
        {
                _logger = logger;
                _options = new ChromeOptions();
                _options.AddArgument("--start-maximized");
                _options.AddArgument("--remote-debugging-port=9223");
                _options.AddUserProfilePreference("download.default_directory", @"C:\Users\mhyri\Downloads\selenium_downloads");
                _options.AddUserProfilePreference("download.prompt_for_download", false);
                _options.AddUserProfilePreference("download.directory_upgrade", true);
                _options.AddUserProfilePreference("safebrowsing.enabled", true);
                _driver = new ChromeDriver(_options);
                _httpClientFactory = httpClientFactory;
                DevToolsSession _devtools = ((ChromeDriver)_driver).GetDevToolsSession();
                _gptClient = gptClient;
                _dGProductRepository = dGProductRepository;
                _dbUpdater = dbUpdater;
        }


        public async Task StartCaching()
        {
            try
            {
                _driver.Navigate().GoToUrl("https://dgland.ae/sitemap.xml");

                var pageSource = _driver.PageSource;

                foreach(var link in LinkParser.ExtractLinks(pageSource))
                {
                    if(!string.IsNullOrEmpty(link))
                    {

                        //_driver.Navigate().GoToUrl(link);
                        //var innerlinks = ExtractLinks(_driver.PageSource);

                        ////JsonNode parameters = JsonNode();
                        //await _devtools.SendCommand("Performance.enable", new JsonObject());
                        //var response = await _devtools.SendCommand("Performance.getMetrics", new JsonObject());

                        //_devtools.DevToolsEventReceived += (sender, e) =>
                        //{
                        //    Console.WriteLine(e.ToString());
                        //};

                        //var metrics = response;
                        //Console.WriteLine(response.ToString());
                        //var metrics = System.Text.Json.JsonSerializer.Deserialize<Root>(response.ToString());
                        //// Extract LCP, CLS, and INP from the metrics
                        //var lcp = metrics["largestContentfulPaint"]; 
                        //var cls = metrics["cumulativeLayoutShift"]; 
                        //var inp = metrics["interactionToNextPaint"];
                        //foreach(var jobUrl in innerlinks)
                        //{
                        //_driver.Navigate().GoToUrl(jobUrl);



                        //var requestBody = new UrlNotification
                        //{
                        //    Url = jobUrl,
                        //    Type = action
                        //};



                        //var response = await httpClient.SendAsync(url);
                        //Console.WriteLine($"  >> Current link >>  " + link + "  >> Current innerlink >>  " + innerlink);
                        //}
                        //Console.WriteLine("  >> Current link >>  " + link);
                    }
                }


            }
            catch(Exception e)
            {
                _logger.LogError("An error occurred: {Message}", new { Message = e.Message });

                _logger.LogError("An error occurred: {StackTrace}", new { StackTrace = e.StackTrace });
            }

        }

        private Task<List<DGProduct>> GetProductList()
        {
            List<DGProduct> PdList = [];

            try
            {
                _driver.Navigate().GoToUrl("https://dgland.ae/product-sitemap.xml");

                var pageSource = _driver.PageSource;

                foreach(var link in LinkParser.ExtractLinks(pageSource))
                {
                    if(!string.IsNullOrEmpty(link) && !link.Contains("shop"))
                    {
                        PdList.Add(new DGProduct
                        {
                            Name = link.Replace("https://dgland.ae/product/", "")
                                  .Replace("/", "")
                                  .Replace("-", " ").ToUpperInvariant(),
                            Url = link
                        });

                    }
                }
            }
            catch(Exception e)
            {
                _logger.LogError("An error occurred: {Message}", new { Message = e.Message });

                _logger.LogError("An error occurred: {StackTrace}", new { StackTrace = e.StackTrace });
            }

            return Task.FromResult(PdList);
        }

        public async Task UpdateAltImages()
        {
            await _driver.Navigate().GoToUrlAsync("https://dgland.ae/wp-admin/upload.php");

            await Task.Delay(5000);

            try
            {
                for(var i = 0; i < 20; i++)
                {
                    _driver.FindElement(By.ClassName("load-more")).Click();

                    await Task.Delay(3000);
                }

            }
            catch(Exception e)
            {
                _logger.LogError("An error occurred: {Message}", new { Message = e.Message });

                _logger.LogError("An error occurred: {StackTrace}", new { StackTrace = e.StackTrace });
            }

            var doc = new HtmlDocument();

            doc.LoadHtml(_driver.PageSource);

            var nodes = doc.DocumentNode.SelectNodes("//li[@role='checkbox']");

            if(nodes != null)
            {
                foreach(var node in nodes)
                {
                    try
                    {

                        _driver.FindElement(By.XPath(node.XPath)).Click();

                        var productElement = _driver.FindElement(By.XPath("//div[@class='uploaded-to']//a"));

                        if(productElement != null)
                        {
                            //_driver.FindElement(By.Id("attachment-details-two-column-alt-text")).Clear();

                            //_driver.FindElement(By.Id("attachment-details-two-column-alt-text")).SendKeys(productElement.Text);

                            _driver.FindElement(By.Id("attachment-details-two-column-title")).Clear();

                            _driver.FindElement(By.Id("attachment-details-two-column-title")).SendKeys(productElement.Text);

                            //_driver.FindElement(By.Id("attachment-details-two-column-caption")).Clear();

                            //_driver.FindElement(By.Id("attachment-details-two-column-caption")).SendKeys(productElement.Text);

                            //_driver.FindElement(By.Id("attachment-details-two-column-description")).Clear();

                            //_driver.FindElement(By.Id("attachment-details-two-column-description")).SendKeys(productElement.Text);
                        }

                        await Task.Delay(100);

                        _driver.FindElement(By.ClassName("media-modal-close")).Click();

                    }
                    catch(NoSuchElementException e)
                    {
                        _logger.LogError("An error occurred: {Message}", new { Message = e.Message });

                        _driver.FindElement(By.ClassName("media-modal-close")).Click();

                        _logger.LogError("An error occurred: {StackTrace}", new { StackTrace = e.StackTrace });
                    }
                    catch(Exception e)
                    {
                        _logger.LogError("An error occurred: {Message}", new { Message = e.Message });

                        _logger.LogError("An error occurred: {StackTrace}", new { StackTrace = e.StackTrace });
                    }
                }
            }
        }

        public async Task PostPDP()
        {
            AdminPanelHelper.Login(_driver);

            var productList = await GetProductList();

            foreach(var item in productList)
            {
                await DotheJob(item);
            }
        }

        private async Task DotheJob(DGProduct item)
        {
            try
            {
                var start = Stopwatch.GetTimestamp();

                _logger.LogInformation("Start processing item: {Message}", item.Name);
                _driver.Navigate().GoToUrl(item.Url);
                var elem = _driver.FindElement(By.XPath("//div[@class='woocommerce-product-gallery__wrapper']"));

                var img = elem.FindElement(By.XPath("//img[@class='zoomImg']"));

                var productImageSource = img.GetAttribute("src");
                var productCategory = _driver.FindElement(By.XPath("//div[@class='product-category']"));
                string category = productCategory.FindElements(By.TagName("a"))[0]!.Text;

                string titlePrompt = $"Just give me a title and use important parts of the product name like brand, model name, and the model number in the end, less than 8 words, and a short description for the product description page," +
                $" less than 23 words, using the exact generated title inside for this product: {item.Name}";

                var title_root = await _gptClient.GetResultFromGPT(titlePrompt);
                var pdpTitle = title_root!.choices.FirstOrDefault();
                var pdpTitleReult = pdpTitle?.message.content!.Split("\n\n");
                var titleDescription = new
                {
                    Title = pdpTitleReult[0]!.Replace("**Title:** ", "").Replace("\n", ""),
                    Description = pdpTitleReult[1]!.Replace("**Description:** ", "").Replace("\n", "")
                };


                string bodyPrompt = $"Just create a SEO PDP and consider using at least 850 words " +
                    $"without any Customer Reviews, don't use <ol> instead use <ul> tags, don't add any" +
                    $"explanation at the end, for {item.Name} in html div," +
                    $"please add this tag <img style=\"width: 300px; height: 300px; border: none; max-width: none; max-height: none;\" " +
                    $"src='{productImageSource}'alt='{item.Name}' /> and use the excact {titleDescription.Title} somewhere in the begining of your text, " +
                    $"please don't capitalize the product name in your text, " +
                    $"please add the style='display:none;' to the h1 tags" +
                    $"and this call to action button after your PDP in the html" +
                    $"<div style='display:flex''><div title='View {item.Name} on DGland'><a class='button' style='margin-right: 4px;text-decoration: none;background-color: #ed1c24;color: white;' 'href='{item.Url}'>View Product on DGland</a></div><div title='{item.Name} on Amazon'><a class='button' style='text-decoration: none;background-color: #febd69;' href='https://www.amazon.ae/s?k={item.Name.Replace(' ', '+')}'>View Product on Amazon</a></div></div>";

                var body_root = await _gptClient.GetResultFromGPT(bodyPrompt);


                var pdpBody = body_root!.choices.FirstOrDefault();

                var pdpBodyReult = pdpBody?.message.content!
                    .Replace("```html", "")
                    .Replace("```", "")
                    .Replace("\n", "");


                _driver.Navigate().GoToUrl("https://dgland.ae/wp-admin/post-new.php");

                _driver.FindElement(By.XPath("//input[@name='post_title']")).SendKeys(titleDescription.Title);

                _driver.FindElement(By.XPath("//button[@id='content-html']")).Click();

                _driver.FindElement(By.XPath("//textarea[@class='wp-editor-area']")).SendKeys(pdpBodyReult);





                IWebElement editSnippet = _driver.FindElement(By.XPath("//button[@class='components-button rank-math-edit-snippet is-primary']"));
                _driver.ExecuteJavaScript("arguments[0].scrollIntoView(true);", editSnippet);
                editSnippet.Click();
                await Task.Delay(200);

                IWebElement editSnippetTtile = _driver.FindElement(By.XPath("//input[@id='rank-math-editor-title']"));
                editSnippetTtile.Clear();
                editSnippetTtile.SendKeys(titleDescription.Title);

                IWebElement editSnippetDescription = _driver.FindElement(By.XPath("//textarea[@id='rank-math-editor-description']"));
                editSnippetDescription.Clear();
                editSnippetDescription.SendKeys(titleDescription.Description);

                _driver.FindElement(By.XPath("//button[@class='components-button is-small has-icon']")).Click();
                await Task.Delay(200);


                IWebElement categoryDiv = _driver.FindElement(By.XPath("//div[@id='categorydiv']"));
                _driver.ExecuteJavaScript("arguments[0].scrollIntoView(true);", categoryDiv);
                await Task.Delay(100);
                var categorychecklist = _driver.FindElements(By.XPath("//ul[@id='categorychecklist']"));
                var normilizedCat = StringHelper.CapitalizeWords(category);

                var categoryElement = _driver.FindElements(By.XPath($"//label[contains(text(),'{normilizedCat}')]"));
                if(categoryElement.Count != 0)
                {
                    _driver.ExecuteJavaScript("arguments[0].scrollIntoView(true);", categoryElement[0]);
                    await Task.Delay(500);
                    categoryElement[0].Click();
                }
                else
                {
                    IWebElement a = _driver.FindElement(By.XPath("//a[@id='category-add-toggle']"));
                    _driver.ExecuteJavaScript("arguments[0].scrollIntoView(true);", a);
                    a.Click();

                    IWebElement b = _driver.FindElement(By.XPath("//select[@id='newcategory_parent']"));
                    _driver.ExecuteJavaScript("arguments[0].scrollIntoView(true);", b);
                    b.Click();

                    IWebElement c = _driver.FindElement(By.XPath("//option[@value='17']"));
                    _driver.ExecuteJavaScript("arguments[0].scrollIntoView(true);", c);
                    c.Click();

                    IWebElement d = _driver.FindElement(By.XPath("//input[@id='newcategory']"));
                    _driver.ExecuteJavaScript("arguments[0].scrollIntoView(true);", d);
                    d.SendKeys(normilizedCat);

                    _driver.FindElement(By.XPath("//input[@id='category-add-submit']")).Click();
                }
                await Task.Delay(500);


                // Get the list of elements matching the XPath expression
                IWebElement rankMathSeoH2 = _driver.FindElement(By.XPath("//h2[@class='components-panel__body-title']"));
                _driver.ExecuteJavaScript("arguments[0].scrollIntoView(true);", rankMathSeoH2);
                await Task.Delay(1000);
                var rankMathSeo = _driver.FindElements(By.XPath("//tags[@class='tagify tagify--noTags tagify--empty']"));
                // Ensure that at least two elements exist
                if(rankMathSeo.Count != 0)
                {
                    // Access the second element (index 1 because the list is 0-indexed)
                    IWebElement secondElement = rankMathSeo[^1];
                    IWebElement childElement = secondElement.FindElement(By.XPath("./*"));
                    // Find the next sibling of the second element
                    //IWebElement nextSibling = secondElement.FindElement(By.XPath("following-sibling::*[1]"));

                    //_driver.ExecuteJavaScript("arguments[0].setAttribute('class', 'tagify tagify--noTags tagify--focus');", secondElement);
                    //IWebElement inputSpan = secondElement.FindElement(By.XPath("//span[@class='tagify__input'][@aria-multiline='false']"));
                    // Get the placeholder attribute of the next sibling

                    childElement.SendKeys(titleDescription.Title);
                    childElement.Click();
                    childElement.Click();
                    rankMathSeoH2.Click();
                    await Task.Delay(1000);
                }

                var visual = _driver.FindElement(By.XPath("//button[@id='content-tmce']"));
                _driver.ExecuteJavaScript("arguments[0].scrollIntoView(true);", visual);
                visual.Click();
                await Task.Delay(3000);


                var publish = _driver.FindElement(By.XPath("//div[@id='submitdiv']"));
                _driver.ExecuteJavaScript("arguments[0].scrollIntoView(true);", publish);
                await Task.Delay(100);
                _driver.FindElement(By.XPath("//input[@id='publish']")).Click();

                await Task.Delay(10000);

                var delta = Stopwatch.GetElapsedTime(start, Stopwatch.GetTimestamp());

                _logger.LogInformation("Finish processing item: {Message} ==> It tootk {Delta}", item.Name, delta);

            }
            catch(Exception e)
            {
                _logger.LogError("Something went wrong, {Message}", e.Message);
            }

        }

        public async Task PostProductReview()
        {
            var random = new Random();

            foreach(var item in GetProductList().Result)
            {
                try
                {
                    int arabicReview = random.Next(0, 1);
                    int randNegative = random.Next(1, 4);
                    int randAge = random.Next(1, 3);
                    int randNationality = random.Next(0, 5);
                    string randomNegative = randNegative == 3 ? "and randomly make medium comment with a reason" : "";
                    string randomBirth = randAge == 1 ? "and a random age number minus 2025 " : "";
                    string _arabicReview = arabicReview == 1 ? "in Arabic language " : "in English language";
                    string randomNath = new string[] { "Russians", "Europians", "Indins", "Pakistani", "Phlipino", "Iranins" }[randNationality];


                    string prompt = $"Please just give me an authentic product review {_arabicReview} less than 30 words {randomNegative}" +
                        ",use moderate street language, use the product name in your review but avoid copying it " +
                        "and also don't use totally worth it, fire, dope or vibes, solid terms" +
                        $", add random username (don't use Petrov, Rahul ,Ali Raza, Khan, Malik, Dmitry Ivanov, Sharma and Arjun Patel) after review (choose from this {randomNath} " +
                        "nationality), add email based on the mixed conmibintion " +
                        $"of {randomBirth} with gmail, add random rating from 2 to 5" +
                        $" for this product: {item.Name}. Please make sure your reponse is separated in the following order" +
                        " review - name - rating - email. Also please consider the gender according" +
                        " to the product; for example, men don't use hairdryers";



                    await CaptureSubmit(item, random, prompt);
                }
                catch(Exception ex)
                {
                    _logger.LogError("An error occurred: {Message}", ex.Message);
                }

            }
        }

        private async Task CaptureSubmit(DGProduct product, Random random, string prompt)
        {
            _driver.Navigate().GoToUrl(product.Url);

            IWebElement element = _driver.FindElement(By.XPath("//a[@href='#tab-reviews']"));

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            await Task.Delay(1000);

            element.Click();

            await Task.Delay(1000);

            int randomNumber = random.Next(2, 7);

            _logger.LogInformation("Start adding review {ProductName}", product.Name);

            for(int i = 0; i < randomNumber; i++)
            {
                var root = await _gptClient.GetResultFromGPT(prompt);

                if(root!.choices.Any())
                {
                    try
                    {
                        var firstResult = root!.choices.FirstOrDefault();

                        var res = firstResult?.message.content!.Replace("\n", "").Split("  ");

                        _logger.LogInformation("GPT Result {Res}", firstResult?.message.content);

                        var gptReview = new GptReview
                        {
                            Message = res[0],
                            PersonName = res[1].Replace("-", "").Trim(),
                            Score = Convert.ToInt16(res[2].Replace("Rating: ", "").Replace("-", "").Trim()),
                            Email = res[3].Replace("-", "").Trim()
                        };

                        _logger.LogInformation("Adding Review for {ProductName}", product.Name);

                        await AddReview(gptReview);

                        _logger.LogInformation("Review Added {ProductName}", product.Name);

                        await Task.Delay(8000);

                    }
                    catch(Exception ex)
                    {
                        _logger.LogError("CaptureSubmit {Error}", ex.Message);
                    }
                }
            }
        }

        private async Task AddReview(GptReview gptReview)
        {
            try
            {
                Actions actions = new Actions(_driver);
                IWebElement score = _driver.FindElement(By.XPath($"//a[@class='star-{gptReview.Score}']"));
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", score);
                await Task.Delay(100);
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", score);


                _driver.FindElement(By.XPath("//textarea[@id='comment']")).SendKeys(gptReview.Message);


                _driver.FindElement(By.XPath("//input[@id='author']")).SendKeys(gptReview.PersonName);


                _driver.FindElement(By.XPath("//input[@id='email']")).SendKeys(gptReview.Email);
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", _driver.FindElement(By.XPath("//input[@id='email']")));

                IWebElement submit = _driver.FindElement(By.CssSelector("input[id='submit']"));
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", submit);

                await Task.Delay(100);
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", submit);
                await Task.Delay(100);
            }
            catch(Exception ex) { _logger.LogError("AddReview {Error}", ex.Message); }
        }

        public async Task DownloadDGLandProducts()
        {


            AdminPanelHelper.Login(_driver);

            _driver.Navigate().GoToUrl("https://dgland.ae/wp-admin/edit.php?post_type=product&page=product_exporter");
            _driver.FindElement(By.XPath("//button[@value='Generate CSV']")).Click();

            await Task.Delay(20000);

            await Task.Delay(100);

            bool closeSignin = true;


            //foreach(var item in data.Reverse())
            //{
            //    try
            //    {

            //        var searchElem = _driver.FindElement(By.XPath("//input[@data-qa='txt_searchBar']"));

            //        _logger.LogInformation("Search Element: {Message}", searchElem);
            //        var clear = _driver.FindElements(By.TagName("button"));
            //        clear[2].Click();

            //        searchElem.Clear();
            //        searchElem.SendKeys(item.Name.Replace(" ","-"));
            //        searchElem.SendKeys(Keys.Enter);
            //        await Task.Delay(3000);
            //        var continer = _driver.FindElements(By.ClassName("grid"));

            //        if(closeSignin)
            //        {
            //            await Task.Delay(2000);
            //            _driver.FindElement(By.CssSelector("[class*='SigninV2_closeButton']")).Click();
            //            await Task.Delay(1000);
            //            closeSignin = false;
            //        }


            //        if(continer.Count > 0)
            //        {
            //            var stringCheck = item.Name.Split(' ');
            //            foreach(var elem in continer)
            //            {
            //                try
            //                {
            //                    var productNameElement = elem.FindElement(By.CssSelector("div[data-qa='product-name']"));
            //                    if(CheckProductSearch(stringCheck, productNameElement))
            //                    {

            //                        var amountElement = elem.FindElement(By.ClassName("amount"));
            //                        _logger.LogInformation("Product Detail: {Message}", amountElement.Text);

            //                        _logger.LogInformation("Product Detail: {Message}", productNameElement.GetAttribute("title"));



            //                        if(!string.IsNullOrEmpty(productNameElement.GetAttribute("title")) && !string.IsNullOrEmpty(amountElement.Text))
            //                        {
            //                            item.GoogleResult.Add(new GoogleSearchResult
            //                            {
            //                                Title = productNameElement.GetAttribute("title"),
            //                                Price = amountElement.Text,
            //                                Supplier = "Noon.com",
            //                                CreationTime = DateTime.Now
            //                            });

            //                        }
            //                    }
            //                }
            //                catch(Exception e)
            //                {
            //                    _logger.LogError("An error occurred: {Message}", e.Message);
            //                }
            //            }

            //            item.CrawlDateTime = DateTime.Now;
            //            await _dGProductRepository.AddAsync(item);
            //        }
            //    }
            //    catch(Exception e)
            //    {
            //        _logger.LogError("An error occurred: {Message}", e.Message);
            //    }

            //}

            //await AddGoogleContainerResult(data);
        }

        public async Task CrawlThreeMainSupplier()
        {
            foreach(var dg in await _dGProductRepository.GetList())
            {
                foreach(var google in dg.GoogleResult)
                {
                    try
                    {
                        _driver.Navigate().GoToUrl(google.BaseUrl);
                        IWebElement priceElement = null;

                        switch(google.Supplier)
                        {
                            case "noon":
                                string noonScript = "return document.querySelectorAll('div[class=\"priceNow\"]').length > 0;";
                                bool noonElement = _driver.ExecuteJavaScript<bool>(noonScript);
                                if(noonElement)
                                {
                                    var price = _driver.FindElement(By.XPath("//div[@class='priceNow']")).Text;
                                    if(!string.IsNullOrEmpty(price))
                                    {
                                        google.Price = price
                                                .Replace("Inclusive of VAT", "")
                                                .Replace("AED", "")
                                                .Replace("\r\n", "");
                                    }
                                }
                                else
                                {
                                    google.Price = "0";
                                }

                                break;
                            case "sharafdg":
                                string sharafScript = "return document.querySelectorAll('meta[itemprop=\"price\"]').length > 0;";
                                bool sharafElement = _driver.ExecuteJavaScript<bool>(sharafScript);
                                if(sharafElement)
                                {
                                    priceElement = _driver.FindElement(By.XPath("//meta[@itemprop='price']"));
                                    google.Price = priceElement.GetAttribute("Content");
                                }
                                else
                                {
                                    google.Price = "0";
                                }
                                break;
                            case "amazon":
                                string amazonScript = "return document.querySelectorAll('span[class=\"a-price-whole\"]').length > 0;";
                                bool amazonElement = _driver.ExecuteJavaScript<bool>(amazonScript);
                                if(amazonElement)
                                {
                                    priceElement = _driver.FindElement(By.XPath("//span[@class='a-price-whole']"));
                                    google.Price = priceElement.Text;
                                }
                                else
                                {
                                    google.Price = "0";
                                }
                                break;
                            default:
                                break;
                        }
                        await _dGProductRepository.UpdateGoogleSearchResultsAsync(dg.Id, dg.GoogleResult);
                    }
                    catch(Exception e)
                    {
                        _logger.LogError("Something went wrong: {error}", e.Message);
                    }
                }
            }
        }

        

        private static bool CheckProductSearch(string[] stringCheck, IWebElement productNameElement)
        {
            var stringCheckList = stringCheck.Where(x => !x.Contains('–') && !x.Contains("-")).ToList();

            List<bool> checkList = new();

            int removeExtraTerms = stringCheckList.Any(x => x.ToLower() == "iphone") ? 4 : 0;

            string title = productNameElement.GetAttribute("title");

            for(int i = 0; i < stringCheckList.Count - removeExtraTerms; i++)
            {
                checkList.Add(title.ToLower().Contains(stringCheckList[i].ToLower()));
            }

            var result = checkList.Aggregate((a, b) => a && b);

            if(result)
            {

            }
            return result;
        }

        public void Dispose()
        {
            _driver.Close();
            _driver.Dispose();
        }

        public async Task GetProductSearchKeywords()
        {
            var productList = await _dGProductRepository.GetList();

            foreach(var dg in productList)
            {
                try
                {
                    string query = $"I just need the user search keywords (please consider irregular words as well) for this product {dg.Name}, " +
                        $"please separate your results using dash and space remove your explnation from your respone.";
                    var respone = await _gptClient.GetResultFromGPT(query);

                    var firstResult = respone!.choices.FirstOrDefault();

                    var result = firstResult?.message.content!.Split("- ");

                    dg.Keywords ??= [];

                    foreach(var item in result!.Where(x=> !string.IsNullOrEmpty(x)))
                    {
                        dg.Keywords.Add(new Keyword { value = item.Replace("\n","").Trim() });
                    }

                    await _dGProductRepository.AddAsync(dg);

                    _logger.LogInformation(dg.Name);

                }catch(Exception ex)
                {
                    _logger.LogError(dg.Name);
                }
            }

            //await _dGProductRepository.BulkUpdate(productList);
        }

        public Task AdminLogin(AdminPanelCredential credential)
        {
            _driver.Navigate().GoToUrl("https://dgland.ae/wp-admin/");

            _driver.FindElement(By.Id("user_login")).SendKeys(credential.Useranme);

            _driver.FindElement(By.Id("user_pass")).SendKeys(credential.Password);

            _driver.FindElement(By.Id("wp-submit")).Click();

            return Task.CompletedTask;

        }
    }
}
    