using UltimateParser.Config;
using UltimateParser.Export;

namespace UltimateParser.Utils 
{
    public static class SaveManager {

        public static void GetSave (List<string> head,string path,List<Dictionary<string,string>> result,ParserConfig config) {
            switch (config.ExportTo) {
                case 0: CSV.GetCSV(head,path,result); break;
            }
        }
    }
}