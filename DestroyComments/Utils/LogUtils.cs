using System;
using NLog;
using RIS.Extensions;
using LogManager = RIS.Logging.LogManager;

namespace DestroyComments.Utils
{
    public static class LogUtils
    {
        private static void LogLineInternal(LogLevel logLevel, ConsoleColor color,
            string text, string additionalInfo = null)
        {
            string line = null;

            line += string.IsNullOrEmpty(additionalInfo)
                ? string.Empty
                : $"AdditionalInfo - {additionalInfo} ||| ";
            line += text;

            LogLineInternal(logLevel,
                line, color);
        }
        private static void LogLineInternal(LogLevel logLevel,
            ConsoleColor color, Exception exception,
            string additionalInfo = null)
        {
            string line = null;

            line += string.IsNullOrEmpty(additionalInfo)
                ? string.Empty
                : $"AdditionalInfo - {additionalInfo} ||| ";
            line += exception.Message;

            LogLineInternal(logLevel,
                line, color);
        }
        private static void LogLineInternal(LogLevel logLevel,
            string line, ConsoleColor color)
        {
            if (string.IsNullOrEmpty(line))
                return;

            ConsoleExtensions.WriteLineColored(
                line, color);
            LogManager.Log.Log(
                logLevel, line);
        }



        public static void LogInformation(string text,
            string additionalInfo = null)
        {
            LogLineInternal(LogLevel.Info, ConsoleColor.Blue,
                text, additionalInfo);
        }
        public static void LogInformation(Exception exception,
            string additionalInfo = null)
        {
            LogLineInternal(LogLevel.Info, ConsoleColor.Blue,
                exception, additionalInfo);
        }

        public static void LogWarning(string text,
            string additionalInfo = null)
        {
            LogLineInternal(LogLevel.Warn, ConsoleColor.Yellow,
                text, additionalInfo);
        }
        public static void LogWarning(Exception exception,
            string additionalInfo = null)
        {
            LogLineInternal(LogLevel.Warn, ConsoleColor.Yellow,
                exception, additionalInfo);
        }

        public static void LogError(string text,
            string additionalInfo = null)
        {
            LogLineInternal(LogLevel.Error, ConsoleColor.Red,
                text, additionalInfo);
        }
        public static void LogError(Exception exception,
            string additionalInfo = null)
        {
            LogLineInternal(LogLevel.Error, ConsoleColor.Red,
                exception, additionalInfo);
        }

        public static void LogFatalError(string text,
            string additionalInfo = null)
        {
            LogLineInternal(LogLevel.Fatal, ConsoleColor.DarkRed,
                text, additionalInfo);
        }
        public static void LogFatalError(Exception exception,
            string additionalInfo = null)
        {
            LogLineInternal(LogLevel.Fatal, ConsoleColor.DarkRed,
                exception, additionalInfo);
        }
    }
}
