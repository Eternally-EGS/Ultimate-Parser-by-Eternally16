
using DocumentFormat.OpenXml.Wordprocessing;

namespace UltimateParser.Config {

    // Css Selector and attribute
    public class FieldConfig
    {
        // Collumn Name 
        public string Name {get; set;} = string.Empty;
        // Selector ID
        public string Selector {get; set;} = string.Empty;
        // Selesor Attribute
        public string? Attribute {get; set;}
        // Flags (1 Full link/2 Trim/3 Cleanup/4 Date Format/5 XPath)
        public int?[] Flags {get;set;} = [];
    }

    // Main setting
    public class ParserConfig {
    // Proxy Setting

        // Adress List
        public List<string> Proxies {get; set;} = new List<string>();
        // Main flag
        public bool UseProxy {get; set;} = false;
        // Timeout (secunde)
        public int TimeOut {get;set;}

    // Playwridht Setting

        // View browser
        public bool Headless {get; set;}
        // Set Loacation
        public string Locale {get; set;} = string.Empty;
        // Set TimeZone
        public string TimezoneId {get; set;} = string.Empty;
        // Waiting Selector
        public string WaitForSelector {get; set;} = string.Empty;
        // Enable JS 
        public bool JS {get; set;}
        // Randomization tick
        public int MaxDelay {get; set;}
        public int MinDelay {get; set;}
        // Scrol imitation
        public bool ScrollImitation {get; set;}

    // Main Setting

        // Export format (0 CSV/1 Excel/2 JSON)
        public int ExportTo {get; set;}
        // Engine type (0 AgleSharp/1 Playwright)
        public int EngineType {get; set;}
        // Parsing URL
        public string Url {get; set;} = string.Empty;
        // Selector List 
        public List<FieldConfig> Fields {get; set;} = new List<FieldConfig>();
        // Base Selector
        public string MainSelector {get; set;} = string.Empty;
        // Base Selector Type (CSS/XPath)
        public string MainSelectorType {get; set;} = string.Empty;
        // Page count
        public int Pages {get; set;} = 0;
        // Browser Name
        public string UserAgent {get; set;} = string.Empty;
        // Export folder base (project/Exports)
        public string ExportFolderPath {get; set;} = string.Empty;
        // Table cleanup
        public bool PostProcessing { get; set; } = true;
        // Browser path
        public string ExecutablePath { get; set; } = "";
        public float NetworkTimeout { get; set; } = 30000f; 
        public float SelectorTimeout { get; set; } = 15000f; 
        public bool MoveMouseImitation {get; set;}
        public int ImitationStepsCount {get; set;} = 10;
        public string PriorityField {get; set;} = string.Empty;
    }

}