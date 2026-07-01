using UltimateParser.Config;
using AngleSharp.Dom;
using UltimateParser.Parsers;
using UltimateParser.Utils;
using AngleSharp.XPath;
using UltimateParser.Export;

namespace UltimateParser.Engines
{
    public class PlaywrightEngine : IParserEngine
    {
        public event Action<List<Dictionary<string, string>>>? OnCheckpoint = null;

        public async Task<List<Dictionary<string, string>>> GetParse(ParserConfig config)
        {
            var results = new List<Dictionary<string, string>>();
            if (config == null) return results;

            for (var i = 1; i <= config.Pages; i++)
            {
                if (UltimateParser_Main.isExit) break;

                string url = (config.Url ?? "").Replace("{Page}", i.ToString());
                Logger.Log("Nav_Start", url);

                IDocument? document = await LoadPageAsync(url, config);
                if (document == null) continue;

                var items = GetItems(document, config);
                if (!items.Any())
                {
                    Logger.Log("Crit_Layout_Changed");
                    continue;
                }

                Logger.Log("Container_Found", items.Count());

                foreach (var item in items)
                {
                    var row = ParseSingleItem(item, config, url);
                    if (TableProcessing.TableCP(row, config)) { results.Add(row); }
                }

                Logger.Log("Page_Done", i, results.Count);
                OnCheckpoint?.Invoke(results);
                await Task.Delay(Random.Shared.Next(config.MinDelay, config.MaxDelay));
            }
            return results;
        }

        private async Task<IDocument?> LoadPageAsync(string url, ParserConfig config)
        {
            try
            {
                var doc = await PageLoader.GetPagePlaywrightAsync(url, config);
                if (doc != null) Logger.Log("Html_Received", doc.Source?.Text?.Length ?? 0);
                return doc;
            }
            catch { Logger.Log("Err_Page_Skip", url, 1); return null; }
        }

        private IEnumerable<IElement> GetItems(IDocument document, ParserConfig config)
        {
            if (config.MainSelectorType == "XPath")
            {
                return document.DocumentElement?.SelectNodes(config.MainSelector ?? "")?.OfType<IElement>() ?? Enumerable.Empty<IElement>();
            }
            return document.QuerySelectorAll(config.MainSelector ?? "") ?? Enumerable.Empty<IElement>();
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

                if (element == null) { Logger.Log("Warn_No_Field", field.Name ?? "", ""); continue; }

                string value = !string.IsNullOrEmpty(field.Attribute) 
                    ? element.GetAttribute(field.Attribute) ?? "" 
                    : element.TextContent.Trim();

                row[field.Name ?? ""] = FlagSystem.GetFlag(value, field, url) ?? "";
            }
            return row;
        }
    }
}