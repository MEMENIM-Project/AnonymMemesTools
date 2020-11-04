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
        public static Log LogFile { get; }

        static LogUtils()
        {
            LogFile = new Log(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }

        public static void LogInformation(string information, string additionalInfo = null)
        {
            string line = string.Empty;

            StackTrace trace = new StackTrace(false);
            StackFrame[] frames = trace.GetFrames();
            MethodBase caller = (frames.Length > 1 ? frames[1] : frames[0])?.GetMethod();

            string senderName = caller?.DeclaringType?.FullName;
            string callerName = caller?.Name;

            for (int i = 1; i < frames.Length; ++i)
            {
                MethodBase frameCaller = frames[i]?.GetMethod();

                if (frameCaller == null)
                    continue;

                Type frameSender = frameCaller.DeclaringType;
                string frameSenderName = frameSender?.FullName;
                string frameCallerName;

                if (string.IsNullOrEmpty(frameSenderName))
                    continue;

                string[] frameSenderComponents = frameSenderName.Split('+');

                if (frameSenderComponents.Length == 1)
                    continue;

                frameSenderName = frameSenderComponents[0];

                if (frameSenderComponents[1].StartsWith('<'))
                {
                    int endCharIndex = frameSenderComponents[1].IndexOf('>');

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

            line += $"sender = {senderName}";
            line += $" || caller = {callerName}";
            line += string.IsNullOrEmpty(additionalInfo)
                ? string.Empty
                : $" || {additionalInfo}";
            line += $" || information: {information}";

            ConsoleExtensions.WriteLineColored(line, ConsoleColor.Blue);

            LogFile.WriteLine(line, LogSituation.Information);
        }

        public static void LogWarning(string warning, string additionalInfo = null)
        {
            string line = string.Empty;

            StackTrace trace = new StackTrace(false);
            StackFrame[] frames = trace.GetFrames();
            MethodBase caller = (frames.Length > 1 ? frames[1] : frames[0])?.GetMethod();

            string senderName = caller?.DeclaringType?.FullName;
            string callerName = caller?.Name;

            for (int i = 1; i < frames.Length; ++i)
            {
                MethodBase frameCaller = frames[i]?.GetMethod();

                if (frameCaller == null)
                    continue;

                Type frameSender = frameCaller.DeclaringType;
                string frameSenderName = frameSender?.FullName;
                string frameCallerName;

                if (string.IsNullOrEmpty(frameSenderName))
                    continue;

                string[] frameSenderComponents = frameSenderName.Split('+');

                if (frameSenderComponents.Length == 1)
                    continue;

                frameSenderName = frameSenderComponents[0];

                if (frameSenderComponents[1].StartsWith('<'))
                {
                    int endCharIndex = frameSenderComponents[1].IndexOf('>');

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

            line += $"sender = {senderName}";
            line += $" || caller = {callerName}";
            line += string.IsNullOrEmpty(additionalInfo)
                ? string.Empty
                : $" || {additionalInfo}";
            line += $" || warning: {warning}";

            ConsoleExtensions.WriteLineColored(line, ConsoleColor.Yellow);

            LogFile.WriteLine(line, LogSituation.Warning);
        }

        public static void LogError(string error, string additionalInfo = null)
        {
            string line = string.Empty;

            StackTrace trace = new StackTrace(false);
            StackFrame[] frames = trace.GetFrames();
            MethodBase caller = (frames.Length > 1 ? frames[1] : frames[0])?.GetMethod();

            string senderName = caller?.DeclaringType?.FullName;
            string callerName = caller?.Name;

            for (int i = 1; i < frames.Length; ++i)
            {
                MethodBase frameCaller = frames[i]?.GetMethod();

                if (frameCaller == null)
                    continue;

                Type frameSender = frameCaller.DeclaringType;
                string frameSenderName = frameSender?.FullName;
                string frameCallerName;

                if (string.IsNullOrEmpty(frameSenderName))
                    continue;

                string[] frameSenderComponents = frameSenderName.Split('+');

                if (frameSenderComponents.Length == 1)
                    continue;

                frameSenderName = frameSenderComponents[0];

                if (frameSenderComponents[1].StartsWith('<'))
                {
                    int endCharIndex = frameSenderComponents[1].IndexOf('>');

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

            line += $"sender = {senderName}";
            line += $" || caller = {callerName}";
            line += string.IsNullOrEmpty(additionalInfo)
                ? string.Empty
                : $" || {additionalInfo}";
            line += $" || error: {error}";

            ConsoleExtensions.WriteLineColored(line, ConsoleColor.Red);

            LogFile.WriteLine(line, LogSituation.Error);
        }
    }
}
