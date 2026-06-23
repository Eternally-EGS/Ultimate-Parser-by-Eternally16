using System;
using UltimateParser.Config;
using System.Text.Json;

namespace UltimateParser.Utils 
{
    public static class ConfigLoader {

        public static ParserConfig GetConfig (string path) {

            if (!File.Exists(path)) 
            {
                
            }

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ParserConfig>(json)!;
        }
    }
}