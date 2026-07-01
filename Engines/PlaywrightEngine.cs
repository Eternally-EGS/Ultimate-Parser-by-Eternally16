using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.XPath;
using UltimateParser.Config;
using UltimateParser.Export;
using UltimateParser.Parsers;
using UltimateParser.Utils;

namespace UltimateParser.Engines 
{
    public class PlaywrightEngine : IParserEngine 
    {
        public event Action<List<Dictionary<string, string>>>? OnCheckpoint = null;

        public async Task<List<Dictionary<string, string>>> GetParse(ParserConfig config) 
        {
            var results = new List<Dictionary<string, string>>();
            if (config == null) return results;

            int totalPages = config.Pages;

                // ================= ВАРИАНТ 1: ПОСТРАНИЧНЫЙ ПАРСИНГ (Pages > 0) =================
                if (totalPages > 0) 
                {
                    for (var i = 1; i <= totalPages; i++) 
                    {
                        if (UltimateParser_Main.isExit) 
                        { 
                            Logger.Log("App_Cancel");
                            break; 
                        }

                        string url = (config.Url ?? "").Replace("{Page}", i.ToString());
                        Logger.Log("Nav_Start", url);

                        IDocument? document = null;
                        try 
                        {
                            document = await PageLoader.GetPagePlaywrightAsync(url, config);
                            if (document != null)
                            {
                                Logger.Log("Html_Received", document.Source?.Text?.Length ?? 0);
                            }
                        } 
                        catch (Exception) 
                        {
                            Logger.Log("Err_Page_Skip", url, 1);
                            continue;
                        }

                        var items = GetElements(document, config);
                        int itemCount = items.Count();
                        
                        if (itemCount == 0) 
                        {
                            Logger.Log("Crit_Layout_Changed");
                            continue;
                        } 
                        else 
                        {
                            Logger.Log("Container_Found", itemCount);
                        }

                        int itemIndex = 0;
                        foreach (var item in items) 
                        {
                            itemIndex++;
                            Logger.Log("Item_Parse_Start", itemIndex, itemIndex);

                            var row = ParseItem(item, config, url);
                            if (TableProcessing.TableCP(row, config)) 
                            { 
                                results.Add(row); 
                            }
                        }

                        Logger.Log("Page_Done", i, results.Count);
                        OnCheckpoint?.Invoke(results);

                        await Task.Delay(Random.Shared.Next(config.MinDelay, config.MaxDelay));
                    }
                } 
                // ================= ВАРИАНТ 2: БЕСКОНЕЧНЫЙ СКРОЛЛ (Pages <= 0) =================
                else 
                {
                    string url = config.Url ?? "";
                    Logger.Log("Nav_Start", url);
                    
                    IDocument? document = null;
                    try 
                    {
                        document = await PageLoader.GetPagePlaywrightAsync(url, config);
                        if (document != null)
                        {
                            Logger.Log("Html_Received", document.Source?.Text?.Length ?? 0);
                        }
                    } 
                    catch (Exception) 
                    {
                        Logger.Log("Err_Page_Skip", url, 1);
                        return results;
                    }

                    int targetCount = Math.Abs(config.Pages);
                    var parsedLinks = new HashSet<string>(); // Защита от дублей

                    while (true) 
                    {
                        // 1. ЖЕСТКИЙ И ЕДИНСТВЕННЫЙ ВЫХОД ПО КОЛИЧЕСТВУ ИЗ КОНФИГА
                        if (results.Count >= targetCount) 
                        { 
                            Logger.Log("Page_Done", results.Count); 
                            break; 
                        }

                        if (UltimateParser_Main.isExit) 
                        { 
                            Logger.Log("App_Cancel");
                            break; 
                        }

                        var items = GetElements(document, config);
                        int itemCount = items.Count();

                        // Если на странице вообще исчезли карточки (например, бан поймали или пустая страница)
                        if (itemCount == 0) 
                        {
                            Logger.Log("Crit_Layout_Changed");
                            break;
                        }

                        // Парсим ВСЁ, что нашли на текущем снимке страницы
                        foreach (var item in items) 
                        {
                            var row = ParseItem(item, config, url);
                            
                            string uniqueKey = row.TryGetValue("Link", out var l) && !string.IsNullOrEmpty(l) ? l : (row.TryGetValue("Title", out var t) ? t : "");
                            
                            // Если элемент уже собирали — просто пропускаем его и идем к СЛЕДУЮЩЕМУ в foreach
                            if (!string.IsNullOrEmpty(uniqueKey) && parsedLinks.Contains(uniqueKey)) 
                                continue;

                            // Если карточка валидная (не реклама), добавляем в базу
                            if (TableProcessing.TableCP(row, config)) 
                            { 
                                results.Add(row);
                                if (!string.IsNullOrEmpty(uniqueKey)) parsedLinks.Add(uniqueKey);
                                
                                // Стреляем на выход, если добрали до лимита прямо посреди страницы
                                if (results.Count >= targetCount) break;
                            }
                        }

                        Logger.Log("Container_Found", results.Count);
                        OnCheckpoint?.Invoke(results);

                        // Проверяем лимит еще раз после foreach
                        if (results.Count >= targetCount) 
                        { 
                            Logger.Log("Page_Done", results.Count); 
                            break; 
                        }

                        // Просто пинаем скролл дальше, без паники и лишних проверок счетчиков
                        var page = PlaywrightUtils.GetCurrentPage();
                        if (page != null)
                        {
                            var nextDoc = await PlaywrightUtils.ScrollAndGetNextPageAsync(page, config);
                            if (nextDoc != null)
                            {
                                document = nextDoc; // Обновляем DOM-дерево для следующего круга while
                            }
                            else
                            {
                                // Если скролл вернул null (совсем беда с браузером), стопаем
                                break;
                            }
                        }

                        await Task.Delay(Random.Shared.Next(config.MinDelay, config.MaxDelay));
                    }
                }
            
            return results;
        }

        // Вспомогательный метод сбора элементов по типу селектора
        private IEnumerable<IElement> GetElements(IDocument? document, ParserConfig config)
        {
            if (config.MainSelectorType == "XPath") 
            {
                var nodes = document?.DocumentElement?.SelectNodes(config.MainSelector ?? "");
                return nodes?.OfType<IElement>().ToList() ?? new List<IElement>();
            }
            return document?.QuerySelectorAll(config.MainSelector ?? "") ?? Enumerable.Empty<IElement>();
        }

        // Общая логика обработки полей одной карточки товара
        private Dictionary<string, string> ParseItem(IElement item, ParserConfig config, string url)
        {
            var row = new Dictionary<string, string>();

            foreach (var field in config.Fields ?? new List<FieldConfig>()) 
            {
                if (field == null) continue;

                string localName = field.Name ?? "";
                string localSelector = field.Selector ?? "";
                string localAttribute = field.Attribute ?? "";
                
                IElement? element = null;

                if (field.Flags != null && field.Flags.Contains(5)) 
                {
                    if (!string.IsNullOrEmpty(localSelector)) 
                    {
                        if (!localSelector.StartsWith(".")) localSelector = "." + localSelector;
                        element = item.SelectSingleNode(localSelector) as IElement;
                    }
                } 
                else 
                {
                    element = string.IsNullOrEmpty(localSelector) ? item : item.QuerySelector(localSelector);
                }

                if (element == null) 
                { 
                    Logger.Log("Warn_No_Field", localName, localName);
                    row[localName] = "";
                    continue; 
                }

                string value;
                if (!string.IsNullOrEmpty(localAttribute)) 
                {
                    value = element.GetAttribute(localAttribute) ?? "";
                    if (string.IsNullOrEmpty(value))
                    {
                        Logger.Log("Warn_No_Field", localName, localAttribute);
                        row[localName] = "";
                        continue;
                    }
                } 
                else 
                {
                    value = element.TextContent.Trim();
                }
                
                // Жесткая обрезка строки перед отправкой во флаги и экспорт (Решает падение лимита в 32,767 символов)
                if (value.Length > 32000)
                {
                    value = value.Substring(0, 32000) + "... [Текст обрезан из-за лимита ячейки]";
                }

                string endValue = FlagSystem.GetFlag(value, field, url) ?? "";
                row[localName] = endValue;
            }

            return row;
        }
    }
}