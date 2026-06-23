using UltimateParser.Config;
using System.Text.Json;

namespace UltimateParser.Utils 
{
    public static class ConfigLoader {

        public static ParserConfig GetConfig (string path) {

            string safePath = path ?? "config.json";

            if (!File.Exists(safePath)) 
            {
                Logger.Log("Config_Error", safePath);
                return new ParserConfig();
            }

            try {
                string json = File.ReadAllText(safePath);
                var config = JsonSerializer.Deserialize<ParserConfig>(json);
                
                Logger.Log("Config_Loaded", safePath);
                return config ?? new ParserConfig();
            }
            catch (Exception) {
                Logger.Log("Config_Error", safePath);
                return new ParserConfig();
            }
        }
    }
}