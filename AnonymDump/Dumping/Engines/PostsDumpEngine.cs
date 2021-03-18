using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AnonymDump.Database;
using AnonymDump.Settings;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using RIS.Collections.Nestable;
using RIS.Connection.MySQL.Requests;

namespace AnonymDump.Dumping.Engines
{
#pragma warning disable U2U1009 // Async or iterator methods should avoid state machine generation for early exits (throws or synchronous returns)
    public class PostsDumpEngine : IDumpEngine
    {
        private readonly Timer _autoUpdateTimer;

        private int _headId;

        public PostsDumpEngine()
        {
            _headId = SettingsManager.AppSettings.PostsOffset;

            _autoUpdateTimer = new Timer(TimeSpan.FromSeconds(60).TotalMilliseconds);
            _autoUpdateTimer.Elapsed += AutoUpdateTimerCallback;
            _autoUpdateTimer.Stop();
        }

        private async Task UpdateHeadId()
        {
            if (!DBConnection.IsOpened())
                return;

            var result = await PostApi.Get(PostType.New, 1, 0)
                .ConfigureAwait(false);

            if (result.IsError)
            {
                Program.LogError($"Request posts dump auto update error: headId={_headId}");

                return;
            }

            if (result.Data == null
                || result.Data.Count == 0
                || result.Data[0] == null)
            {
                Program.LogWarning($"Request posts dump update skipped: headId={_headId}");

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

            while (DBConnection.IsOpened())
            {
                try
                {
                    if (SettingsManager.AppSettings.PostsOffset >= _headId)
                    {
                        SettingsManager.AppSettings.PostsOffset = _headId;

                        SettingsManager.AppSettings.Save();

                        await Task.Delay(TimeSpan.FromSeconds(30))
                            .ConfigureAwait(false);

                        continue;
                    }

                    var startId = SettingsManager.AppSettings.PostsOffset;
                    var ids = Enumerable.Range(startId, 20).ToArray();

                    var result = await PostApi.GetById(
                            ids)
                        .ConfigureAwait(false);

                    if (result.IsError)
                    {
                        Program.LogError($"Request posts dump error: ids={string.Join(", ", ids)}");

                        //await Task.Delay(TimeSpan.FromMilliseconds(100))
                        //    .ConfigureAwait(false);

                        continue;
                    }

                    if (result.Data == null || result.Data.Count == 0)
                    {
                        SettingsManager.AppSettings.PostsOffset += 20;

                        SettingsManager.AppSettings.Save();

                        Program.LogWarning($"Request posts dump skipped: ids={string.Join(", ", ids)}");

                        //await Task.Delay(TimeSpan.FromMilliseconds(100))
                        //    .ConfigureAwait(false);

                        continue;
                    }

                    Parallel.ForEach(result.Data, async post =>
                    {
                        try
                        {
                            var tags = new NestableListL<string>();

                            foreach (var tag in post.Tags)
                            {
                                tags.Add(tag);
                            }

                            var attachments = new NestableListL<string>();

                            for (int i = 0; i < post.Attachments.Count; ++i)
                            {
                                var attachment = post.Attachments[i];

                                if (attachment == null)
                                    break;

                                var attachmentCollection = new NestableListL<string>();

                                attachmentCollection.Add(attachment.Type.ToString() ?? string.Empty);
                                attachmentCollection.Add(attachment.Link ?? string.Empty);

                                var photoCollection = new NestableListL<string>();

                                if (attachment.Photo != null)
                                {

                                    photoCollection.Add(attachment.Photo.SmallUrl ?? string.Empty);
                                    photoCollection.Add(attachment.Photo.MediumUrl ?? string.Empty);
                                    photoCollection.Add(attachment.Photo.BigUrl ?? string.Empty);

                                    var photoSizeCollection = new NestableListL<string>();

                                    var photoSizeSmallCollection = new NestableListL<string>
                                    {
                                        attachment.Photo.Size.Small?.Width.ToString() ?? "0",
                                        attachment.Photo.Size.Small?.Height.ToString() ?? "0"
                                    };

                                    photoSizeCollection.Add(photoSizeSmallCollection);

                                    var photoSizeMediumCollection = new NestableListL<string>
                                    {
                                        attachment.Photo.Size.Medium?.Width.ToString() ?? "0",
                                        attachment.Photo.Size.Medium?.Height.ToString() ?? "0"
                                    };

                                    photoSizeCollection.Add(photoSizeMediumCollection);

                                    var photoSizeBigCollection = new NestableListL<string>
                                    {
                                        attachment.Photo.Size.Big?.Width.ToString() ?? "0",
                                        attachment.Photo.Size.Big?.Height.ToString() ?? "0"
                                    };

                                    photoSizeCollection.Add(photoSizeBigCollection);

                                    photoCollection.Add(photoSizeCollection);
                                }

                                attachmentCollection.Add(photoCollection);

                                attachments.Add(attachmentCollection);
                            }

                            await ReplaceRequest.ExecuteAsync(DBConnection.RequestEngine, new[]
                                {
                                    post.Id.ToString(),
                                    post.Text,
                                    ((byte)post.Status).ToString(),
                                    Convert.ToByte(post.IsAnonymous).ToString(),
                                    Convert.ToByte(post.IsAdult).ToString(),
                                    Convert.ToByte(post.IsHidden).ToString(),
                                    Convert.ToByte(post.IsCommentsOpen).ToString(),
                                    post.CategoryId.ToString(),
                                    post.CategoryName,
                                    post.Filter?.ToString(),
                                    post.Type.ToString(),
                                    post.UtcDate.ToString(),
                                    post.OwnerId?.ToString(),
                                    post.OwnerNickname,
                                    post.OwnerPhotoUrl,
                                    post.Views.TotalCount.ToString(),
                                    post.Shares.ToString(),
                                    post.Likes.TotalCount.ToString(),
                                    post.Dislikes.TotalCount.ToString(),
                                    post.Comments.TotalCount.ToString(),
                                    tags.ToStringRepresent(),
                                    attachments.ToStringRepresent()

                                }, "Posts")
                                .ConfigureAwait(false);

                            Program.LogInfo(
                                $"Request posts dump success: id={post.Id}");

                            ApiResponse<List<CommentSchema>> resultComments;
                            int offsetComments = 0;

                            while (DBConnection.IsOpened())
                            {
                                resultComments = await PostApi.GetComments(
                                        post.Id,
                                        SettingsManager.AppSettings.CommentsCountPerTime,
                                        offsetComments)
                                    .ConfigureAwait(false);

                                if (resultComments.IsError)
                                {
                                    Program.LogError($"Request posts comments dump error: id={post.Id},offset={offsetComments}");

                                    //await Task.Delay(TimeSpan.FromMilliseconds(100))
                                    //    .ConfigureAwait(false);

                                    continue;
                                }

                                if (resultComments.Data == null || resultComments.Data.Count == 0)
                                {
                                    Program.LogWarning($"Request posts comments dump skipped: id={post.Id},offset={offsetComments}");

                                    //await Task.Delay(TimeSpan.FromMilliseconds(100))
                                    //    .ConfigureAwait(false);

                                    break;
                                }

                                Parallel.ForEach(resultComments.Data, async comment =>
                                {
                                    try
                                    {
                                        await ReplaceRequest.ExecuteAsync(DBConnection.RequestEngine, new[]
                                            {
                                                comment.Id.ToString(),
                                                post.Id.ToString(),
                                                comment.Text,
                                                Convert.ToByte(comment.IsAnonymous).ToString(),
                                                comment.UtcDate.ToString(),
                                                comment.User?.Id?.ToString(),
                                                comment.User?.Nickname,
                                                comment.User?.PhotoUrl,
                                                comment.Likes.TotalCount.ToString(),
                                                comment.Dislikes.TotalCount.ToString()
                                            }, "Comments")
                                            .ConfigureAwait(false);
                                    }
                                    catch (Exception ex)
                                    {
                                        Program.LogError($"Error: {ex.Message}");
                                        Program.LogWarning($"Request posts comments dump skipped: id={post.Id},commentId={comment.Id}");
                                    }
                                });

                                offsetComments += resultComments.Data.Count;

                                //await Task.Delay(TimeSpan.FromMilliseconds(100))
                                //    .ConfigureAwait(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            Program.LogError($"Error: {ex.Message}");
                            Program.LogWarning($"Request posts dump skipped: id={post.Id}");
                        }
                    });

                    SettingsManager.AppSettings.PostsOffset += 20;

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
