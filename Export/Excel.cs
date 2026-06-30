using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace UltimateParser.Export 
{
    public static class Excel {

        public static void GetExcel (string path, string pathXL) {

            string safePath = path ?? "";
            string safePathXL = pathXL ?? "Out.xlsx";

            if (!File.Exists(safePath)) { 
                 
                Logger.Log("File_Write_Error", "Исходный CSV файл не найден для конвертации.");
                return; 
            }

            var lines = File.ReadAllLines(safePath, Encoding.UTF8);
            if (lines.Length <= 0) return;

            var table = new DataTable();

            var header = (lines[0] ?? "").Split(';');
            foreach (var head in header) {
                if (head != null) {
                    table.Columns.Add(head.Trim('"'));
                }
            }

            var csvParser = new Regex(";(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

            for (var i = 1; i < lines.Length; i++) {
                string currentLine = lines[i];
                if (string.IsNullOrWhiteSpace(currentLine)) continue;

                var values = csvParser.Split(currentLine);

                for (int j = 0; j < values.Length; j++) {
                    string v = (values[j] ?? "").Trim();
                    if (v.StartsWith("\"") && v.EndsWith("\"") && v.Length >= 2) {
                        v = v.Substring(1, v.Length - 2);
                    }
                    values[j] = v.Replace("\"\"", "\"");
                }

                if (values.Length > table.Columns.Count) {
                    Array.Resize(ref values, table.Columns.Count);
                } 
                else if (values.Length < table.Columns.Count) {
                    var temporary = new string[table.Columns.Count];
                    Array.Copy(values, temporary, values.Length);
                    for (int k = values.Length; k < temporary.Length; k++) temporary[k] = "";
                    values = temporary;
                }

                table.Rows.Add(values);
            }

            try {
                using (var workbook = new XLWorkbook()) {
                    var works = workbook.Worksheets.Add("Data");
                    works.Cell(1, 1).InsertTable(table);
                    works.Columns().AdjustToContents();
                    workbook.SaveAs(safePathXL);
                }
                Logger.Log("Data_Saved", table.Rows.Count, safePathXL);
            }
            catch (Exception ex) {
                Logger.Log("File_Write_Error", ex.Message);
            }
        }
    }
}