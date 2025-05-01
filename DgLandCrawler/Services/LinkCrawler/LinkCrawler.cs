using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools;
using DgLandCrawler.Models;
using OpenQA.Selenium.Support.Extensions;
using DgLandCrawler.Data.Repository;
using Log = Serilog.Log;


namespace DgLandCrawler.Services.LinkCrawler
{
    public class LinkCrawler : ILinkCrawler, IDisposable
    {
        private readonly ChromeOptions _options;
        private readonly IWebDriver _driver;
        private readonly IDGProductRepository _dGProductRepository;
        public LinkCrawler(IDGProductRepository dGProductRepository)
        {
            _options = new ChromeOptions();
            _options.AddArgument("--start-maximized");
            _options.AddArgument("--remote-debugging-port=9222");
            _options.AddUserProfilePreference("download.default_directory", @"C:\Users\mhyri\Downloads\selenium_downloads");
            _options.AddUserProfilePreference("download.prompt_for_download", false);
            _options.AddUserProfilePreference("download.directory_upgrade", true);
            _options.AddUserProfilePreference("safebrowsing.enabled", true);
            _driver = new ChromeDriver(_options);
            DevToolsSession _devtools = ((ChromeDriver)_driver).GetDevToolsSession();
            _dGProductRepository = dGProductRepository;
        }

        public async Task GetGoogleProductLinks()
        {
            var query = await _dGProductRepository.GetUnCrawledGoolgeList();

            _driver.Navigate().GoToUrl("https://www.google.com/");
            _driver.FindElement(By.XPath("//a[text()='English']")).Click();
            

            foreach(var row in query)
            {
                await PostGoogleData(row);
            }
        }

        private async Task PostGoogleData(DGProductData row)
        {
            try
            {
                IWebElement searchElem = _driver.FindElements(By.TagName("textarea")).FirstOrDefault()!;
                _driver.ExecuteJavaScript("arguments[0].scrollIntoView(true);", searchElem);

                searchElem.Clear();

                await Task.Delay(500);

                searchElem.SendKeys(row.Name);

                searchElem.SendKeys(Keys.Enter);

                await Task.Delay(500);

                await SarchforSupplier(row);

                await _dGProductRepository.AddAsync(row);

                Random rnd = new Random();

                await Task.Delay(rnd.Next(10000, 15000));

            }
            catch(Exception e)
            {
                Log.Error("An error occurred: {Message}", e.Message);
            }
        }

        private Task SarchforSupplier(DGProductData row)
        {
            string noonScript = "return document.querySelectorAll('a[href*=\"noon\"]').length > 0;";
            string sharafDGScript = "return document.querySelectorAll('a[href*=\"sharafdg\"]').length > 0;";
            string amazonScript = "return document.querySelectorAll('a[href*=\"amazon\"]').length > 0;";

            //string script = "return document.getElementsByClassName('commercial-unit-desktop-rhs').length > 0;";
            //string script2 = "return document.getElementsByClassName('top-pla-group-inner').length > 0;";

            bool noonElement = _driver.ExecuteJavaScript<bool>(noonScript);
            bool sharafDGElement = _driver.ExecuteJavaScript<bool>(sharafDGScript);
            bool amazonElement = _driver.ExecuteJavaScript<bool>(amazonScript);
            //bool elementExists = _driver.ExecuteJavaScript<bool>(script);
            //bool elementExists2 = _driver.ExecuteJavaScript<bool>(script2);
            string url;

            if(noonElement)
            {
                var noon = _driver.FindElements(By.PartialLinkText("noon"));

                url = GetAttribute(noon, "href");

                var elements = _driver.FindElements(By.XPath("//a[contains(@href, 'noon')]"));

                url = GetAttribute(elements, "href");

                UpdateGoogleResult(row, url, "noon");
            }

            if(sharafDGElement)
            {
                var sh = _driver.FindElements(By.PartialLinkText("sharafdg"));

                url = GetAttribute(sh, "href");

                var elements = _driver.FindElements(By.XPath("//a[contains(@href, 'sharafdg')]"));

                url = GetAttribute(elements, "href");

                UpdateGoogleResult(row, url, "sharafdg");
            }

            if(amazonElement)
            {
                var az = _driver.FindElements(By.PartialLinkText("amazon"));

                url = GetAttribute(az, "href");

                var elements = _driver.FindElements(By.XPath("//a[contains(@href, 'amazon')]"));
                
                url = GetAttribute(elements, "href");

                UpdateGoogleResult(row, url, "amazon");
            }

            return Task.CompletedTask;
        }

        private static void UpdateGoogleResult(DGProductData row, string url, string supplier)
        {
            if(url.Split("?").Length > 1)
            {
                url = url.Split("?")[0];
            }

            row.GoogleResult.Add(new GoogleSearchResult
            {
                Title = "link",
                Price = "0",
                BaseUrl = url,
                CreationTime = DateTime.Now,
                Supplier = supplier
            });
        }

        private static string GetAttribute(System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> elements, string attribute)
        {
            if(elements.Count != 0)
            {
                return elements[0].GetAttribute(attribute);
            }

            return string.Empty;
        }

        public void Dispose()
        {
            _driver.Close();
            _driver.Dispose();
        }
    }
}
