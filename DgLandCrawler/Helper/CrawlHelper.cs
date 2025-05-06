
using DgLandCrawler.Models;
using DgLandCrawler.Models.DTO;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serilog;
using static DgLandCrawler.Services.SiteCrawler.SiteCrawlerService;

namespace DgLandCrawler.Helper
{
    public static class CrawlHelper
    {
        public static UriBuilder BuildSupplierUri(string productName, Supplier supplier)
        {
            if (supplier == Supplier.Noon)
            {
                return new UriBuilder("https", "www.noon.com") { Path = "uae-en/search", Query = $"q={productName}" };
            }
            else // SharafDG
            {
                return new UriBuilder("https", "uae.sharafdg.com") { Query = $"q={productName}&post_type=product" };
            }
        }

        public static IReadOnlyCollection<IWebElement>? WaitForSearchResult(IWebDriver driver, Supplier supplier, DGProductData dg)
        {
            try
            {
                var builder = BuildSupplierUri(dg.Name, supplier);

                driver.Navigate().GoToUrl(builder.Uri.AbsoluteUri);

                new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(driver =>
                    ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").ToString() == "complete");

                var jsReady = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                    .Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");


                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
                    Thread.Sleep(1000); // give JS time to render

                WebDriverWait wait = new(driver, TimeSpan.FromSeconds(30));

                By locator = supplier switch
                {
                    Supplier.SharafDG => By.XPath("//div[@id='hits']"),
                    _ => By.XPath("//div[contains(@class, 'ProductListDesktop_layoutWrapper')]")
                };

                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(locator));

                var results = driver.FindElements(By.XPath(GetSearchResultXPath(supplier)));

                if (results.Count == 0)
                {
                    Log.Warning("WaitForSearchResult >> Found zero elements for supplier {Supplier}", supplier);
                }

                return results;
            }
            catch (Exception ex)
            {
                Log.Error("WaitForSearchResult >> Error: {Message}", ex.Message);
                return null;
            }
        }


        public static By GetSupplierContainerDiv(Supplier supplier)
        {
            return supplier == Supplier.Noon
                ? By.XPath("//div[@class='ProductListDesktop_layoutWrapper__Kiw3A']")
                : By.XPath("//div[@id='hits']");
        }

        public static string GetSearchResultXPath(Supplier supplier)
        {
            return supplier == Supplier.Noon
                ? "//*[@id='catalog-page-container']/div/div[2]/div[2]/div[4]/div"
                : "//div[@id='hits']";
        }

        public static List<string> ExtractTitles(IReadOnlyCollection<IWebElement> searchResult, Supplier supplier)
        {
            string titleXPath = supplier == Supplier.Noon
                ? ".//h2[@class='ProductDetailsSection_title__JorAV']"
                : ".//h4[@class='name']";

            return searchResult
                .SelectMany(x => x.FindElements(By.XPath(titleXPath)))
                .Select(e => e.Text)
                .ToList();
        }

        public static List<string> ExtractPrices(IReadOnlyCollection<IWebElement> searchResult, Supplier supplier)
        {
            string priceXPath = supplier == Supplier.Noon
                ? ".//strong[@class='Price_amount__2sXa7']"
                : ".//div[@class='price']";

            return searchResult
                .SelectMany(x => x.FindElements(By.XPath(priceXPath)))
                .Select(e => e.Text)
                .ToList();
        }

        public static List<string> ExtractProductUrls(IReadOnlyCollection<IWebElement> searchResult, Supplier supplier)
        {
            // Both Noon and SharafDG seem to have direct anchor links inside each item
            return searchResult
                .SelectMany(x => x.FindElements(By.XPath("./a")))
                .Select(e => e.GetAttribute("href"))
                .ToList();
        }

        public static string BuildMatchingQuery(string productTitle, List<string> titles)
        {
            return $"I have a list of product titles: [{string.Join("; ", titles)}]. " +
                   $"My product title is: \"{productTitle}\". " +
                   $"First, check for an exact match and return only the matched title. " +
                   $"If no exact match exists, find and return only the closest matching product title based on model, storage, and color. " +
                   $"Return only the product title, no explanation or extra text. " +
                   $"If no match exists, reply exactly with 'No Match'.";
        }

        public static Task AdminLogin(AdminPanelCredential credential, IWebDriver _driver)
        {
            _driver.Navigate().GoToUrl("https://dgland.ae/wp-admin/");

            _driver.FindElement(By.Id("user_login")).SendKeys(credential.Useranme);

            _driver.FindElement(By.Id("user_pass")).SendKeys(credential.Password);

            _driver.FindElement(By.Id("wp-submit")).Click();

            return Task.CompletedTask;
        }
    }
}
