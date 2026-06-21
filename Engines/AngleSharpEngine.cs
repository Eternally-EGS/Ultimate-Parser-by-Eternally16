using AngleSharp.Dom;
using UltimateParser.Config;
using UltimateParser.Parsers;
using UltimateParser.Utils;
using HtmlAgilityPack;
using ClosedXML.Excel;
using AngleSharp.XPath;

namespace UltimateParser.Engines 
{
    public class AngelSharpEngine {
        public event Action<List<Dictionary<string,string>>>? OnCheckpoint = null;
        public async Task<List<Dictionary<string,string>>> GetParse (ParserConfig config) {
            
            var results = new List<Dictionary<string,string>>();

            for (var i = 1;i<= config?.Pages;i++) {

                // save and exit
                if (UltimateParser_Main.isExit) { break; }
                
                // Buffer
                OnCheckpoint?.Invoke(results);

            // Pages select

            string url = config.Url.Replace("{Page}",i.ToString());

            IDocument? document = null;
            
            // Create base page
            try {
                Logger.ConsoleOutput($"Подключение... к: {url}",2);
                document = await PageLoader.GetPageAsync(url,config.UserAgent);
            } 
            catch (Exception ex) {
            Logger.ConsoleOutput($"Ошибка подключния к: {config?.Url ?? ""} : {ex}",1);
            continue;
            }

            Logger.ConsoleOutput($"Страница: {i} загружена !!!",2); 

            IEnumerable<IElement> items;

            // Xpath suport
            
            if (config.MainSelectorType == "XPath") {
                var nodes = document.DocumentElement.SelectNodes(config?.MainSelector ?? "");
                items = nodes.OfType<IElement>().ToList();
            }
            else {
                items = document.QuerySelectorAll(config?.MainSelector ?? "");
            }
            
            int itemCount = items?.Count() ?? 0;
            
            if (items == null || itemCount == 0) {
                Logger.ConsoleOutput($"Элементы по MainSelector: {config?.MainSelector ?? ""} не найдены !!!",1);    
                continue;
            } else {
                Logger.ConsoleOutput($"На странице: {i} найдено: {itemCount} элементов !!!",2); 
            }

            // Main parsing 
            foreach(var item in items!) {
                var row = new Dictionary<string,string>();

                foreach(var field0 in config?.Fields ?? new List<FieldConfig>()) {

                    string localName = field0.Name;
                    string localSelector = field0.Selector;
                    string localAttribute = field0?.Attribute ?? "";
                    
                   IElement? element = null;

                    // Flag 5 Xpath  !!
                    if (field0!.Flags.Contains(5)) {
                        if (string.IsNullOrEmpty(localSelector)) {

                            if(!localSelector.StartsWith(".")) localSelector = "." + localSelector;
                            element = item.SelectSingleNode(localSelector) as IElement;
                        }


                    } else {
                    element = string.IsNullOrEmpty(localSelector) ? item : item.QuerySelector(localSelector);
                    }

                    if (element == null) { 
                    Logger.ConsoleOutput($"Элемент: {field0?.Name ?? ""} не найден ",1);    
                    continue; }

                    string value;
                    // Attribute check
                    if(!string.IsNullOrEmpty(localAttribute)) {
                        value = element?.GetAttribute(localAttribute) ?? "";
                        
                        if (string.IsNullOrEmpty(value)){
                            Logger.ConsoleOutput($"Элемент: {localName} Не найден атребут: {localAttribute}",1);
                            row[localName] = "";
                            continue;
                        }
                    
                    } else {
                        value = element.TextContent.Trim();
                    }
                    
                    // Flag system
                    string Endvalue = FlagSystem.GetFlag(value,field0!,url);

                    row[localName] = Endvalue ?? "";
                }

            results.Add(row);
            }

            await Task.Delay(2000);
            }
            return results;
        }
    }
}