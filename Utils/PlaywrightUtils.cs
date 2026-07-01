using System;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Playwright;
using UltimateParser.Config;

namespace UltimateParser.Utils 
{
    public static class PlaywrightUtils 
    {
        private static IPage? _currentPage;

        // Фиксируем текущую активную страницу, которую открыл PageLoader
        public static void SetCurrentPage(IPage page)
        {
            _currentPage = page;
        }

        // Отдаем страницу движку, если понадобится напрямую
        public static IPage? GetCurrentPage()
        {
            return _currentPage;
        }

        // Самая магия: крутит страницу вниз и отдаёт AngleSharp новый снимок DOM
        public static async Task<IDocument?> ScrollAndGetNextPageAsync(IPage page, ParserConfig config)
        {
            if (page == null) return null;

            try
            {
                // 1. Скроллим само окно до самого низа
                await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight);");

                // 2. Хитрый скрипт: ищем внутри Яндекса бесконечные контейнеры ( scrollable elements ) и принудительно опускаем их вниз
                await page.EvaluateAsync(@"() => {
                    const scrollableDivs = document.querySelectorAll('div, main, section');
                    for (let el of scrollableDivs) {
                        if (el.scrollHeight > el.clientHeight && window.getComputedStyle(el).overflowY !== 'hidden') {
                            el.scrollTop = el.scrollHeight;
                        }
                    }
                }");

                // 3. Эмулируем нажатие клавиши END прямо в браузер (это заставляет фокус упасть в пол)
                await page.Keyboard.PressAsync("End");
                
                // Имитация движения мыши, чтобы Яндекс думал, что за компом живой человек
                if (config.MoveMouseImitation)
                {
                    int randomX = Random.Shared.Next(200, 600);
                    int randomY = Random.Shared.Next(200, 600);
                    await page.Mouse.MoveAsync(randomX, randomY);
                }

                // ЖЕСТКАЯ ПАУЗА: Даем Яндексу время (минимум 2-3 секунды) сообразить и подгрузить Ajax-запрос
                int delay = Random.Shared.Next(config.MinDelay, config.MaxDelay);
                if (delay < 2500) delay = Random.Shared.Next(2500, 4000); 
                await Task.Delay(delay);

                // Ждем, пока на странице появится ХОТЯ БЫ ОДИН новый элемент (чтобы не забирать пустой или старый HTML)
                if (!string.IsNullOrEmpty(config.WaitForSelector))
                {
                    try
                    {
                        await page.WaitForSelectorAsync(config.WaitForSelector, new PageWaitForSelectorOptions 
                        { 
                            Timeout = 4000 
                        });
                    }
                    catch { }
                }

                // Забираем обновленный HTML
                string html = await page.ContentAsync();

                var angleConfig = Configuration.Default.WithDefaultLoader();
                var angleContext = BrowsingContext.New(angleConfig);
                return await angleContext.OpenAsync(reg => reg.Content(html ?? ""));
            }
            catch (Exception ex)
            {
                Logger.Log("Log_Playwright_Attempt_Failed", 0, 0, page.Url, $"Ошибка скролла: {ex.Message}");
                return null;
            }
        }
    }
}