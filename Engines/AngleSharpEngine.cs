using AngleSharp.Dom;
using UltimateParser.Config;
using UltimateParser.Parsers;
using UltimateParser.Utils;
using HtmlAgilityPack;
using ClosedXML.Excel;
using AngleSharp.XPath;

namespace UltimateParser.Engines 
{
    public class AngelSharpEngine : IParserEngine {
        // Autosave 
        public event Action<List<Dictionary<string,string>>>? OnCheckpoint = null;

        // Engine
        public async Task<List<Dictionary<string,string>>> GetParse (ParserConfig config) {
            
            // Main result
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
                
                document = await PageLoader.GetPageAsync(url,config.UserAgent,config);
            } 
            catch (Exception ex) {
            
            continue;
            }

             

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
                    
                continue;
            } else {
                 
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
                        if (!string.IsNullOrEmpty(localSelector)) {

                            if(!localSelector.StartsWith(".")) localSelector = "." + localSelector;
                            element = item.SelectSingleNode(localSelector) as IElement;
                        }


                    } else {
                    element = string.IsNullOrEmpty(localSelector) ? item : item.QuerySelector(localSelector);
                    }

                    if (element == null) { 
                        
                    continue; }

                    string value;
                    
                    // Attribute check
                    if(!string.IsNullOrEmpty(localAttribute)) {
                        value = element?.GetAttribute(localAttribute) ?? "";
                        
                        if (string.IsNullOrEmpty(value)){
                            
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
            if (row.Count > 0 && row.Any(kvp => !string.IsNullOrWhiteSpace(kvp.Value))) {
                results.Add(row);
            }

            }

            await Task.Delay(2000);
            }
            return results;
        }
    }
}