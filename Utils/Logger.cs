using System;

namespace UltimateParser.Utils 
{
    public static class Logger {

        private static List<string> LogFile = new List<string>();

        // Error output
        public static void ConsoleOutput(string message,int color) {
            var colors = ConsoleColor.White;
            switch (color) {
            case 0: colors = ConsoleColor.Red; break;
                case 1: colors = ConsoleColor.Yellow; break;
                    case 2: colors = ConsoleColor.Cyan; break;
            }
            Console.ForegroundColor = colors;
            Console.WriteLine(message +"  "+ DateTime.Now);
            Console.ResetColor();
            LogFile.Add(message +"  "+ DateTime.Now);
            // Log file
            File.WriteAllLines("ProgramLog.txt",LogFile);
        }
    }
}