using UltimateParser.Config;
using UltimateParser.Export;
using System;
using System.Collections.Generic;
using System.IO;

namespace UltimateParser.Utils 
{
    public static class SaveManager {
        
        private static string folderPath = string.Empty;

        public static void GetSaving (List<string> head, List<Dictionary<string, string>> result, ParserConfig config, bool EndProgram) {
            
            if (config == null) {
                Logger.Log("Config_Error"); 
                return;
            }

            if (string.IsNullOrEmpty(folderPath)) 
            {
                string parameter = DateTime.Now.ToString("dd_MM_yyyy_HH-mm-ss");

                string baseDirectory = string.IsNullOrEmpty(config.ExportFolderPath)
                    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports")
                    : config.ExportFolderPath;

                folderPath = Path.Combine(baseDirectory, parameter);

                if (!Directory.Exists(folderPath)) 
                {
                    Directory.CreateDirectory(folderPath); 
                }

                try 
                {
                    string configPath = Path.Combine(folderPath, "config_snapshot.json");
                    var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                    string configJson = System.Text.Json.JsonSerializer.Serialize(config, jsonOptions);
                    File.WriteAllText(configPath, configJson, System.Text.Encoding.UTF8);
                }
                catch (Exception ex) 
                {
                    Logger.Log($"Config_Snapshot_Error: {ex.Message}");
                }
            }

            string safePath = Path.Combine(folderPath, "Out.csv");
            string safePath_t = Path.Combine(folderPath, "TEMP_Out.csv");
            string safePath2 = Path.Combine(folderPath, "Out.xlsx");
            string safePath3 = Path.Combine(folderPath, "Out.json");
            
            var safeHead = head ?? new List<string>();
            var safeResult = result ?? new List<Dictionary<string, string>>();

            switch (config.ExportTo) {
                case 0: 
                    CSV.GetCSV(safeHead, safePath, safeResult); 
                    break;

                case 1: 
                    // Если это промежуточный экспорт (не конец программы), мы просто дописываем/обновляем TEMP_Out.csv
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
                                if (File.Exists(safePath)) File.Delete(safePath);
                                File.Move(safePath_t, safePath);
                            }
                        }
                        
                        folderPath = string.Empty;
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