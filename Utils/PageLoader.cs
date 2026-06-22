using System.Net;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using AngleSharp.Io.Network;
using UltimateParser.Config;

namespace UltimateParser.Utils 
{
    public class ProxyRotation : DelegatingHandler {
        private readonly string[] _proxies;
        private int currentIndex = 0;

        public ProxyRotation(string[] proxy) : base(new HttpClientHandler()){
            _proxies = proxy ?? Array.Empty<string>();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken Token){
            
            if (_proxies.Length == 0) {
                return base.SendAsync(request,Token);
            }

            int index = Interlocked.Increment(ref currentIndex) % _proxies.Length;
            string currentProxy = _proxies[Math.Abs(index)];

            Logger.ConsoleOutput($"[PROXY] Запрос через: {currentProxy}",2);

            if (InnerHandler is HttpClientHandler clientHandler) {
                clientHandler.Proxy = new WebProxy(currentProxy);
                clientHandler.UseProxy = true;
            }

            return base.SendAsync(request,Token);
        }
    }

    public static class PageLoader {
        private static HttpClient? httpclient;
        private static readonly object _Lock = new object();

        private static void InitalComponent(ParserConfig config) {
            if (httpclient != null) return;

            lock (_Lock) {
                if (config.UseProxy && config.Proxies != null && config.Proxies.Count > 0) {
                    var activeproxy = config.Proxies.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();

                    if (activeproxy.Length > 0) {
                        var proxyhand = new ProxyRotation(activeproxy);
                        httpclient = new HttpClient(proxyhand);
                    } else {
                        httpclient = new HttpClient();
                    }
                } else {
                    httpclient = new HttpClient();
                }

                httpclient.Timeout = TimeSpan.FromSeconds(config.TimeOut);
            }
        }

        public static async Task<IDocument> GetPageAsync (string url,string UserAgent,ParserConfig config) {

            //Logger.ConsoleOutput($"UseProxy: {config.UseProxy} Proxy Count:{config.Proxies?.Count ?? 0}",2);

            InitalComponent(config);

            var req = new DefaultHttpRequester();
            req.Headers["User-Agent"] = UserAgent ?? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

            var configstr = Configuration.Default
                .WithRequester(new HttpClientRequester(httpclient))
                .WithDefaultLoader()
                .WithCookies();

            var context = BrowsingContext.New(configstr);
            return await context.OpenAsync(url);
        }
    }
}