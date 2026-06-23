using System.Text.Json;

public static class Logger
{
    private static readonly string LogFilePath = "parser_log.txt";
    private static readonly string LangFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "messagesRu-ru.json");
    private static readonly object FileLock = new object();
    private static Dictionary<string, LogItem> Phrases = new Dictionary<string, LogItem>();

    private class LogItem
    {
        public string Level { get; set; } = "INFO";
        public string Text { get; set; } = string.Empty;
    }

    static Logger()
    {
        LoadLocalization();
    }

    public static void LoadLocalization()
    {
        try
        {
            if (File.Exists(LangFilePath))
            {
                string json = File.ReadAllText(LangFilePath);
                Phrases = JsonSerializer.Deserialize<Dictionary<string, LogItem>>(json) ?? new Dictionary<string, LogItem>();
            }
        }
        catch
        {
            Phrases = new Dictionary<string, LogItem>();
        }
    }

    public static void Log(string key, params object[] args)
    {
        string safeKey = key ?? string.Empty;
        string level;
        string template;

        if (Phrases.TryGetValue(safeKey, out var item) && item != null)
        {
            level = item.Level?.ToUpper() ?? "INFO";
            template = item.Text ?? safeKey;
        }
        else
        {
            level = "INFO";
            template = safeKey;
        }

        string msg = template;
        try 
        { 
            msg = string.Format(template, args ?? Array.Empty<object>()); 
        } 
        catch 
        { 
        }

        ConsoleColor color = GetColorByLevel(level);
        Write(msg, level, color);
    }

    public static void Divider()
    {
        Write("==================================================", "", ConsoleColor.Cyan);
    }

    private static ConsoleColor GetColorByLevel(string level) => (level ?? "").ToUpper() switch
    {
        "INIT"     => ConsoleColor.Cyan,
        "SUCCESS"  => ConsoleColor.Green,
        "WARNING"  => ConsoleColor.Yellow,
        "ERROR"    => ConsoleColor.Red,
        "CRITICAL" => ConsoleColor.DarkRed,
        "NETWORK"  => ConsoleColor.Magenta,
        "PARSER"   => ConsoleColor.Blue,
        _          => ConsoleColor.Gray 
    };

    private static void Write(string msg, string level, ConsoleColor color)
    {
        var now = DateTime.Now;
        string safeMsg = msg ?? string.Empty;
        string safeLevel = level ?? string.Empty;
        bool isRaw = string.IsNullOrEmpty(safeLevel);

        string consolePrefixTime = isRaw ? "" : $"[{now:HH:mm:ss}] ";
        string consolePrefixLevel = isRaw ? "" : $"[{safeLevel.PadRight(7)}] ";
        string fileLine = isRaw ? safeMsg : $"[{now:yyyy-MM-dd HH:mm:ss}] [{safeLevel.PadRight(7)}] {safeMsg}";

        lock (FileLock)
        {
            if (!isRaw)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(consolePrefixTime);

                Console.ForegroundColor = color;
                Console.Write(consolePrefixLevel);
            }

            Console.ForegroundColor = color;
            Console.WriteLine(safeMsg);
            Console.ResetColor();

            try
            {
                using (StreamWriter sw = File.AppendText(LogFilePath))
                {
                    sw.WriteLine(fileLine);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"[{now:HH:mm:ss}] [LOG_ERR] {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}