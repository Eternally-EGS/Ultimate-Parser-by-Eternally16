using AngleSharp.Dom;
using UltimateParser.Config;
using UltimateParser.Utils;

namespace UltimateParser.Engiens 
{
    public class AngelSharpEngien {

        public async Task<List<Dictionary<string,string>>> GetParse (ParserConfig config) {
            
            var results = new List<Dictionary<string,string>>();

            for (var i = 1;i<= config?.Pages;i++) {

            // Pages select

            string url = config.Url.Replace("{Page}",i.ToString());

            IDocument? document = null;

            try {
            // Create base page
                Logger.ConsoleOutput($"Подключение... к: {url}",2);
                document = await PageLoader.GetPageAsync(url,config.UserAgent);
            } catch (Exception ex)
            {
            Logger.ConsoleOutput($"Ошибка подключния к: {config?.Url ?? ""} : {ex}",1);
            continue;
            }

            Logger.ConsoleOutput($"Страница: {i} загружена !!!",2); 

            var items = document?.QuerySelectorAll(config?.MainSelector ?? "");

            if (items == null || items.Count == 0) {
                Logger.ConsoleOutput($"Элементы по MainSelector: {config?.MainSelector ?? ""} не найдены !!!",1);    
                continue;
            } else {

            Logger.ConsoleOutput($"На странице: {i} найдено: {items.Count} элементов !!!",2); 

            }

            // Main parsing 
            foreach(var item in items!) {
                var row = new Dictionary<string,string>();

                foreach(var field0 in config?.Fields ?? new List<FieldConfig>()) {

                    string localName = field0.Name;
                    string localSelector = field0.Selector;
                    string localAttribute = field0?.Attribute ?? "";
                    
                    var element = string.IsNullOrEmpty(localSelector) ? item : item.QuerySelector(localSelector);
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

                    // Flags system

                    if (!string.IsNullOrEmpty(value) && field0!.Flags != null){

                    // Full Link 1
                    if (field0.Flags.Contains(1)){
                        if (value.StartsWith("/"))
                            value = new Uri(new Uri(config?.Url ?? ""),value).ToString();    
                    }

                    // Trim 2
                    if (field0.Flags.Contains(2)){
                            value = value.Trim();
                    }

                    // Link Cleanup 3
                    if (field0.Flags.Contains(3)){
                            int index = value.IndexOf("?");
                            if (index != -1) { value = value.Substring(0,index); }
                    }

                    // Data and time reload 4
                    if (field0.Flags.Contains(4)){
                        if (DateTime.TryParse(value,out DateTime date)) 
                            value = date.ToString("yyyy-MM-dd");
                    }
                }
                    row[localName] = value ?? "";
                }

            results.Add(row);

            }

            await Task.Delay(2000);
            }
            return results;
        }
    }
}