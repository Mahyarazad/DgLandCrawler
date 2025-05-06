using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using HtmlAgilityPack;
using DgLandCrawler.Models;
using OpenQA.Selenium.Interactions;
using DgLandCrawler.Services.GptClient;
using System.Data;
using DgLandCrawler.Helper;
using OpenQA.Selenium.Support.Extensions;
using System.Diagnostics;
using DgLandCrawler.Data.Repository;
using DgLandCrawler.Models.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using OpenQA.Selenium.Support.UI;


namespace DgLandCrawler.Services.SiteCrawler
{
    public class SiteCrawlerService : ISiteCrawlerService
    {
        private readonly IGptClient _gptClient;
        private readonly IDGProductRepository _dGProductRepository;
        private readonly AppConfig _config;

        public SiteCrawlerService(IOptions<AppConfig> _appConfig,
            IGptClient gptClient, IDGProductRepository dGProductRepository, IConfiguration config)
        {
            _gptClient = gptClient;
            _dGProductRepository = dGProductRepository;
            _config = _appConfig.Value;
        }

        public (IWebDriver Driver, string ProfilePath) CreateDriver(int remoteDebuggingPort, bool headless = true)
        {
            var options = new ChromeOptions();

            string userDataDir = Path.Combine(Path.GetTempPath(), $"chrome-profile-{Guid.NewGuid()}");
            Directory.CreateDirectory(userDataDir);
            options.AddArgument($"--user-data-dir={userDataDir}");

            if (headless)
            {
                options.AddArgument("--headless=new");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--window-size=1920,1080");
            }
            else
            {
                options.AddArgument("--start-maximized");
            }

            options.AddUserProfilePreference("download.default_directory", @"C:\Users\mhyri\Downloads\selenium_downloads");
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("download.directory_upgrade", true);
            options.AddUserProfilePreference("safebrowsing.enabled", true);

            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            var driver = new ChromeDriver(service, options);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            return (driver, userDataDir);
        }





        public async Task StartCaching()
        {
            var (_driver, profilePath) = CreateDriver(9231);

            try
            {
                var taskList = new List<Task>();
                _driver.Navigate().GoToUrl("https://dgland.ae/sitemap_index.xml");
                var parentLinks = LinkParser.GetSiteUrl(_driver.PageSource);
                foreach (var (link, index) in parentLinks.Select((value, index) => (value, index)))
                {
                    if (!string.IsNullOrEmpty(link))
                    {
                        _driver.Navigate().GoToUrl(link);

                        var childNodes = LinkParser.GetSiteUrl(_driver.PageSource);

                        taskList.Add(Task.Run(() =>
                        {
                            var (__driver, profilePath) = CreateDriver(940 + index);
                            foreach (var childLink in childNodes[1..])
                            {
                                try
                                {
                                    Log.Information("Caching >>  Url >> {Message}", new { Message = childLink });
                                    __driver.Navigate().GoToUrl(childLink);
                                }
                                catch (Exception ex)
                                {
                                    Log.Error("Inner navigation error: {Message}", ex.Message);
                                    Log.Error("StackTrace: {StackTrace}", ex.StackTrace);
                                }
                                finally
                                {
                                    __driver.Quit();
                                    try { Directory.Delete(profilePath, true); }
                                    catch (Exception ex) { Log.Warning("Profile cleanup failed: {Message}", ex.Message); }
                                }

                            }
                        }
                        ));
                    }
                }

                await Task.WhenAll(taskList);
            }
            catch (Exception e)
            {
                Log.Error("An error occurred: {Message}", new { Message = e.Message });

                Log.Error("An error occurred: {StackTrace}", new { StackTrace = e.StackTrace });
            }
            finally
            {
                _driver.Quit();
                try { Directory.Delete(profilePath, true); }
                catch (Exception ex) { Log.Warning("Profile cleanup failed: {Message}", ex.Message); }
            }

        }

        private Task<List<DGProduct>> GetProductList()
        {
            List<DGProduct> PdList = [];
            var (_driver, profilePath) = CreateDriver(9232);

            try
            {
                _driver.Navigate().GoToUrl("https://dgland.ae/product-sitemap.xml");

                var pageSource = _driver.PageSource;

                foreach (var link in LinkParser.ExtractLinks(pageSource))
                {
                    if (!string.IsNullOrEmpty(link) && !link.Contains("shop"))
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
            catch (Exception e)
            {
                Log.Error("An error occurred: {Message}", new { Message = e.Message });

                Log.Error("An error occurred: {StackTrace}", new { StackTrace = e.StackTrace });
            }
            finally
            {
                _driver.Quit();
                try { Directory.Delete(profilePath, true); }
                catch (Exception ex) { Log.Warning("Profile cleanup failed: {Message}", ex.Message); }
            }



            return Task.FromResult(PdList);
        }

        public async Task UpdateAltImages()
        {
            var (_driver, profilePath) = CreateDriver(9222);
            await _driver.Navigate().GoToUrlAsync("https://dgland.ae/wp-admin/upload.php");

            await Task.Delay(5000);

            try
            {
                for (var i = 0; i < 20; i++)
                {
                    _driver.FindElement(By.ClassName("load-more")).Click();

                    await Task.Delay(3000);
                }

            }
            catch (Exception e)
            {
                Log.Error("An error occurred: {Message}", new { Message = e.Message });

                Log.Error("An error occurred: {StackTrace}", new { StackTrace = e.StackTrace });
            }


            var doc = new HtmlDocument();

            doc.LoadHtml(_driver.PageSource);

            var nodes = doc.DocumentNode.SelectNodes("//li[@role='checkbox']");

            if (nodes != null)
            {
                try
                {
                    foreach (var node in nodes)
                    {

                        _driver.FindElement(By.XPath(node.XPath)).Click();

                        var productElement = _driver.FindElement(By.XPath("//div[@class='uploaded-to']//a"));

                        if (productElement != null)
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
                }
                catch (NoSuchElementException e)
                {
                    Log.Error("An error occurred: {Message}", new { Message = e.Message });

                    _driver.FindElement(By.ClassName("media-modal-close")).Click();

                    Log.Error("An error occurred: {StackTrace}", new { StackTrace = e.StackTrace });
                }
                catch (Exception e)
                {
                    Log.Error("An error occurred: {Message}", new { Message = e.Message });

                    Log.Error("An error occurred: {StackTrace}", new { StackTrace = e.StackTrace });
                }
                finally
                {
                    _driver.Quit();
                    try { Directory.Delete(profilePath, true); }
                    catch (Exception ex) { Log.Warning("Profile cleanup failed: {Message}", ex.Message); }
                }
            }

        }

        public async Task PostPDP()
        {
            var (_driver, profilePath) = CreateDriver(9223);

            var productList = await GetProductList();
            try
            {
                foreach (var item in productList)
                {
                    await DotheJob(item, _driver);
                }
            }
            catch (Exception e)
            {
                Log.Error("An error occurred: {Message}", new { Message = e.Message });

                Log.Error("An error occurred: {StackTrace}", new { StackTrace = e.StackTrace });
            }
            finally
            {
                _driver.Quit();
                try { Directory.Delete(profilePath, true); }
                catch (Exception ex) { Log.Warning("Profile cleanup failed: {Message}", ex.Message); }
            }
        }

        private async Task DotheJob(DGProduct item, IWebDriver _driver)
        {
            try
            {
                var start = Stopwatch.GetTimestamp();

                Log.Information("Start processing item: {Message}", item.Name);
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
                if (categoryElement.Count != 0)
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
                if (rankMathSeo.Count != 0)
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

                Log.Information("Finish processing item: {Message} ==> It tootk {Delta}", item.Name, delta);

            }
            catch (Exception e)
            {
                Log.Error("Something went wrong, {Message}", e.Message);
            }

        }

        public async Task PostProductReview()
        {
            var random = new Random();

            foreach (var item in GetProductList().Result)
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
                catch (Exception ex)
                {
                    Log.Error("An error occurred: {Message}", ex.Message);
                }

            }
        }

        private async Task CaptureSubmit(DGProduct product, Random random, string prompt)
        {
            var (_driver, profilePath) = CreateDriver(9224);
            try
            {
                _driver.Navigate().GoToUrl(product.Url);

                IWebElement element = _driver.FindElement(By.XPath("//a[@href='#tab-reviews']"));

                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
                await Task.Delay(1000);

                element.Click();

                await Task.Delay(1000);

                int randomNumber = random.Next(2, 7);

                Log.Information("Start adding review {ProductName}", product.Name);

                for (int i = 0; i < randomNumber; i++)
                {
                    var root = await _gptClient.GetResultFromGPT(prompt);

                    if (root!.choices.Any())
                    {
                        try
                        {
                            var firstResult = root!.choices.FirstOrDefault();

                            var res = firstResult?.message.content!.Replace("\n", "").Split("  ");

                            Log.Information("GPT Result {Res}", firstResult?.message.content);

                            var gptReview = new GptReview
                            {
                                Message = res[0],
                                PersonName = res[1].Replace("-", "").Trim(),
                                Score = Convert.ToInt16(res[2].Replace("Rating: ", "").Replace("-", "").Trim()),
                                Email = res[3].Replace("-", "").Trim()
                            };

                            Log.Information("Adding Review for {ProductName}", product.Name);

                            await AddReview(gptReview);

                            Log.Information("Review Added {ProductName}", product.Name);

                            await Task.Delay(8000);

                        }
                        catch (Exception ex)
                        {
                            Log.Error("CaptureSubmit {Error}", ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _driver.Quit();
                try { Directory.Delete(profilePath, true); }
                catch (Exception ex) { Log.Warning("Profile cleanup failed: {Message}", ex.Message); }
            }

        }

        private async Task AddReview(GptReview gptReview)
        {
            var (_driver, profilePath) = CreateDriver(9225);

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
            catch (Exception ex) { Log.Error("AddReview {Error}", ex.Message); }
            finally
            {
                _driver.Quit();
                try { Directory.Delete(profilePath, true); }
                catch (Exception ex) { Log.Warning("Profile cleanup failed: {Message}", ex.Message); }
            }


        }

        public async Task DownloadDGLandProducts(AdminPanelCredential credential)
        {

            var (_driver, profilePath) = CreateDriver(9226);
            try
            {
                var login = Task.Run(() => CrawlHelper.AdminLogin(credential, _driver));

                Task.WaitAll(login);

                await Task.Delay(100);

                _driver.Navigate().GoToUrl("https://dgland.ae/wp-admin/edit.php?post_type=product&page=product_exporter");

                await Task.Delay(100);
                _driver.FindElement(By.XPath("//button[@value='Generate CSV']")).Click();



                var timeStamp = DateTime.Now;

                while (Directory.GetFiles(_config.DownloadPath).Count() == 0)
                {
                    await Task.Delay(5000);
                }

                while (File.GetCreationTime(Directory.GetFiles(_config.DownloadPath).LastOrDefault()!) < timeStamp)
                {
                    await Task.Delay(5000);
                }
            }
            catch (Exception ex) { Log.Error("AddReview {Error}", ex.Message); }
            finally
            {
                _driver.Quit();
                try { Directory.Delete(profilePath, true); }
                catch (Exception ex) { Log.Warning("Profile cleanup failed: {Message}", ex.Message); }
            }

        }

        public async Task CrawlSuppliers()
        {
            var productList = await _dGProductRepository.GetList();
            var (_driver, profilePath) = CreateDriver(9227);
            try
            {

                foreach (var dg in productList.Reverse())
                {
                    foreach (var google in dg.GoogleResult)
                    {
                        _driver.Navigate().GoToUrl(google.BaseUrl);
                        IWebElement priceElement = null;

                        switch (google.Supplier)
                        {
                            case "Noon":
                                string noonScript = "return document.querySelectorAll('span[class=\"PriceOffer_priceNowText__08sYH\"]').length > 0;";
                                bool noonElement = _driver.ExecuteJavaScript<bool>(noonScript);
                                if (noonElement)
                                {
                                    var price = _driver.FindElement(By.XPath("//span[@class='PriceOffer_priceNowText__08sYH']")).Text;
                                    if (!string.IsNullOrEmpty(price))
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
                                google.UpdateTime = DateTime.Now;

                                break;
                            case "SharafDG":
                                string sharafScript = "return document.querySelectorAll('meta[itemprop=\"price\"]').length > 0;";
                                bool sharafElement = _driver.ExecuteJavaScript<bool>(sharafScript);
                                if (sharafElement)
                                {
                                    priceElement = _driver.FindElement(By.XPath("//meta[@itemprop='price']"));
                                    google.Price = priceElement.GetAttribute("Content");

                                }
                                else
                                {
                                    google.Price = "0";
                                }

                                google.UpdateTime = DateTime.Now;
                                break;
                            default:
                                break;
                        }


                        await _dGProductRepository.UpdateGoogleSearchResultsAsync(dg.Id, dg.GoogleResult);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Something went wrong: {error}", e.Message);
            }
            finally
            {
                _driver.Quit();
                try { Directory.Delete(profilePath, true); }
                catch (Exception ex) { Log.Warning("Profile cleanup failed: {Message}", ex.Message); }
            }
        }

        public enum Supplier
        {
            Noon,
            SharafDG
        }

        public async Task FetchSupplierLinks(Supplier supplier)
        {
            var productList = await _dGProductRepository.GetList();

            productList = [.. productList.Reverse()];

            var semaphore = new SemaphoreSlim(20);

            var tasks = new List<Task>();

            foreach (var (index, product) in productList.Select((index, value) => (value, index)))
            {

                await semaphore.WaitAsync(); // ⬅️ Wait for slot

                try
                {
                    Log.Information("Adding product {Id} task", product.Id); // ⬅️ Log task start

                    tasks.Add(ProcessNoonMatching(supplier, product, index));
                }
                catch (Exception ex)
                {
                    Log.Error("SiteCrealerService >> FetchSupplierLinks >> Error processing product {ProductId}: {Message}", product.Id, ex.Message);
                }
                finally
                {
                    semaphore.Release();
                }
            }

            Log.Information("Starting executing ===================>>>"); // ⬅️ Log task start
            await Task.WhenAll(tasks);
        }

        private async Task ProcessNoonMatching(Supplier supplier, DGProductData dg, int index)
        {
            if (dg.GoogleResult != null && dg.GoogleResult.Any(x => x.Supplier == supplier.ToString()))
                return;

            var (_driver, profilePath) = CreateDriver(9500 + index, false);

            List<GoogleSearchResult>? productData = null;

            try
            {
                var builder = CrawlHelper.BuildSupplierUri(dg.Name, supplier);
                _driver.Navigate().GoToUrl(builder.Uri.AbsoluteUri);

                var waitForPageLoad = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                waitForPageLoad.Until(driver =>
                    ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").ToString() == "complete");

                ((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
                Thread.Sleep(1000); // let JS render

                WebDriverWait wait = new(_driver, TimeSpan.FromSeconds(30));
                By locator = supplier switch
                {
                    Supplier.SharafDG => By.XPath("//div[@id='hits']"),
                    _ => By.XPath("//div[@class='ProductListDesktop_layoutWrapper__Kiw3A']")
                };
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(locator));

                var searchResult = _driver.FindElements(By.XPath(CrawlHelper.GetSearchResultXPath(supplier)));
                if (searchResult == null || searchResult.Count == 0)
                {
                    Log.Warning("WaitForSearchResult >> Found zero elements for supplier {Supplier}", supplier);
                    return;
                }

                var titles = CrawlHelper.ExtractTitles(searchResult, supplier);
                var prices = CrawlHelper.ExtractPrices(searchResult, supplier);
                var productUrls = CrawlHelper.ExtractProductUrls(searchResult, supplier);

                if (!titles.Any())
                    return;

                var query = CrawlHelper.BuildMatchingQuery(dg.Name, titles);
                var gptResponse = await _gptClient.GetResultFromGPT(query);

                if (gptResponse?.choices.Any() == true)
                {
                    var firstResult = gptResponse.choices.FirstOrDefault();
                    var res = firstResult?.message.content?.Replace("\n", "").Split("  ");

                    if (res != null && res.Length > 0)
                    {
                        int _index = titles.IndexOf(res[0]);
                        if (_index != -1)
                        {
                            string price = supplier == Supplier.SharafDG
                                ? prices[_index].Replace("AED ", "")
                                : prices[_index];

                            productData =
                            [
                                new ()
                                {
                                    Title = titles[_index],
                                    BaseUrl = productUrls[_index],
                                    CreationTime = DateTime.Now,
                                    DGProductId = dg.Id,
                                    Supplier = supplier.ToString(),
                                    Price = price
                                }
                            ];
                        }
                    }
                }
            }catch(Exception ex)
            {
                Log.Warning("FetchSupplierLinks >> ProcessNoonMatching >> {Message}", new { Message = ex.Message });
            }
            finally
            {
                try
                {
                    if (productData != null)
                    {
                        Log.Information("FetchSupplierLinks >> UpdateGoogleSearchResultsAsync >> {Message}", new { Message = productData });

                        await _dGProductRepository.UpdateGoogleSearchResultsAsync(dg.Id, productData);
                    }

                    _driver.Quit();

                    Directory.Delete(profilePath, true);
                }
                catch (Exception ex)
                {
                    Log.Warning("FetchSupplierLinks >> ProcessNoonMatching >> ProfileCleanup__UpdateGoogleSearchResultsAsync >> {Message}", new { Message = ex.Message });
                }
            }

            
        }

    }

}

