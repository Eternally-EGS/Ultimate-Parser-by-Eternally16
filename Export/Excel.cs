using System;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using UltimateParser.Utils;

namespace UltimateParser.Export 
{
    public static class Excel {

        public static void GetExcel (string path, string pathXL) {

            if (!File.Exists(path)) { 
                 
                return; 
            }

            var lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length <= 0) return;

            var table = new DataTable();

            var header = lines[0].Split(';');
            foreach (var head in header) {
                table.Columns.Add(head.Trim('"'));
            }

            var csvParser = new Regex(";(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

            for (var i = 1; i < lines.Length; i++) {
                string currentLine = lines[i];
                if (string.IsNullOrWhiteSpace(currentLine)) continue;

                var values = csvParser.Split(currentLine);

                for (int j = 0; j < values.Length; j++) {
                    string v = values[j].Trim();
                    if (v.StartsWith("\"") && v.EndsWith("\"")) {
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
                    workbook.SaveAs(pathXL);
                }
                
            }
            catch (Exception ex) {
                
            }
        }
    }
}