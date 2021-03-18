using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AnonymDump.Database;
using AnonymDump.Settings;
using Memenim.Core.Api;
using RIS.Connection.MySQL.Requests;

namespace AnonymDump.Dump.Engines
{
#pragma warning disable U2U1009 // Async or iterator methods should avoid state machine generation for early exits (throws or synchronous returns)
    public class UsersDumpEngine : IDumpEngine
    {
        private readonly Timer _autoUpdateTimer;

        private int _headId;

        public UsersDumpEngine()
        {
            _headId = SettingsManager.AppSettings.UsersOffset;

            _autoUpdateTimer = new Timer(TimeSpan.FromSeconds(60).TotalMilliseconds);
            _autoUpdateTimer.Elapsed += AutoUpdateTimerCallback;
            _autoUpdateTimer.Stop();
        }

        private async Task UpdateHeadId()
        {
            if (!DBConnection.IsOpened())
                return;

            var result = await UserApi.GetUsers(1, 0)
                .ConfigureAwait(false);

            if (result.IsError)
            {
                Program.LogError($"Request users dump auto update error: headId={_headId}");

                return;
            }

            if (result.Data == null
                || result.Data.Count == 0
                || result.Data[0] == null)
            {
                Program.LogWarning($"Request users dump update skipped: headId={_headId}");

                return;
            }

            _headId = result.Data[0].Id;
        }

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
        public async Task Start()
        {
            if (!DBConnection.IsOpened())
                return;

            await UpdateHeadId()
                .ConfigureAwait(false);

            _autoUpdateTimer.Start();

            //while (DBConnection.IsOpened())
            //{
            //    try
            //    {
            //        if (SettingsManager.AppSettings.UsersOffset >= _headId)
            //        {
            //            SettingsManager.AppSettings.UsersOffset = _headId;

            //            await Task.Delay(TimeSpan.FromSeconds(30))
            //                .ConfigureAwait(false);

            //            continue;
            //        }

            //        var result = await UserApi.GetProfileById(
            //                SettingsManager.AppSettings.UsersOffset)
            //            .ConfigureAwait(false);

            //        if (result.IsError)
            //        {
            //            Program.LogError($"Request users dump error: id={SettingsManager.AppSettings.UsersOffset}");

            //            //await Task.Delay(TimeSpan.FromMilliseconds(100))
            //            //    .ConfigureAwait(false);

            //            continue;
            //        }

            //        if (result.Data == null)
            //        {
            //            ++SettingsManager.AppSettings.UsersOffset;

            //            SettingsManager.AppSettings.Save();

            //            Program.LogWarning($"Request users dump skipped: id={SettingsManager.AppSettings.UsersOffset - 1}");

            //            //await Task.Delay(TimeSpan.FromMilliseconds(100))
            //            //    .ConfigureAwait(false);

            //            continue;
            //        }

            //        ++SettingsManager.AppSettings.UsersOffset;

            //        SettingsManager.AppSettings.Save();

            //        Program.LogInfo($"Request users dump success: id={SettingsManager.AppSettings.UsersOffset - 1}");

            //        //await Task.Delay(TimeSpan.FromMilliseconds(100))
            //        //    .ConfigureAwait(false);
            //    }
            //    catch (Exception ex)
            //    {
            //        Program.LogError($"Error: {ex.Message}");
            //    }

            while (DBConnection.IsOpened())
            {
                try
                {
                    if (SettingsManager.AppSettings.UsersOffset >= _headId)
                    {
                        SettingsManager.AppSettings.UsersOffset = _headId;

                        SettingsManager.AppSettings.Save();

                        await Task.Delay(TimeSpan.FromSeconds(30))
                            .ConfigureAwait(false);

                        continue;
                    }

                    var startId = SettingsManager.AppSettings.UsersOffset;
                    var ids = Enumerable.Range(startId, 20).ToArray();

                    var result = await UserApi.GetUserById(
                            ids)
                        .ConfigureAwait(false);

                    if (result.IsError)
                    {
                        Program.LogError($"Request users dump error: ids={string.Join(", ", ids)}");

                        //await Task.Delay(TimeSpan.FromMilliseconds(100))
                        //    .ConfigureAwait(false);

                        continue;
                    }

                    if (result.Data == null || result.Data.Count == 0)
                    {
                        SettingsManager.AppSettings.UsersOffset += 20;

                        SettingsManager.AppSettings.Save();

                        Program.LogWarning($"Request users dump skipped: ids={string.Join(", ", ids)}");

                        //await Task.Delay(TimeSpan.FromMilliseconds(100))
                        //    .ConfigureAwait(false);

                        continue;
                    }

                    Parallel.ForEach(result.Data, async user =>
                    {
                        try
                        {
                            await ReplaceRequest.ExecuteAsync(DBConnection.RequestEngine, new[]
                                {
                                    user.Id.ToString(),
                                    user.RocketId,
                                    user.Nickname,
                                    "0",
                                    "0",
                                    null,
                                    user.PhotoUrl,
                                    user.BannerUrl,
                                    user.Login,
                                    ((byte)user.Status).ToString(),
                                    null,
                                    null,
                                    null,
                                    "0",
                                    null,
                                    null,
                                    null,
                                    null,
                                    null,
                                    null,
                                    null
                                }, "Users")
                                .ConfigureAwait(false);

                            Program.LogInfo(
                                $"Request users dump success: id={user.Id}");
                        }
                        catch (Exception ex)
                        {
                            Program.LogError($"Error: {ex.Message}");
                            Program.LogWarning($"Request users dump skipped: id={user.Id}");
                        }
                    });

                    SettingsManager.AppSettings.UsersOffset += 20;

                    SettingsManager.AppSettings.Save();

                    //await Task.Delay(TimeSpan.FromMilliseconds(100))
                    //    .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Program.LogError($"Error: {ex.Message}");
                }
            }

            _autoUpdateTimer.Stop();
        }
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

        private async void AutoUpdateTimerCallback(object sender, ElapsedEventArgs e)
        {
            if (!_autoUpdateTimer.Enabled)
                return;

            await UpdateHeadId()
                .ConfigureAwait(false);
        }
    }
#pragma warning restore U2U1009 // Async or iterator methods should avoid state machine generation for early exits (throws or synchronous returns)
}
