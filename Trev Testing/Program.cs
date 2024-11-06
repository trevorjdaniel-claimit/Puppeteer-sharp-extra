// Initialization plugin builder
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerExtraSharp.Plugins.Recaptcha;
using PuppeteerExtraSharp;
using PuppeteerSharp;
using PuppeteerExtraSharp.Plugins.Recaptcha.Provider.AntiCaptcha;

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

//await page.GoToAsync("https://dpdgroupuk.my.salesforce-sites.com/Support/ClaimForm?recordId=vDdL5sPWRKjfChagE%2Buqyjv9zyaIEiWjwETxZy5yTTQ%3D");
await page.GoToAsync("https://www.google.com/recaptcha/api2/demo");

await page.WaitForNetworkIdleAsync();

//await page.TypeAsync(".slds-input", "3006995");

//await Task.Delay(3000);

//var service = new Recaptcha(null, null);
//await service.WriteToInput(page, "sddffdfdfdfs");

var recaptchaPlugin = new RecaptchaPlugin(new AntiCaptcha("af5e36b189e0ec2654d382b11a5e5d08"));
var recaptchaResult = await recaptchaPlugin.SolveCaptchaAsync(page);

var trev = "";