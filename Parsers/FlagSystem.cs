using UltimateParser.Config;

namespace UltimateParser.Parsers 
{
    public class FlagSystem {
        public static string GetFlag (string value,FieldConfig field,string Baseurl) {
        
            string safeValue = value ?? "";
            string safeUrl = Baseurl ?? "";

            // Flags system
            if (!string.IsNullOrEmpty(safeValue) && field != null && field.Flags != null){

                // Full Link 1
                if (field.Flags.Contains(1)){
                    if (safeValue.StartsWith("/") && !string.IsNullOrEmpty(safeUrl))
                    {
                        try {
                            safeValue = new Uri(new Uri(safeUrl), safeValue).ToString();    
                        }
                        catch {
                            Logger.Log("Warn_Data_Corrupted", field.Name ?? "", "URL", safeValue);
                        }
                    }
                }

                // Trim 2
                if (field.Flags.Contains(2)){
                        safeValue = safeValue.Trim();
                }

                // Link Cleanup 3
                if (field.Flags.Contains(3)){
                        int index = safeValue.IndexOf("?");
                        if (index != -1) { safeValue = safeValue.Substring(0,index); }
                }

                // Data and time reload 4
                if (field.Flags.Contains(4)){
                    if (DateTime.TryParse(safeValue,out DateTime date)) 
                        safeValue = date.ToString("yyyy-MM-dd");
                    else
                        Logger.Log("Warn_Data_Corrupted", field.Name ?? "", "DateTime", safeValue);
                }
            }
            return safeValue;
        }
    }
}