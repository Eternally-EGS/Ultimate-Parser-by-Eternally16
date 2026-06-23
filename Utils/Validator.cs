// Validation and security

using UltimateParser.Config;

namespace UltimateParser.Utils 
{
    public static class Validator {

        public static bool GetValidator(ParserConfig config) {

            // Config
            if (config == null) {
                    
                return false;
            }

            // URL
            if (string.IsNullOrEmpty(config?.Url ?? "")){
                    
                return false;
            }

            // Main selector
            if (string.IsNullOrEmpty(config?.MainSelector ?? "")){
                    
                return false;
            }

            // Fields
            if (config?.Fields == null || config?.Fields.Count == 0 ){
                    
                return false;
            }

            // Pages
            if (config?.Pages < 1) {
                    
                return false;
            }

            // ExportTo
            if (config?.ExportTo == null) {
                    
                return false;
            }

        
        return true;
        }
    }
}