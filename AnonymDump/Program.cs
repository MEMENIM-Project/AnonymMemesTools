using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using AnonymDump.Database;
using AnonymDump.Dumping;
using AnonymDump.Dumping.Engines;
using AnonymDump.Logging;
using AnonymDump.Settings;
using Memenim.Core.Api;
using RIS;
using RIS.Extensions;
using Environment = RIS.Environment;

namespace AnonymDump
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
#pragma warning disable SS002 // DateTime.Now was referenced
            NLog.GlobalDiagnosticsContext.Set("AppStartupTime",
                DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss", CultureInfo.InvariantCulture));
#pragma warning restore SS002 // DateTime.Now was referenced

            Console.Title = "Anonym dump tool";

            Events.Information += OnInformation;
            Events.Warning += OnWarning;
            Events.Error += OnError;

            ApiRequestEngine.Information += OnCoreInformation;
            ApiRequestEngine.Warning += OnCoreWarning;
            ApiRequestEngine.Error += OnCoreError;

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.TypeResolve += OnResolve;
            AppDomain.CurrentDomain.ResourceResolve += OnResolve;

            ApiRequestEngine.ConnectionStateChanged += OnConnectionStateChanged;

            LogManager.Log.Info("App Run");

            LogManager.Log.Info($"Libraries Directory - {Environment.ExecAppDirectoryName}");
            LogManager.Log.Info($"Execution File Directory - {Environment.ExecProcessDirectoryName}");
            LogManager.Log.Info($"Is Standalone App - {Environment.IsStandalone}");
            LogManager.Log.Info($"Is Single File App - {Environment.IsSingleFile}");
            LogManager.Log.Info($"Runtime Name - {Environment.RuntimeName}");
            LogManager.Log.Info($"Runtime Version - {Environment.RuntimeVersion}");
            LogManager.Log.Info($"Runtime Identifier - {Environment.RuntimeIdentifier}");

            LogManager.Log.Info("Deleted older logs - " +
                                $"{LogManager.DeleteLogs(Path.Combine(Environment.ExecProcessDirectoryName, "logs"), SettingsManager.AppSettings.LogRetentionDaysPeriod)}");
            LogManager.Log.Info("Deleted older debug logs - " +
                                $"{LogManager.DeleteLogs(Path.Combine(Environment.ExecProcessDirectoryName, "logs", "debug"), SettingsManager.AppSettings.LogRetentionDaysPeriod)}");

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



        private static void WaitPressExitKey()
        {
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);
            } while (key.Key != ConsoleKey.Escape);
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

        private static void Exit()
        {
            SettingsManager.AppSettings.Save();

            System.Environment.Exit(0x0);
        }



        public static void LogInfo(string message)
        {
            ConsoleExtensions.WriteLineColored(message, ConsoleColor.Gray);
            LogManager.Log.Info(message);
        }

        public static void LogWarning(string message)
        {
            ConsoleExtensions.WriteLineColored(message, ConsoleColor.Yellow);
            LogManager.Log.Warn(message);
        }

        public static void LogError(string message)
        {
            ConsoleExtensions.WriteLineColored(message, ConsoleColor.Red);
            LogManager.Log.Error(message);
        }



        private static void OnConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            if (e.NewState != ConnectionStateType.Disconnected)
                LogInfo($"Connection state changed to: {e.NewState}");
            else
                LogError($"Connection state changed to: {e.NewState}");
        }



        private static void OnInformation(object sender, RInformationEventArgs e)
        {
            LogManager.DebugLog.Info($"{e.Message}");
        }

        private static void OnWarning(object sender, RWarningEventArgs e)
        {
            LogManager.Log.Warn($"{e.Message}");
        }

        private static void OnError(object sender, RErrorEventArgs e)
        {
            LogManager.Log.Error($"{e.SourceException?.GetType().Name ?? "Unknown"} - Message={e.Message ?? (e.SourceException?.Message ?? "Unknown")},HResult={e.SourceException?.HResult ?? 0},StackTrace=\n{e.SourceException?.StackTrace ?? "Unknown"}");
        }



        private static void OnCoreInformation(object sender, CoreInformationEventArgs e)
        {
            //LogManager.DebugLog.Info($"{e.Message}");
        }

        private static void OnCoreWarning(object sender, CoreWarningEventArgs e)
        {
            LogManager.Log.Warn($"{e.Message}");
        }

        private static void OnCoreError(object sender, CoreErrorEventArgs e)
        {
            LogManager.Log.Error($"{e.SourceException?.GetType().Name ?? "Unknown"} - Message={e.Message ?? (e.SourceException?.Message ?? "Unknown")},HResult={e.SourceException?.HResult ?? 0},StackTrace=\n{e.Stacktrace ?? (e.SourceException?.StackTrace ?? "Unknown")}");
        }



        private static void OnProcessExit(object sender, EventArgs e)
        {
            SettingsManager.AppSettings.Save();

            DBConnection.Close();
            LogManager.Log.Info($"App Exit Code - {System.Environment.ExitCode}");
            NLog.LogManager.Shutdown();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;

            LogManager.Log.Fatal($"{exception?.GetType().Name ?? "Unknown"} - Message={exception?.Message ?? "Unknown"},HResult={exception?.HResult ?? 0},StackTrace=\n{exception?.StackTrace ?? "Unknown"}");

            SettingsManager.AppSettings.Save();

            DBConnection.Close();
            LogManager.Log.Info($"App Exit Code - {System.Environment.ExitCode}");
            NLog.LogManager.Shutdown();
        }

        private static void OnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            LogManager.DebugLog.Error($"{e.Exception.GetType().Name} - Message={e.Exception.Message},HResult={e.Exception.HResult},StackTrace=\n{e.Exception.StackTrace ?? "Unknown"}");
        }



        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs e)
        {
            LogManager.DebugLog.Info($"Resolve - Name={e.Name ?? "Unknown"},RequestingAssembly={e.RequestingAssembly?.FullName ?? "Unknown"}");

            return e.RequestingAssembly;
        }

        private static Assembly OnResolve(object sender, ResolveEventArgs e)
        {
            LogManager.DebugLog.Info($"Resolve - Name={e.Name ?? "Unknown"},RequestingAssembly={e.RequestingAssembly?.FullName ?? "Unknown"}");

            return e.RequestingAssembly;
        }
    }
}