using System;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using DestroyComments.Utils;
using Memenim.Core.Schema;

namespace DestroyComments
{
    public static class Program
    {
        private static bool _waitPressKeysActive;

        private static async Task Main()
        {
            Console.Title = "Anonym destroy comments tool";

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            try
            {
                int nodesCount = 1;
                int nodeBotsCount = 2;
                PostType nodePostsType = PostType.Popular;
                int nodePostsCount = 10;
                int nodePostsOffset = 0;

                nodesCount = ReadValue(nodesCount, nameof(nodesCount));
                nodeBotsCount = ReadValue(nodeBotsCount, nameof(nodeBotsCount));
                nodePostsType = Enum.Parse<PostType>(ReadValue(nodePostsType.ToString(), nameof(nodePostsType),
                    $"(valid values: {string.Join(", ", Enum.GetNames(typeof(PostType)))})"), true);
                nodePostsCount = ReadValue(nodePostsCount, nameof(nodePostsCount));
                nodePostsOffset = ReadValue(nodePostsOffset, nameof(nodePostsOffset));

                Console.WriteLine("\n\n");

                SpamEngine engine = new SpamEngine();

                await engine.Initialize(nodesCount, nodeBotsCount,
                        nodePostsType, nodePostsCount, nodePostsOffset)
                    .ConfigureAwait(false);

                engine.Start();

                WaitPressExitKey();

                engine.Cancel();

                await Task.Delay(TimeSpan.FromSeconds(3))
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogUtils.LogError(ex);
            }

            Console.WriteLine("\n\n");
            Console.ReadKey(true);
        }

        private static void WaitPressExitKey()
        {
            ConsoleKeyInfo keyInfo;

            do
            {
                keyInfo = Console.ReadKey(true);
            }
            while (keyInfo.Key != ConsoleKey.Escape);
        }

        private static void StartWaitPressKeys()
        {
            if (_waitPressKeysActive)
                return;

            Task.Run(() =>
            {
                while (_waitPressKeysActive)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.Escape:
                            //Environment.Exit(0x0);
                            break;
                    }
                }
            });
        }

        private static void StopWaitPressKeys()
        {
            _waitPressKeysActive = false;
        }

        public static T ReadValue<T>(T defaultValue, string valueName, string additionalInfo = null)
        {
            Console.Write($"Enter {valueName}" +
                          $"{(!string.IsNullOrEmpty(additionalInfo) ? $" | {additionalInfo} |" : string.Empty)} " +
                          $"(default = {defaultValue}): ");

            string inputLine = Console.ReadLine();

            return !string.IsNullOrWhiteSpace(inputLine)
                ? (T)Convert.ChangeType(inputLine, typeof(T))
                : defaultValue;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;

            LogUtils.LogCriticalError(ex);

            OnProcessExit(null, e);
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            LogUtils.LogFile.Close();
        }
    }
}
