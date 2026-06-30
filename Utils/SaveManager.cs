using UltimateParser.Config;
using UltimateParser.Export;

namespace UltimateParser.Utils 
{
    
    public static class SaveManager {
        
        private static string folderPath = string.Empty;

        public static void GetSaving (List<string> head, List<Dictionary<string, string>> result, ParserConfig config, bool EndProgram) {
            
            if (config == null) {
                Logger.Log("Config_Error"); 
                return;
            }

            if (string.IsNullOrEmpty(config.ExportFolderPath)) {
            
            // Path
            folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");

            // Auto create
            if (!Directory.Exists(folderPath)) 
            {
                Directory.CreateDirectory(folderPath);  }
            
            } else {

                string userpath = config.ExportFolderPath;

                // Auto create
                if (!Directory.Exists(userpath)) 
                {
                Directory.CreateDirectory(userpath);  }

                folderPath = config.ExportFolderPath;
            }

            string Parameter = DateTime.Now.ToString("dd_MM HH-mm");
            string safePath = Path.Combine(folderPath ?? ".", $"[{Parameter}] Out.csv");
            string safePath_t = Path.Combine(folderPath ?? ".", $"TEMP_[{Parameter}] Out.csv");
            string safePath2 = Path.Combine(folderPath ?? ".", $"[{Parameter}] Out.xlsx");
            string safePath3 = Path.Combine(folderPath ?? ".", $"[{Parameter}] Out.json");
            var safeHead = head ?? new List<string>();
            var safeResult = result ?? new List<Dictionary<string, string>>();

             switch (config.ExportTo) {
                case 0: 
                    CSV.GetCSV(safeHead, safePath, safeResult); 
                    break;

                case 1: 
                    CSV.GetCSV(safeHead, safePath_t, safeResult); 
                    
                    if (EndProgram) {  
                        try {
                            Excel.GetExcel(safePath_t, safePath2); 
                            
                            if (File.Exists(safePath_t)) {
                                File.Delete(safePath_t);
                            }
                        }
                        catch (Exception ex) {
                            Logger.Log($"Excel_Error: {ex.Message}. Saving fallback CSV...");
                            
                            if (File.Exists(safePath_t)) {
                                File.Move(safePath_t, safePath);
                            }
                        }
                    }
                    break;
                case 2: 
                    JSON.GetJSON(safePath3, safeResult); 
                    break;

                default:
                    Logger.Log("Export_Type_Error");
                    break;
            }



        }

    }
}