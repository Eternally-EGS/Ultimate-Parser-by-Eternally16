
using DocumentFormat.OpenXml.Wordprocessing;

namespace UltimateParser.Config {

    // Css Selector and attribute
    public class FieldConfig
    {
        public string Name {get; set;} = string.Empty;
        public string Selector {get; set;} = string.Empty;
        public string? Attribute {get; set;}
        public int?[] Flags {get;set;} = [];
    }

    // Main setting
    public class ParserConfig {
        public int ExportTo {get; set;}
        public string Url {get; set;} = string.Empty;
        public List<FieldConfig> Fields {get; set;} = new List<FieldConfig>();
        public string MainSelector {get; set;} = string.Empty;
        public string MainSelectorType {get; set;} = string.Empty;
        public int Pages {get; set;} = 0;
        public List<string> Proxies {get; set;} = new List<string>();
        public bool UseProxy {get; set;} = false;
        public string UserAgent {get; set;} = string.Empty;
    }

}