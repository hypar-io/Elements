using System;

namespace Hypar
{
    public enum LogLevel
    {
        Info, Warning, Error, Success
    }

    public static class Logger
    {
        public static void LogInfo(string message)
        {
            Log(message, LogLevel.Info);
        }

        public static void LogWarning(string message)
        {
            Log(message, LogLevel.Warning);
        }

        public static void LogError(string message)
        {
            Log(message, LogLevel.Error);
        }

        public static void LogSuccess(string message)
        {
            Log(message, LogLevel.Success);
        }

        private static void Log(string message, LogLevel level)
        {
            var prefix = "";

            switch(level)
            {
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                default:
                    Console.ResetColor();
                    break;
            }

            Console.WriteLine(prefix + message);
            Console.ResetColor();
        }
    }
}