using UltimateParser.Config;
using UltimateParser.Export;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Playwright;

namespace UltimateParser.Utils 
{

public static class SimplePageLoader 
{
    private static IBrowser? _browser;
    private static IBrowserContext? _context;
    private static IPage? _page;

    // 1. Открытие браузера (один раз на старт)
    public static async Task InitBrowserAsync(ParserConfig config) 
    {
        if (_browser != null) return;
        var playwright = await Playwright.CreateAsync();
        _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = config.Headless });
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();
    }

    // 2. Просто переход по ссылке
    public static async Task GotoAsync(string url) 
    {
        if (_page == null) throw new Exception("Браузер не инициализирован!");
        await _page.GotoAsync(url);
    }

    public static async Task<string> GetHtmlAsync() 
    {
        return await _page!.ContentAsync();
    }

    
    public static async Task ScrollDownAsync() 
    {
        await _page!.EvaluateAsync("window.scrollBy(0, 150)");
    }
}

}