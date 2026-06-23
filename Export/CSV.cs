using System.Text;

namespace UltimateParser.Export 
{
    public static class CSV {

        public static void GetCSV (List<string> head,string path,List<Dictionary<string,string>> result) {
            
            try {
                var safeHead = head ?? new List<string>();
                var safeResult = result ?? new List<Dictionary<string,string>>();
                var safePath = path ?? "output.csv";

                using(var writer = new StreamWriter(safePath,false,Encoding.UTF8)){
                    writer.WriteLine(string.Join(";",safeHead));

                    foreach (var row in safeResult) {
                        if (row == null) continue;
                        var value = safeHead.Select(el => el != null && row.ContainsKey(el) ? row[el] : "");
                        writer.WriteLine(string.Join(";",value.Select(v => $"\"{v}\"")));
                    }
                }

                Logger.Log("Data_Saved", safeResult.Count, safePath);
            }
            catch (Exception ex) {
                Logger.Log("File_Write_Error", ex.Message);
            }
            
        }
    }
}