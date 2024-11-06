using System;
using System.Linq;
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
                var key = GetKeyAsync(page);
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

        public string GetKeyAsync(IPage page)
        {
            string key = string.Empty;
            var captchaFrame = page.Frames.Where(x => x.Url.Contains("recaptcha")).FirstOrDefault();
            if (captchaFrame != null)
            {
                key = HttpUtility.ParseQueryString(captchaFrame.Url).Get("k");
            }
            else
            {
                throw new Exception("ReCaptcha frame not found");
            }
            return key;

            //try
            //{
            //    var element =
            //        page.QuerySelectorAsync("iframe[src^='https://www.google.com/recaptcha/api2/anchor'][name^=\"a-\"]").Result;

            //    if (element == null)
            //        throw new CaptchaException(page.Url, "Recaptcha key not found!");

            //    var src = await element.GetPropertyAsync("src");

            //    if (src == null)
            //        throw new CaptchaException(page.Url, "Recaptcha key not found!");

            //    var key = HttpUtility.ParseQueryString(src.ToString()).Get("k");
            //    return key;
            //}
            //catch (Exception ex)
            //{
            //    return null;
            //}
        }

        public async Task<string> GetSolutionAsync(string key, string urlPage)
        {
            return await _provider.GetSolution(key, urlPage);
        }

        public async Task<IFrame> GetFrame(IPage page, string content)
        {
            IFrame frame = null;

            // loop through the frames checking the content
            foreach (var currentFrame in page.Frames)
            {
                var frameContent = await currentFrame.GetContentAsync();
                if (frameContent.Contains(content))
                    frame = currentFrame;
            }

            return frame;
        }

        public async Task WriteToInput(IPage page, string value)
        {
            // enter the code into the page
            var containingFrame = await GetFrame(page, "g-recaptcha-response");
            var recaptchaResponse = await containingFrame.QuerySelectorAsync("#g-recaptcha-response");
            await recaptchaResponse.EvaluateFunctionAsync($"() => {{document.getElementById('g-recaptcha-response').innerHTML='{value}'}}");

            // read the local js file
            var script = ResourcesReader.ReadFile(this.GetType().Namespace + ".Scripts.EnterRecaptchaCallBackScript.js");

            // find the captcha frame
            var captchaFrame = await GetFrame(page, "name=\"captchaFrame\"");
            var captchaFramsHead = await captchaFrame.QuerySelectorAllAsync("head");

            try
            {
                await captchaFrame.EvaluateFunctionAsync($@"(value) => {{{script}}}", value);
            }
            catch
            {
                // ignored
            }
        }
    }
}
