using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using RIS.Extensions;
using RIS.Logging;

namespace DestroyComments.Utils
{
    public static class LogUtils
    {
        private const string DefaultGetCallInfoError = "Error determining the call location";

        public static Log LogFile { get; }

        static LogUtils()
        {
            LogFile = new Log(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }

        private static (string senderName, string callerName) GetCallInfo(
            int skippedFrames = 0)
        {
            try
            {
                if (skippedFrames < 0)
                    skippedFrames = 0;

                StackTrace trace = new StackTrace(false);
                StackFrame[] frames = trace.GetFrames();

                if (frames.Length - 1 < skippedFrames + 1)
                    skippedFrames = frames.Length - 2;

                MethodBase caller = frames[skippedFrames + 1]?.GetMethod();
                string senderName = caller?.DeclaringType?.FullName;
                string callerName = caller?.Name;

                for (int i = skippedFrames + 1; i < frames.Length; ++i)
                {
                    MethodBase frameCaller = frames[i]?.GetMethod();

                    if (frameCaller == null)
                        continue;

                    Type frameSender = frameCaller.DeclaringType;
                    string frameSenderName = frameSender?.FullName;
                    string frameCallerName = frameCaller.Name;

                    if (string.IsNullOrEmpty(frameSenderName))
                        continue;

                    string[] frameSenderComponents = frameSenderName.Split('+');

                    if (frameSenderComponents.Length == 1)
                    {
                        if (i == skippedFrames + 1)
                        {
                            senderName = frameSenderName;
                            callerName = frameCallerName;

                            break;
                        }

                        continue;
                    }

                    frameSenderName = frameSenderComponents[0];

                    if (frameSenderComponents[1].StartsWith('<'))
                    {
                        int endCharIndex = frameSenderComponents[1].IndexOf('>');

                        //string frameCallerNameExt = frameSenderComponents[1].Substring(endCharIndex + 1);

                        //if (frameCallerNameExt.StartsWith("d__"))
                        //    break;

                        frameCallerName = frameSenderComponents[1].Substring(1, endCharIndex - 1);
                    }
                    else
                    {
                        frameCallerName = frameSenderComponents[1];
                    }

                    senderName = frameSenderName;
                    callerName = frameCallerName;

                    break;
                }

                if (senderName == null && callerName == null)
                    throw new NullReferenceException("couldn't get either senderName or callerName");

                senderName ??= DefaultGetCallInfoError;
                callerName ??= DefaultGetCallInfoError;

                return (senderName, callerName);
            }
            catch (Exception ex)
            {
                LogError(ex.Message, $"Type = {ex.GetType().Name}, HResult = {ex.HResult}");
                return (DefaultGetCallInfoError, DefaultGetCallInfoError);
            }
        }

        private static string GenerateLogLine(LogSituation situation, string text,
            string additionalInfo = null, int skippedFrames = 0)
        {
            if (text == null)
                return string.Empty;

            string line = string.Empty;
            string situationText = LogUtilities.GetTextFromSituation(situation).SituationText;

            (string senderName, string callerName) = GetCallInfo(skippedFrames + 1);

            line += "Sender = " +
                    $"{senderName ?? DefaultGetCallInfoError}";
            line += " || Caller = " +
                    $"{callerName ?? DefaultGetCallInfoError}";
            line += string.IsNullOrEmpty(additionalInfo)
                ? string.Empty
                : $" || {additionalInfo}";
            line += $" || {situationText}: {text}";

            return line;
        }
        private static string GenerateLogLine(LogSituation situation, Exception exception,
            string additionalInfo = null, int skippedFrames = 0)
        {
            if (exception == null)
                return string.Empty;

            string line = string.Empty;
            string situationText = LogUtilities.GetTextFromSituation(situation).SituationText;

            (string senderName, string callerName) = GetCallInfo(skippedFrames + 1);

            line += "Sender = " +
                    $"{senderName ?? DefaultGetCallInfoError}";
            line += " || Caller = " +
                    $"{callerName ?? DefaultGetCallInfoError}";
            line += " || SourceSender = " +
                    $"{exception.TargetSite?.DeclaringType?.FullName ?? DefaultGetCallInfoError}";
            line += " || SourceCaller = " +
                    $"{exception.TargetSite?.Name ?? DefaultGetCallInfoError}";
            line += " || Type = " +
                    $"{exception.GetType().FullName ?? exception.GetType().Name}";
            line += " || HResult = " +
                    $"{exception.HResult}";

            if (exception.Data.Count > 0)
            {
                line += " || Data = ";

                foreach (var dataKey in exception.Data.Keys)
                {
                    if (dataKey == null)
                        continue;

                    var dataValue = exception.Data[dataKey];

                    line += $"{{{dataKey} :: {dataValue}}}, ";
                }

                line = line.Substring(0, line.Length - 2);
            }

            line += string.IsNullOrEmpty(additionalInfo)
                ? string.Empty
                : $" || {additionalInfo}";
            line += $" || {situationText}: {exception.Message}";

            return line;
        }

        private static void LogLineInternal(LogSituation situation, ConsoleColor color,
            string text, string additionalInfo = null, int skippedFrames = 0)
        {
            string line = GenerateLogLine(situation, text,
                additionalInfo, skippedFrames + 1);

            if (string.IsNullOrEmpty(line))
                return;

            ConsoleExtensions.WriteLineColored(line, color);
            LogFile.WriteLine(line, situation);
        }
        private static void LogLineInternal(LogSituation situation, ConsoleColor color,
            Exception exception, string additionalInfo = null, int skippedFrames = 0)
        {
            string line = GenerateLogLine(situation, exception,
                additionalInfo, skippedFrames + 1);

            if (string.IsNullOrEmpty(line))
                return;

            ConsoleExtensions.WriteLineColored(line, color);
            LogFile.WriteLine(line, situation);
        }



        public static void LogInformation(string text,
            string additionalInfo = null, int skippedFrames = 0)
        {
            LogLineInternal(LogSituation.Information, ConsoleColor.Blue,
                text, additionalInfo, skippedFrames + 1);
        }
        public static void LogInformation(Exception exception,
            string additionalInfo = null, int skippedFrames = 0)
        {
            LogLineInternal(LogSituation.Information, ConsoleColor.Blue,
                exception, additionalInfo, skippedFrames + 1);
        }

        public static void LogWarning(string text,
            string additionalInfo = null, int skippedFrames = 0)
        {
            LogLineInternal(LogSituation.Warning, ConsoleColor.Yellow,
                text, additionalInfo, skippedFrames + 1);
        }
        public static void LogWarning(Exception exception,
            string additionalInfo = null, int skippedFrames = 0)
        {
            LogLineInternal(LogSituation.Warning, ConsoleColor.Yellow,
                exception, additionalInfo, skippedFrames + 1);
        }

        public static void LogError(string text,
            string additionalInfo = null, int skippedFrames = 0)
        {
            LogLineInternal(LogSituation.Error, ConsoleColor.Red,
                text, additionalInfo, skippedFrames + 1);
        }
        public static void LogError(Exception exception,
            string additionalInfo = null, int skippedFrames = 0)
        {
            LogLineInternal(LogSituation.Error, ConsoleColor.Red,
                exception, additionalInfo, skippedFrames + 1);
        }

        public static void LogCriticalError(string text,
            string additionalInfo = null, int skippedFrames = 0)
        {
            LogLineInternal(LogSituation.CriticalError, ConsoleColor.DarkRed,
                text, additionalInfo, skippedFrames + 1);
        }
        public static void LogCriticalError(Exception exception,
            string additionalInfo = null, int skippedFrames = 0)
        {
            LogLineInternal(LogSituation.CriticalError, ConsoleColor.DarkRed,
                exception, additionalInfo, skippedFrames + 1);
        }
    }
}
