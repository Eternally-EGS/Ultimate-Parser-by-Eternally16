using AngleSharp.Dom;
using UltimateParser.Config;
using UltimateParser.Parsers;
using UltimateParser.Utils;
using AngleSharp.XPath;
using UltimateParser.Export;

namespace UltimateParser.Engines 
{
    public class AngelSharpEngine : IParserEngine {
        // Autosave 
        public event Action<List<Dictionary<string,string>>>? OnCheckpoint = null;

        // Engine
        public async Task<List<Dictionary<string,string>>> GetParse (ParserConfig config) {
            
            // Main result
            var results = new List<Dictionary<string,string>>();

            if (config == null) return results;

            int totalPages = config.Pages;

            for (var i = 1; i <= totalPages; i++) {

                // save and exit
                if (UltimateParser_Main.isExit) { 
                    Logger.Log("App_Cancel");
                    break; 
                }
                
                // Buffer
                OnCheckpoint?.Invoke(results);

                // Pages select

                string url = (config.Url ?? "").Replace("{Page}", i.ToString());

                Logger.Log("Nav_Start", url);

                IDocument? document = null;
                
                // Create base page
                try {
                    document = await PageLoader.GetPageAsync(url, config.UserAgent ?? "", config);
                    if (document != null)
                    {
                        Logger.Log("Html_Received", document.Source?.Text?.Length ?? 0);
                    }
                } 
                catch (Exception) {
                    Logger.Log("Err_Page_Skip", url, 1);
                    continue;
                }

                IEnumerable<IElement> items;

                // Xpath suport
                
                if (config.MainSelectorType == "XPath") {
                    var nodes = document?.DocumentElement?.SelectNodes(config.MainSelector ?? "");
                    items = nodes?.OfType<IElement>().ToList() ?? new List<IElement>();
                }
                else {
                    items = document?.QuerySelectorAll(config.MainSelector ?? "") ?? Enumerable.Empty<IElement>();
                }
                
                int itemCount = items.Count();
                
                if (itemCount == 0) {
                    Logger.Log("Crit_Layout_Changed");
                    continue;
                } else {
                    Logger.Log("Container_Found", itemCount);
                }

                int itemIndex = 0;

                // Main parsing 
                foreach(var item in items) {
                    itemIndex++;
                    Logger.Log("Item_Parse_Start", itemIndex, itemIndex);

                    var row = new Dictionary<string,string>();

                    foreach(var field0 in config.Fields ?? new List<FieldConfig>()) {
                        if (field0 == null) continue;

                        string localName = field0.Name ?? "";
                        string localSelector = field0.Selector ?? "";
                        string localAttribute = field0.Attribute ?? "";
                        
                        IElement? element = null;

                        // Flag 5 Xpath  !!
                        if (field0.Flags != null && field0.Flags.Contains(5)) {
                            if (!string.IsNullOrEmpty(localSelector)) {

                                if(!localSelector.StartsWith(".")) localSelector = "." + localSelector;
                                element = item.SelectSingleNode(localSelector) as IElement;
                            }

                        } else {
                            element = string.IsNullOrEmpty(localSelector) ? item : item.QuerySelector(localSelector);
                        }

                        if (element == null) { 
                            Logger.Log("Warn_No_Field", localName, localName);
                            continue; 
                        }

                        string value;
                        
                        // Attribute check
                        if(!string.IsNullOrEmpty(localAttribute)) {
                            value = element.GetAttribute(localAttribute) ?? "";
                            
                            if (string.IsNullOrEmpty(value)){
                                Logger.Log("Warn_No_Field", localName, localAttribute);
                                row[localName] = "";
                                continue;
                            }
                        
                        } else {
                            value = element.TextContent.Trim();
                        }
                        
                        // Flag system
                        string endValue = FlagSystem.GetFlag(value, field0, url) ?? "";

                        row[localName] = endValue;
                    }
                       if(TableProcessing.TableCP(row,config)) { results.Add(row); };
                }

                Logger.Log("Page_Done", i, results.Count);

                // Randomization tick
                    int Min = config.MinDelay;
                    int Max = config.MaxDelay;
                await Task.Delay(Random.Shared.Next(Min, Max));
            }
            return results;
        }
    }
}