
using System.Data;
using System.Text;
using ClosedXML.Excel;
using UltimateParser.Utils;

namespace UltimateParser.Export 
{
    public static class Excel {

        public static void GetExcel (string path,string pathXL) {

            // Protaction
            if (!File.Exists(path)) { Logger.ConsoleOutput($"CSV файл не найден !",0); return; }

            var lines = File.ReadAllLines(path,Encoding.UTF8);
            if (lines.Length <= 0) { return; }

            var table = new DataTable();
            var header = lines[0].Split(';');

            foreach (var head in header) 
                table.Columns.Add(head);

            for (var i = 1; i < lines.Length;i++){
                var value = lines[i].Split(';');
                table.Rows.Add(value);
            }

            // Saving 
            using (var workbook = new XLWorkbook()) {
                var works = workbook.Worksheets.Add("Data");
                works.Cell(1,1).InsertTable(table);
                workbook.SaveAs(pathXL);
            }

            Logger.ConsoleOutput("Execel успешно создан !",2);

        }
    }
}