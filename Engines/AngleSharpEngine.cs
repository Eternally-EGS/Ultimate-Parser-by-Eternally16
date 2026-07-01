using AngleSharp.Dom;
using UltimateParser.Config;
using UltimateParser.Parsers;
using UltimateParser.Utils;
using AngleSharp.XPath;
using UltimateParser.Export;
using AngleSharp;
using UltimateParser.HotFilters;

namespace UltimateParser.Engines 
{
    public class AngelSharpEngine : IParserEngine {
        public event Action<List<Dictionary<string,string>>>? OnCheckpoint = null;

        public async Task<List<Dictionary<string,string>>> GetParse (ParserConfig config) {
            var results = new List<Dictionary<string,string>>();
            if (config == null) return results;

            for (var i = 1; i <= config.Pages; i++) {
                if (UltimateParser_Main.isExit) { 
                    Logger.Log("App_Cancel");
                    break; 
                }
                
                string url = (config.Url ?? "").Replace("{Page}", i.ToString());
                Logger.Log("Nav_Start", url);

                IDocument? document = null;
                try {
                    document = await PageLoader.GetPageAsync(url, config);
                    if (document != null) Logger.Log("Html_Received", document.Source?.Text?.Length ?? 0);
                } 
                catch {
                    Logger.Log("Err_Page_Skip", url, 1);
                    continue;
                }

                IEnumerable<IElement> items = config.MainSelectorType == "XPath" 
                    ? document?.DocumentElement?.SelectNodes(config.MainSelector ?? "")?.OfType<IElement>() ?? new List<IElement>()
                    : document?.QuerySelectorAll(config.MainSelector ?? "") ?? Enumerable.Empty<IElement>();
                
                if (!items.Any()) {
                    Logger.Log("Crit_Layout_Changed");
                    continue;
                }
                Logger.Log("Container_Found", items.Count());

                int itemIndex = 0;
                foreach(var item in items) {
                    itemIndex++;
                    var row = ParseSingleItem(item, config, url);
                    
                    if (TableProcessing.TableCP(row, config)) { 
                        if (!results.Any(r => r.Values.SequenceEqual(row.Values))) {
                            results.Add(row);
                            Logger.Log("Success_Item_Added", itemIndex);
                        }
                    } else {
                        Logger.Log("Warn_Item_Rejected", itemIndex, "TableCP_Validation_Failed");
                    }
                }
                results = HotFilter.GetFilter(results);

                Logger.Log("Page_Done", i, results.Count);
                OnCheckpoint?.Invoke(results);
                await Task.Delay(Random.Shared.Next(config.MinDelay, config.MaxDelay));
            }
            return results;
        }

        private Dictionary<string, string> ParseSingleItem(IElement item, ParserConfig config, string url)
        {
            var row = new Dictionary<string, string>();
            foreach (var field in config.Fields ?? new List<FieldConfig>())
            {
                if (field == null) continue;
                IElement? element = (field.Flags != null && field.Flags.Contains(5)) 
                    ? item.SelectSingleNode(field.Selector?.StartsWith(".") == true ? field.Selector : "." + field.Selector) as IElement
                    : (string.IsNullOrEmpty(field.Selector) ? item : item.QuerySelector(field.Selector));

                if (element == null) { Logger.Log("Warn_No_Field", field.Name ?? "", "Not_Found"); continue; }

                string value = !string.IsNullOrEmpty(field.Attribute) ? element.GetAttribute(field.Attribute) ?? "" : element.TextContent.Trim();
                row[field.Name ?? ""] = FlagSystem.GetFlag(value, field, url) ?? "";
            }
            Logger.Log("Debug_Parsed_Row", string.Join(" | ", row.Select(x => $"{x.Key}:{x.Value}")));
            return row;
        }
    }
}