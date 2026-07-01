using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using UltimateParser.Config;

namespace UltimateParser.Utils 
{
    public static class SimplePageLoader 
    {
        private static IPlaywright? _playwright;
        private static IBrowser? _browser;
        private static IBrowserContext? _context;
        private static IPage? _page;
        private static ProxyManager? _proxyManager;
        private static readonly SemaphoreSlim _lock = new(1, 1);

        public static async Task InitBrowserAsync(ParserConfig config) 
        {
            if (_browser != null) return;

            await _lock.WaitAsync();
            try 
            {
                if (_browser == null)
                {
                    _playwright = await Playwright.CreateAsync();
                    
                    var launchOptions = new BrowserTypeLaunchOptions 
                    { 
                        Headless = config.Headless,
                        Args = new[] { "--disable-dev-shm-usage", "--no-sandbox", "--disable-gpu", "--disable-extensions","--disable-blink-features=AutomationControlled" }
                    };

                    if (!string.IsNullOrWhiteSpace(config.ExecutablePath))
                        launchOptions.ExecutablePath = config.ExecutablePath;

                    _browser = await _playwright.Chromium.LaunchAsync(launchOptions);
                    
                    if (config.UseProxy && config.Proxies?.Count > 0)
                        _proxyManager = new ProxyManager(config.Proxies);
                }

                // Создаем контекст с прокси (если нужно)
                var contextOptions = new BrowserNewContextOptions();
                if (config.UseProxy && _proxyManager != null)
                {
                    var proxyUri = new Uri(_proxyManager.GetNextProxy()!);
                    contextOptions.Proxy = new Proxy { Server = $"{proxyUri.Scheme}://{proxyUri.Host}:{proxyUri.Port}" };
                }

                _context = await _browser.NewContextAsync(contextOptions);
                _page = await _context.NewPageAsync();

            if (!config.JS) 
            {
                await _page.RouteAsync("**/*.{png,jpg,jpeg,gif,webp,svg,css,woff,woff2,ttf}", async route => {
                    var type = route.Request.ResourceType;
                    if (type == "image" || type == "font" || type == "stylesheet") await route.AbortAsync();
                    else await route.ContinueAsync();
                });
            }

                await _page.AddInitScriptAsync(@"() => { Object.defineProperty(navigator, 'webdriver', { get: () => undefined }); }");
            }
            finally { _lock.Release(); }
        }

        public static async Task GotoAsync(string url) 
        {
            if (_page == null) throw new Exception("Браузер не инициализирован!");
            await _page.GotoAsync(url);
        }

        public static async Task ImageCleanup () {
            await _page.EvaluateAsync(@"() => {
            document.querySelectorAll('img').forEach(img => img.remove());
            document.querySelectorAll('.ads-block, .banner').forEach(el => el.remove());
        }");
        }

        public static async Task<string> GetHtmlAsync() 
        {
            return await _page!.ContentAsync();
        }

        public static async Task ScrollDownAsync(ParserConfig config) 
        {
            if (_page == null) return;
            await _page.EvaluateAsync($"window.scrollBy(0, {Random.Shared.Next(config.ImitationStepsCount/2, config.ImitationStepsCount)})");
        }

        public static async Task CloseAsync()
        {
            if (_browser != null) await _browser.CloseAsync();
            _playwright?.Dispose();
            _page = null;
            _browser = null;
        }
    }
}