using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using AngleSharp.Io.Network;

namespace UltimateParser.Utils 
{
    public static class PageLoader {

        public static async Task<IDocument> GetPageAsync (string url,string UserAgent) {

            var HTTPClient = new HttpClient();
            var req = new DefaultHttpRequester();

            HTTPClient.Timeout = TimeSpan.FromSeconds(10);
            req.Headers["User-Agent"] = UserAgent ?? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

            var configstr = Configuration.Default
                .WithRequester(new HttpClientRequester(HTTPClient))
                .WithDefaultLoader()
                .WithRequesters();

            var context = BrowsingContext.New(configstr);
            return await context.OpenAsync(url);

        }
    }
}