using UltimateParser.Config;

namespace UltimateParser.Utils 
{
    public static class Validator {

        public static bool GetValidator(ParserConfig config) {

            // Config
            if (config == null) {
                Logger.Log("Config_Error");
                return false;
            }

            // URL
            if (string.IsNullOrWhiteSpace(config.Url)){
                Logger.Log("Url_Empty_Error");
                return false;
            }

            // Main selector
            if (string.IsNullOrWhiteSpace(config.MainSelector)){
                Logger.Log("Selector_Empty_Error");
                return false;
            }

            // Main selector type validation
            if (!string.IsNullOrEmpty(config.MainSelectorType) && 
                config.MainSelectorType != "CSS" && config.MainSelectorType != "XPath") {
                Logger.Log("Selector_Type_Error");
                return false;
            }

            // Fields
            if (config.Fields == null || config.Fields.Count == 0 ){
                Logger.Log("Fields_Empty_Error");
                return false;
            }

            foreach (var field in config.Fields) {
                if (field == null || string.IsNullOrWhiteSpace(field.Name)) {
                    Logger.Log("Field_Name_Error");
                    return false;
                }
            }

            // Pages
            if (config.Pages < 1) {
                Logger.Log("Pages_Count_Error");
                return false;
            }

            // ExportTo
            if (config.ExportTo < 0 || config.ExportTo > 2) {
                Logger.Log("Export_Type_Error");
                return false;
            }

            // Timeouts
            if (config.TimeOut <= 0) {
                Logger.Log("Timeout_Error");
                return false;
            }

            // Delays
            if (config.MinDelay < 0 &&  config.MaxDelay < 0 && config.MinDelay > config.MaxDelay) {
                Logger.Log("Delay_Range_Error");
                return false;
            }

            // Proxy
            if (config.UseProxy) {
                if (config.Proxies == null || !config.Proxies.Any(p => !string.IsNullOrWhiteSpace(p))) {
                    Logger.Log("Proxy_Empty_Error");
                    return false;
                }
            }
        
            return true;
        }
    }
}