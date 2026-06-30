using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace UltimateParser.Export 
{
    public static class JSON {
        
        public static void GetJSON (string path,List<Dictionary<string,string>> result){
            try {
                string safePath = path ?? "Out.json";
                var safeResult = result ?? new List<Dictionary<string, string>>();
                string jsonpath = Path.ChangeExtension(safePath, ".json") ?? "output.json";

                var options = new JsonSerializerOptions {
                    WriteIndented = true,
                };

                string rawJson = JsonSerializer.Serialize(safeResult, options);
                string cleanJson = Regex.Unescape(rawJson);

                File.WriteAllText(jsonpath, cleanJson, Encoding.UTF8);
                
                Logger.Log("Data_Saved", safeResult.Count, jsonpath);
            } 
            catch (Exception ex) {
                Logger.Log("File_Write_Error", ex.Message);
            }
        }

    }
}