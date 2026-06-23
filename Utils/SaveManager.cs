using UltimateParser.Config;
using UltimateParser.Export;

namespace UltimateParser.Utils 
{
    public static class SaveManager {

        public static void GetSave (List<string> head, string path, List<Dictionary<string, string>> result, ParserConfig config, bool EndProgram, string Path2) {
            
            if (config == null) {
                Logger.Log("Config_Error"); 
                return;
            }

            string safePath = path ?? "Out.csv";
            string safePath2 = Path2 ?? "Out.xlsx";
            var safeHead = head ?? new List<string>();
            var safeResult = result ?? new List<Dictionary<string, string>>();

            switch (config.ExportTo) {
                case 0: 
                    CSV.GetCSV(safeHead, safePath, safeResult); 
                    break;

                case 1: 
                    CSV.GetCSV(safeHead, safePath, safeResult); 
                    if (EndProgram) { 
                        Excel.GetExcel(safePath, safePath2); 
                        try {
                            if (File.Exists(safePath)) { 
                                File.Delete(safePath);
                            }
                        } 
                        catch {  
                            Logger.Log("File_Delete_Error");
                        } 
                    }
                    break;

                case 2: 
                    JSON.GetJSON(safePath, safeResult); 
                    break;

                default:
                    Logger.Log("Export_Type_Error");
                    break;
            }
        }
    }
}