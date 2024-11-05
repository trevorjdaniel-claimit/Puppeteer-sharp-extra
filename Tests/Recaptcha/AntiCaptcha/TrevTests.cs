using Microsoft.VisualStudio.TestTools.UnitTesting;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerExtraSharp.Plugins.Recaptcha;
using PuppeteerSharp;
using System.Threading.Tasks;

namespace Extra.Tests.Recaptcha.AntiCaptcha
{
    [TestClass]
    public class TrevTests
    {
        [TestMethod]
        public async Task TestInvoiceSummaryView()
        {
            // Initialization plugin builder
            var extra = new PuppeteerExtra();

            // Use stealth plugin
            extra.Use(new StealthPlugin());

            // use recaptcha busting plugin
            extra.Use(new RecaptchaPlugin(new PuppeteerExtraSharp.Plugins.Recaptcha.Provider.AntiCaptcha.AntiCaptcha("af5e36b189e0ec2654d382b11a5e5d08")));

            await new BrowserFetcher().DownloadAsync();

            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false,
                Args = new[] { "--window-size=1920,1080", "--safebrowsing-disable-enhanced-protection" },
                DefaultViewport = new ViewPortOptions() { Width = 1920, Height = 1080 }
            });

            var page = await browser.NewPageAsync();

            await page.GoToAsync("https://dpdgroupuk.my.salesforce-sites.com/Support/ClaimForm?recordId=vDdL5sPWRKjfChagE%2Buqyjv9zyaIEiWjwETxZy5yTTQ%3D");

            var trev = new PuppeteerExtraSharp.Plugins.Recaptcha.Recaptcha(null, null);

            await trev.GetKeyAsync(page);
        }
    }
}
