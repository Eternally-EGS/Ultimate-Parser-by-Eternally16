using System;
using System.IO;

public static class Logger
{
    private static readonly string LogFilePath = "parser_log.txt";
    private static readonly object FileLock = new object();

    public static void Init(string msg)     => Write(msg, "INIT", ConsoleColor.Cyan);
    public static void Info(string msg)     => Write(msg, "INFO", ConsoleColor.Gray);
    public static void Success(string msg)  => Write(msg, "SUCCESS", ConsoleColor.Green);
    public static void Warning(string msg)  => Write(msg, "WARNING", ConsoleColor.Yellow);
    public static void Error(string msg)    => Write(msg, "ERROR", ConsoleColor.Red);
    public static void Critical(string msg) => Write(msg, "CRITICAL", ConsoleColor.DarkRed);
    public static void Network(string msg)  => Write(msg, "NETWORK", ConsoleColor.Magenta);
    public static void Parser(string msg)   => Write(msg, "PARSER", ConsoleColor.Blue);

    public static void Divider()
    {
        Write("==================================================", "INIT", ConsoleColor.Cyan);
    }

    private static void Write(string msg, string level, ConsoleColor color)
    {
        var now = DateTime.Now;
        
        string consolePrefixTime = $"[{now:HH:mm:ss}] ";
        string consolePrefixLevel = $"[{level.PadRight(7)}] ";
        string fileLine = $"[{now:yyyy-MM-dd HH:mm:ss}] [{level.PadRight(7)}] {msg}";

        lock (FileLock)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(consolePrefixTime);

            Console.ForegroundColor = color;
            Console.Write(consolePrefixLevel);

            Console.ResetColor();
            Console.WriteLine(msg);

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