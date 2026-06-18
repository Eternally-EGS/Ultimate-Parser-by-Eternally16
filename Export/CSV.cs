using System.Text;
using UltimateParser.Utils;

namespace UltimateParser.Export 
{
    public static class CSV {

        public static void GetCSV (List<string> head,string path,List<Dictionary<string,string>> result) {
                    
            using(var writer = new StreamWriter(path,false,Encoding.UTF8)){
                writer.WriteLine(string.Join(";",head));

                foreach (var row in result) {
                    var value = head.Select(el => row.ContainsKey(el) ? row[el] : "");
                    writer.WriteLine(string.Join(";",value.Select(v => $"\"{v}\"")));
                }
            }
            Logger.ConsoleOutput("Успешно сохранено в CSV !!!",2);
        }
    }
}