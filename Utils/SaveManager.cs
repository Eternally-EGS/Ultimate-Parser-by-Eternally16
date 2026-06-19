using UltimateParser.Config;
using UltimateParser.Export;

namespace UltimateParser.Utils 
{
    public static class SaveManager {

        public static void GetSave (List<string> head,string path,List<Dictionary<string,string>> result,ParserConfig config,bool EndProgram,string Path2) {
            switch (config.ExportTo) {
                case 0: CSV.GetCSV(head,path,result); break;
                case 1: CSV.GetCSV(head,path,result); if (EndProgram) { Excel.GetExcel(path,Path2); } break;
            }
        }
    }
}