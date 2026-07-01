using System.Collections.Generic;
using System.Linq;

namespace UltimateParser.HotFilters 
{
    public static class HotFilter 
    {
        public static List<Dictionary<string, string>> GetFilter(List<Dictionary<string, string>> result) 
        {

            result.RemoveAll(row => 
            {

                //if (CheckCondition(row, "Price", 1600, "<")) return true;

                // if (row.ContainsKey("Status") && row["Status"] == "Нет в наличии") return true;

                return false;
            });

            

            return result;
        }

        private static bool CheckCondition(Dictionary<string, string> row, string key, decimal threshold, string op)
        {
            if (!row.ContainsKey(key) || row[key] == "Нет данных") return false;

            string clean = new string(row[key].Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray()).Replace(',', '.');
            if (decimal.TryParse(clean, out decimal val))
            {
                if (op == "<") return val < threshold;
                if (op == ">") return val > threshold;
            }
            return false;
        }
    }
}