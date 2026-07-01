using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io.Network;
using Microsoft.Playwright;
using UltimateParser.Config;

namespace UltimateParser.Utils 
{
    public static class PageLoader 
    {
        private static HttpClient? _httpClient;
        private static ProxyManager? _proxyManager;
        private static readonly SemaphoreSlim _playwrightLock = new(1, 1);
        
        private static IPlaywright? _playwright;
        private static IBrowser? _browser;
        private static string _lastExecutablePath = ""; 

        private const int MaxAttempts = 3; 

        public static async Task DisposePlaywrightAsync()
        {
            try
            {
                Logger.Log("Disposing Playwright resources...");
                
                // Сначала жестко закрываем сам браузер, если он открыт
                if (_browser != null)
                {
                    await _browser.CloseAsync();
                    await _browser.DisposeAsync();
                    _browser = null;
                }

                // Затем тушим сам движок Playwright
                if (_playwright != null)
                {
                    _playwright.Dispose();
                    _playwright = null;
                }

                Logger.Log("Playwright disposed successfully.");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during Playwright dispose: {ex.Message}");
            }
        }

        private static void InitHttpClient(ParserConfig config) 
        {
            if (_httpClient != null) return;

            if (config?.UseProxy == true && config.Proxies?.Count > 0) 
            {
                _proxyManager ??= new ProxyManager(config.Proxies);
                var handler = new ProxyRotationHandler(_proxyManager);
                _httpClient = new HttpClient(handler);
            } 
            else 
            {
                _httpClient = new HttpClient();
            }

            if (config != null) 
            {
                _httpClient.Timeout = TimeSpan.FromMilliseconds(config.NetworkTimeout);
            }
        }

        public static async Task<IDocument> GetPageAsync(string url, ParserConfig config) 
        {
            InitHttpClient(config);

            var configstr = Configuration.Default
                .WithRequester(new HttpClientRequester(_httpClient ?? new HttpClient()))
                .WithDefaultLoader()
                .WithCookies();

            var context = BrowsingContext.New(configstr);

            for (int attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    var document = await context.OpenAsync(url ?? "");
                    
                    if (document.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception($"Bad status code: {document.StatusCode}");
                    }

                    return document; 
                }
                catch (Exception ex)
                {
                    Logger.Log("Log_HttpClient_Attempt_Failed", attempt, MaxAttempts, url ?? "", ex.Message);
                    
                    if (attempt == MaxAttempts) throw new Exception($"HttpClient не смог загрузить страницу после {MaxAttempts} попыток.", ex);
                    await Task.Delay(2000); 
                }
            }

            throw new Exception("Неизвестная ошибка цикла переподключений HttpClient.");
        }

        public static async Task<IDocument> GetPagePlaywrightAsync(string url, ParserConfig config) 
        {
            config ??= new ParserConfig();
            _proxyManager ??= new ProxyManager(config.Proxies);

            if (_browser != null && _lastExecutablePath != (config.ExecutablePath ?? ""))
            {
                await _playwrightLock.WaitAsync();
                try {
                    await _browser.CloseAsync();
                    _browser = null;
                } finally { _playwrightLock.Release(); }
            }

            if (_browser == null) 
            {
                await _playwrightLock.WaitAsync();
                try 
                {
                    if (_browser == null) 
                    {
                        _playwright = await Playwright.CreateAsync();
                        _lastExecutablePath = config.ExecutablePath ?? "";

                        // Жёсткие лимиты на ОЗУ и процессы
                        var launchOptions = new BrowserTypeLaunchOptions 
                        { 
                            Headless = config.Headless,
                            Args = new[] 
                            {
                                "--disable-dev-shm-usage",
                                "--no-sandbox",
                                "--disable-gpu",
                                "--disable-extensions",
                                "--js-flags=\"--max-old-space-size=256\"",
                                "--disable-background-networking",
                                "--disable-background-timer-throttling"
                            }
                        };
                        
                        if (!string.IsNullOrWhiteSpace(_lastExecutablePath))
                        {
                            launchOptions.ExecutablePath = _lastExecutablePath;
                        }

                        _browser = await _playwright.Chromium.LaunchAsync(launchOptions);
                    }
                }
                finally 
                {
                    _playwrightLock.Release();
                }
            }

            for (int attempt = 1; attempt <= MaxAttempts; attempt++)
            {

                IBrowserContext? context = null;
                IPage? page = null;
                try
                {
                    var contextOptions = new BrowserNewContextOptions 
                    {
                        UserAgent = config.UserAgent ?? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                        Locale = config.Locale ?? "en-US",
                        TimezoneId = config.TimezoneId ?? "America/New_York"
                    };

                    if (config.UseProxy)
                    {
                        var currentProxy = _proxyManager.GetNextProxy();
                        if (!string.IsNullOrEmpty(currentProxy) && Uri.TryCreate(currentProxy, UriKind.Absolute, out var uri))
                        {
                            contextOptions.Proxy = new Proxy { Server = $"{uri.Scheme}://{uri.Host}:{uri.Port}" };
                            if (!string.IsNullOrEmpty(uri.UserInfo))
                            {
                                var userInfo = uri.UserInfo.Split(':');
                                contextOptions.Proxy.Username = userInfo[0];
                                contextOptions.Proxy.Password = userInfo.Length > 1 ? userInfo[1] : "";
                            }
                        }
                    }

                    context = await _browser.NewContextAsync(contextOptions);
                    context.SetDefaultTimeout(config.NetworkTimeout);

                    page = await context.NewPageAsync();
                    await page.SetViewportSizeAsync(800, 600);

                    await page.AddInitScriptAsync(@"() => {
                        Object.defineProperty(navigator, 'webdriver', { get: () => undefined });
                        Object.defineProperty(navigator, 'languages', { get: () => ['ru-RU', 'ru', 'en-US', 'en'] });
                    }");

                    if (!config.JS) 
                    {
                        await page.RouteAsync("**/*.{png,jpg,jpeg,gif,webp,svg,css,woff,woff2,ttf}", async route => {
                            var type = route.Request.ResourceType;
                            if (type == "image" || type == "font" || type == "stylesheet") await route.AbortAsync();
                            else await route.ContinueAsync();
                        });
                    }

                    if (config.MinDelay > 0 && config.MaxDelay > config.MinDelay) 
                    {
                        await Task.Delay(Random.Shared.Next(config.MinDelay, config.MaxDelay));
                    }

                    var response = await page.GotoAsync(url ?? "", new PageGotoOptions {
                        Timeout = config.NetworkTimeout 
                    });
                    
                    if (response == null || !response.Ok)
                    {
                        throw new Exception($"Playwright не смог загрузить страницу. Статус: {response?.Status}");
                    }

                    await UserImitation.RunAsync(page, config);

                    if (!string.IsNullOrEmpty(config.WaitForSelector)) 
                    {
                        await page.WaitForSelectorAsync(config.WaitForSelector, new PageWaitForSelectorOptions { 
                            Timeout = config.SelectorTimeout 
                        });
                    }

                    var html = await page.ContentAsync();

                    var angleConfig = Configuration.Default.WithDefaultLoader();
                    var angleContext = BrowsingContext.New(angleConfig);
                    return await angleContext.OpenAsync(reg => reg.Content(html ?? ""));
                }
                catch (Exception ex)
                {
                    Logger.Log("Log_Playwright_Attempt_Failed", attempt, MaxAttempts, url ?? "", ex.Message);
                    
                    if (attempt == MaxAttempts) throw new Exception($"Playwright не смог загрузить страницу после {MaxAttempts} попыток.", ex);
                    await Task.Delay(3000); 
                }
                finally
                {
                   /// if (page != null) { try { await page.CloseAsync(); } catch { } }
                  //  if (context != null) { try { await context.CloseAsync(); } catch { } }
                    
                  //  GC.Collect();
                   // GC.WaitForPendingFinalizers();
                }

                // save and exit
                if (UltimateParser_Main.isExit) { 
                    Logger.Log("App_Cancel");
                    break; 
                }

            }

            throw new Exception("Неизвестная ошибка цикла переподключений Playwright.");
        }
    }
}