using System;
using System.Threading.Tasks;
using AnonymDump.Database;
using AnonymDump.Dumping;
using AnonymDump.Dumping.Engines;
using AnonymDump.Settings;
using Memenim.Core.Api;
using RIS.Extensions;
using RIS.Logging;

namespace AnonymDump
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "Anonym dump tool";

            LogManager.Startup();
            LogManager.LoggingShutdown += LogManager_OnLoggingShutdown;

            ApiRequestEngine.ConnectionStateChanged += OnConnectionStateChanged;

            ApiRequestEngine.Information += OnCoreInformation;
            ApiRequestEngine.Warning += OnCoreWarning;
            ApiRequestEngine.Error += OnCoreError;

            LogManager.DeleteLogs(SettingsManager.AppSettings
                .LogRetentionDaysPeriod);

            Task.Run(async () =>
            {
                LogInfo("App start");

                await Start()
                    .ConfigureAwait(false);

                Console.WriteLine("\n\n");
                LogInfo("App stop");
                Console.ReadKey(true);

                Exit();
            });

            WaitPressExitKey();

            Exit();
        }



        private static T ReadValue<T>(T defaultValue,
            string valueName, string additionalInfo = null)
        {
            Console.Write($"Enter {valueName}" +
                          $"{(!string.IsNullOrEmpty(additionalInfo) ? $" | {additionalInfo} |" : string.Empty)} " +
                          $"(default = {defaultValue}): ");

            string inputLine = Console.ReadLine();

            return !string.IsNullOrWhiteSpace(inputLine)
                ? (T)Convert.ChangeType(inputLine, typeof(T))
                : defaultValue;
        }

#pragma warning disable U2U1009 // Async or iterator methods should avoid state machine generation for early exits (throws or synchronous returns)
        private static async Task Start()
        {
            LogInfo("Connection to database...");

            if (!DBConnection.IsOpened())
            {
                LogError("Connection error");

                return;
            }

            LogInfo("Connection complete");

            try
            {
                DumpEngineType dumpEngineType = DumpEngineType.Users;

                dumpEngineType = Enum.Parse<DumpEngineType>(ReadValue(dumpEngineType.ToString(), nameof(dumpEngineType),
                    $"(valid values: {string.Join(", ", Enum.GetNames(typeof(DumpEngineType)))})"), true);

                IDumpEngine dump = new UsersDumpEngine();

                switch (dumpEngineType)
                {
                    case DumpEngineType.Users:
                        dump = new UsersDumpEngine();
                        break;
                    case DumpEngineType.Posts:
                        dump = new PostsDumpEngine();
                        break;
                    default:
                        break;
                }

                await dump.Start()
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError($"Error: {ex.Message}");
            }
        }
#pragma warning restore U2U1009 // Async or iterator methods should avoid state machine generation for early exits (throws or synchronous returns)

        private static void Exit()
        {
            SettingsManager.AppSettings.Save();

            System.Environment.Exit(0x0);
        }

        private static void WaitPressExitKey()
        {
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);
            } while (key.Key != ConsoleKey.Escape);
        }



        public static void LogInfo(string message)
        {
            ConsoleExtensions.WriteLineColored(
                message, ConsoleColor.Gray);
            LogManager.Log.Info(
                message);
        }

        public static void LogWarning(string message)
        {
            ConsoleExtensions.WriteLineColored(
                message, ConsoleColor.Yellow);
            LogManager.Log.Warn(
                message);
        }

        public static void LogError(string message)
        {
            ConsoleExtensions.WriteLineColored(
                message, ConsoleColor.Red);
            LogManager.Log.Error(
                message);
        }



        private static void OnConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            if (e.NewState != ConnectionStateType.Disconnected)
                LogInfo($"Connection state changed to: {e.NewState}");
            else
                LogError($"Connection state changed to: {e.NewState}");
        }



        private static void OnCoreInformation(object sender, CoreInformationEventArgs e)
        {
            //LogManager.DebugLog.Info($"{(!string.IsNullOrEmpty(e.Message) ? e.Message : "Unknown")}");
        }

        private static void OnCoreWarning(object sender, CoreWarningEventArgs e)
        {
            LogManager.Log.Warn($"{(!string.IsNullOrEmpty(e.Message) ? e.Message : "Unknown")}");
        }

        private static void OnCoreError(object sender, CoreErrorEventArgs e)
        {
            LogManager.Log.Error($"{e.SourceException?.GetType().Name ?? "Unknown"} - Message={(!string.IsNullOrEmpty(e.Message) ? e.Message : "Unknown")},HResult={e.SourceException?.HResult ?? 0},StackTrace=\n{e.SourceException?.StackTrace ?? "Unknown"}");
        }



        private static void LogManager_OnLoggingShutdown(object sender, EventArgs e)
        {
            SettingsManager.AppSettings.Save();
            DBConnection.Close();

            ApiRequestEngine.ConnectionStateChanged -= OnConnectionStateChanged;

            ApiRequestEngine.Information -= OnCoreInformation;
            ApiRequestEngine.Warning -= OnCoreWarning;
            ApiRequestEngine.Error -= OnCoreError;
        }
    }
}