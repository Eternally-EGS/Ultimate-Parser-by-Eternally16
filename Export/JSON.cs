using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using UltimateParser.Utils;

namespace UltimateParser.Export 
{
    public static class JSON {
        
        public static void GetJSON (string path,List<Dictionary<string,string>> result){
            try {
            string jsonpath = Path.ChangeExtension(path,".json");

            var options = new JsonSerializerOptions {
                WriteIndented = true,
            };

            string rawJson = JsonSerializer.Serialize(result,options);
            string cleanJson = Regex.Unescape(rawJson);

            File.WriteAllText(jsonpath,cleanJson,System.Text.Encoding.UTF8);
            
            } catch {
                
            }
        }

    }
}