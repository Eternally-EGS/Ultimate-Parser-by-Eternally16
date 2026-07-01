using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using UltimateParser.Config;

namespace UltimateParser.Utils 
{
    public static class UserImitation
    {
        public static async Task RunAsync(IPage page, ParserConfig config)
        {
            if (config == null) return;

            if (config.ScrollImitation)
            {
                await SimulateScrollAsync(page);
            }

            if (config.MoveMouseImitation)
            {
                await SimulateMouseMovementAsync(page, config);
            }
        }

        private static async Task SimulateScrollAsync(IPage page)
        {
            try 
            {
                long lastHeight = 0;
                for (int i = 0; i < 8; i++) 
                { 
                    int scrollStep = Random.Shared.Next(800, 1300);
                    await page.EvaluateAsync($"window.scrollBy(0, {scrollStep});");
                    
                    await Task.Delay(Random.Shared.Next(300, 600)); 
                    
                    long currentHeight = Convert.ToInt64(await page.EvaluateAsync("document.body.scrollHeight"));
                    if (currentHeight == lastHeight) break; 
                    lastHeight = currentHeight;
                }
                
                await page.EvaluateAsync("window.scrollTo(0, 0);");
                await Task.Delay(500);
            } 
            catch 
            { 
                Logger.Log("Warn_Scroll_Imitation_Failed"); 
            }
        }

        private static async Task SimulateMouseMovementAsync(IPage page, ParserConfig config)
        {
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    int targetX = Random.Shared.Next(100, 700);
                    int targetY = Random.Shared.Next(100, 500);
                    
                    await page.Mouse.MoveAsync(targetX, targetY, new MouseMoveOptions { Steps = 5 });
                    await Task.Delay(Random.Shared.Next(200, 400));
                }

                if (!string.IsNullOrEmpty(config.MainSelector))
                {
                    var elements = await page.QuerySelectorAllAsync(config.MainSelector);
                    
                    int countToHover = Math.Min(elements.Count, config.ImitationStepsCount);

                    for (int i = 0; i < countToHover; i++)
                    {
                        var element = elements[Random.Shared.Next(elements.Count)];
                        
                        if (element != null && await element.IsVisibleAsync())
                        {
                            await element.HoverAsync();
                            await Task.Delay(Random.Shared.Next(400, 800));
                        }
                    }
                }
            }
            catch
            {
                Logger.Log("Warn_Mouse_Imitation_Failed");
            }
        }
    }
}