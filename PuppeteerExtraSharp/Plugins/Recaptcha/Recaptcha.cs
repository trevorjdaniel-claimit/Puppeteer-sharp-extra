using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using PuppeteerExtraSharp.Plugins.Recaptcha.Provider;
using PuppeteerExtraSharp.Utils;
using PuppeteerSharp;

namespace PuppeteerExtraSharp.Plugins.Recaptcha
{
    public class Recaptcha
    {
        private readonly IRecaptchaProvider _provider;
        private readonly CaptchaOptions _options;

        public Recaptcha(IRecaptchaProvider provider, CaptchaOptions options)
        {
            _provider = provider;
            _options = options;
        }

        public async Task<RecaptchaResult> Solve(IPage page)
        {
            try
            {
                var key = await GetKeyAsync(page);
                var solution = await GetSolutionAsync(key, page.Url);
                await WriteToInput(page, solution);

                return new RecaptchaResult()
                {
                    IsSuccess = true
                };
            }
            catch (CaptchaException ex)
            {
                return new RecaptchaResult()
                {
                    Exception = ex,
                    IsSuccess = false
                };
            }

        }

        public async Task<string> GetKeyAsync(IPage page)
        {
            IElementHandle element = await page.QuerySelectorAsync("iframe[src^='https://www.google.com/recaptcha/api2/anchor'][name^=\"a-\"]");

            string siteKey = null;

            if (element == null)
            {
                IFrame frame = page.Frames.Single(f => f.Url.Contains("https://www.google.com/recaptcha/api2/anchor"));

                siteKey = HttpUtility.ParseQueryString(frame.Url).Get("k");
            }
            else
            {
                var src = await element.GetPropertyAsync("src");

                if (src == null)
                    throw new CaptchaException(page.Url, "Recaptcha key not found!");

                siteKey = HttpUtility.ParseQueryString(src.ToString()).Get("k");
            }

            if (siteKey == null)
                throw new CaptchaException(page.Url, "No site key found!");

            return siteKey;
        }

        public async Task<string> GetSolutionAsync(string key, string urlPage)
        {
            return await _provider.GetSolution(key, urlPage);
        }

        public async Task WriteToInput(IPage page, string value)
        {
            string script = ResourcesReader.ReadFile(GetType().Namespace + ".Scripts.EnterRecaptchaCallBackScript.js");

            try
            {
                await page.EvaluateFunctionAsync(
                      $"() => {{document.getElementById('g-recaptcha-response').innerHTML='{value}'}}");

                try
                {
                    await page.EvaluateFunctionAsync($@"(value) => {{{script}}}", value);
                }
                catch
                {
                    throw new InvalidOperationException("Unable to execute JS against the captcha response element");
                }
            }
            catch(Exception)
            {
                IFrame[] frames = [.. page.Frames.Where(f => f.Url.Contains("salesforce-sites"))];

                IFrame found = null;

                foreach (IFrame frame in frames)
                {
                    if (frame.Name.Equals("captchaFrame"))
                    {
                        found = frame;
                    }
                }

                if (found == null)
                {
                    throw new InvalidOperationException("Unable to locate captcha frame after solution.");
                }

                IElementHandle responseElement = await found.QuerySelectorAsync("#g-recaptcha-response");

                await responseElement.EvaluateFunctionAsync(
                      $"() => {{document.getElementById('g-recaptcha-response').innerHTML='{value}'}}");

                try
                {
                    //await page.EvaluateFunctionAsync($@"(value) => {{{js}}}", value);
                    await found.EvaluateFunctionAsync($@"(value) => {{{script}}}", value);
                }
                catch
                {
                    throw new InvalidOperationException("Unable to execute JS against the captcha response element");
                }

                //bool success = false;
                //foreach (IFrame frame in page.Frames)
                //{
                //    try
                //    {
                //        await frame.EvaluateFunctionAsync($@"(value) => {{{js}}}", value);

                //        success = true;

                //        break;
                //    }
                //    catch
                //    {

                //    }
                //}
            }
        }
    }
}
