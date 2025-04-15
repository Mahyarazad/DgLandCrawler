using OpenQA.Selenium;

namespace DgLandCrawler.Services.SiteCrawler
{
    public static class DownloadService
    {
        public static string GetDownloadedFileName(IWebDriver driver, int waitTime)
        {
            // Open a new tab
            ((IJavaScriptExecutor)driver).ExecuteScript("window.open()");

            // Switch to the new tab
            driver.SwitchTo().Window(driver.WindowHandles[driver.WindowHandles.Count - 1]);

            // Navigate to chrome://downloads
            driver.Navigate().GoToUrl("chrome://downloads");

            // Define the end time (time when the wait will expire)
            DateTime endTime = DateTime.Now.AddSeconds(waitTime);

            while(DateTime.Now < endTime)
            {
                try
                {
                    // Get download percentage
                    var downloadPercentage = (int)((IJavaScriptExecutor)driver).ExecuteScript(
                        "return document.querySelector('downloads-manager').shadowRoot.querySelector('#downloadsList downloads-item').shadowRoot.querySelector('#progress').value");

                    // Check if the download is complete (100%)
                    if(downloadPercentage == 100)
                    {
                        // Get and return the file name once the download is completed
                        return (string)((IJavaScriptExecutor)driver).ExecuteScript(
                            "return document.querySelector('downloads-manager').shadowRoot.querySelector('#downloadsList downloads-item').shadowRoot.querySelector('div#content  #file-link').textContent");
                    }
                }
                catch
                {
                    // If any error occurs (like element not found), continue and retry
                }

                // Wait for 1 second before checking again
                Thread.Sleep(1000);
            }

            // Return null if download is not completed in the specified time
            return null;
        }
    }
}

