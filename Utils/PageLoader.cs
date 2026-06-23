using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using AngleSharp.Io.Network;
using Microsoft.Playwright;
using UltimateParser.Config;

namespace UltimateParser.Utils 
{
    public class ProxyRotation : DelegatingHandler {
        private readonly string[] _proxies;
        private int currentIndex = 0;

        public ProxyRotation(string[] proxy) : base(new HttpClientHandler()){
            _proxies = proxy ?? Array.Empty<string>();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken Token){
            if (_proxies.Length == 0) {
                return base.SendAsync(request, Token);
            }

            int index = Interlocked.Increment(ref currentIndex) % _proxies.Length;
            string currentProxy = _proxies[Math.Abs(index)];

            Logger.ConsoleOutput($"[PROXY] Запрос через: {currentProxy}", 2);

            if (InnerHandler is HttpClientHandler clientHandler) {
                if (Uri.TryCreate(currentProxy, UriKind.Absolute, out var uri)) {
                    var webProxy = new WebProxy($"{uri.Scheme}://{uri.Host}:{uri.Port}");
                    
                    if (!string.IsNullOrEmpty(uri.UserInfo)) {
                        var userInfo = uri.UserInfo.Split(':');
                        webProxy.Credentials = new NetworkCredential(userInfo[0], userInfo.Length > 1 ? userInfo[1] : "");
                    }
                    
                    clientHandler.Proxy = webProxy;
                    clientHandler.UseProxy = true;
                }
            }

            return base.SendAsync(request, Token);
        }
    }

    public static class PageLoader {
        private static HttpClient? httpclient;
        private static readonly object _Lock = new object();

        private static IPlaywright? _playwright;
        private static IBrowser? _browser;
        private static IBrowserContext? _context;
        private static IPage? _page;

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

        public static async Task<IDocument> GetPageAsync (string url, string UserAgent, ParserConfig config) {
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

        public static async Task<IDocument> GetPagePlaywrightAsync(string url, ParserConfig config) {
            if (_page == null) {
                _playwright = await Playwright.CreateAsync();

                var launchOptions = new BrowserTypeLaunchOptions { Headless = config.Headless };
                 if (config.UseProxy && config.Proxies != null && config.Proxies.Count > 0) {
                    var activeProxy = config.Proxies.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p));
                    if (activeProxy != null) {
                        if (Uri.TryCreate(activeProxy, UriKind.Absolute, out var uri) && !string.IsNullOrEmpty(uri.UserInfo)) {
                            var userInfo = uri.UserInfo.Split(':');
                            launchOptions.Proxy = new Proxy { 
                                Server = $"{uri.Scheme}://{uri.Host}:{uri.Port}",
                                Username = userInfo[0],
                                Password = userInfo.Length > 1 ? userInfo[1] : ""
                            };
                        } else {
                            launchOptions.Proxy = new Proxy { Server = activeProxy };
                        }
                        Logger.ConsoleOutput($"[Playwright] Браузер запускается через прокси: {activeProxy}", 2);
                    }
                }

                _browser = await _playwright.Chromium.LaunchAsync(launchOptions);

                var contextOptions = new BrowserNewContextOptions {
                    UserAgent = config.UserAgent ?? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                    Locale = config.Locale ?? "en-US",
                    TimezoneId = config.TimezoneId ?? "America/New_York"
                };

                _context = await _browser.NewContextAsync(contextOptions);
                _page = _context.Pages.FirstOrDefault() ?? await _context.NewPageAsync();
                await _page.SetViewportSizeAsync(800, 600);

                // Protaction

                await _page.AddInitScriptAsync(@"() => {
                    Object.defineProperty(navigator, 'webdriver', { get: () => undefined });

                    Object.defineProperty(navigator, 'plugins', {
                        get: () => [
                            { name: 'Chrome PDF Viewer', filename: 'internal-pdf-viewer' },
                            { name: 'Chromium PDF Viewer', filename: 'internal-pdf-viewer' }
                        ]
                    });

                    Object.defineProperty(navigator, 'languages', { get: () => ['ru-RU', 'ru', 'en-US', 'en'] });

                    Object.defineProperty(navigator, 'hardwareConcurrency', { get: () => 8 });
                    Object.defineProperty(navigator, 'devicePixelRatio', { get: () => 1 });
                }");


            if (!config.JS) {
                await _page.RouteAsync("**/*.{png,jpg,jpeg,gif,webp,svg,css,woff,woff2,ttf}", async route => {
                        var type = route.Request.ResourceType;
                        if (type == "image" || type == "font" || type == "stylesheet") {
                            await route.AbortAsync();
                        } else {
                            await route.ContinueAsync();
                        }
                    });
            }
            }

            // Randomization tick
            int Min = config.MinDelay;
            int Max = config.MaxDelay;

            if(Min> 0 && Max > Min){ 
                int delay = Random.Shared.Next(Min,Max);
                await Task.Delay(delay);
            }

            await _page.GotoAsync(url);

            // Scroll
            if (config.ScrollImitation) {
                try {
                await _page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight / 2);");
                await Task.Delay(Random.Shared.Next(500, 1500)); 
                await _page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight);");
                await Task.Delay(Random.Shared.Next(500, 1000));
                await _page.EvaluateAsync("window.scrollTo(0, 0);");
                } catch {
                    Logger.ConsoleOutput("[ScrollImitation] не удался!!",1);
                }
            }

            if (!string.IsNullOrEmpty(config.WaitForSelector)) {
                await _page.WaitForSelectorAsync(config.WaitForSelector, new PageWaitForSelectorOptions {
                    Timeout = config.Timeout
                });
            }

            var html = await _page.ContentAsync();

            var angleConfig = Configuration.Default.WithDefaultLoader();
            var angleContext = BrowsingContext.New(angleConfig);
            
            return await angleContext.OpenAsync(reg => reg.Content(html));
        }
    }
}