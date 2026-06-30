using UltimateParser.Config;

namespace UltimateParser.Export 
{
    public static class TableProcessing {
        private static readonly HashSet<string> _seenRows = new HashSet<string>();

        public static bool TableCP(Dictionary<string,string> _row, ParserConfig config) {

            if (config.PostProcessing) {

            // base
            if (_row == null) return false;

            // Pass
            bool hasAnyData = _row.Values.Any(v => !string.IsNullOrWhiteSpace(v));
            if (!hasAnyData) return false;

            // Dublicate
            string rowFingerprint = string.Join("||", _row.OrderBy(k => k.Key).Select(v => v.Value ?? ""));
            bool isUnique = _seenRows.Add(rowFingerprint);
            if (!isUnique) {   return false;   }

            }

            return true;
        }

    }
}