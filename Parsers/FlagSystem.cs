using UltimateParser.Config;

namespace UltimateParser.Parsers 
{
    public class FlagSystem {

        public static string GetFlag (string value,FieldConfig field,string Baseurl) {
        
            // Flags system
            if (!string.IsNullOrEmpty(value) && field!.Flags != null){

                // Full Link 1
                if (field.Flags.Contains(1)){
                    if (value.StartsWith("/"))
                        value = new Uri(new Uri(Baseurl),value).ToString();    
                }

                // Trim 2
                if (field.Flags.Contains(2)){
                        value = value.Trim();
                }

                // Link Cleanup 3
                if (field.Flags.Contains(3)){
                        int index = value.IndexOf("?");
                        if (index != -1) { value = value.Substring(0,index); }
                }

                // Data and time reload 4
                if (field.Flags.Contains(4)){
                    if (DateTime.TryParse(value,out DateTime date)) 
                        value = date.ToString("yyyy-MM-dd");
                }
            }

            return value;
        }
    }
}