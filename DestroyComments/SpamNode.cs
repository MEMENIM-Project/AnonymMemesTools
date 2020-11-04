using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using DestroyComments.Utils;
using Memenim.Core.Schema;
using RIS.Synchronization;

namespace DestroyComments
{
    public class SpamNode
    {
        private Timer _botCheckTimer;

        public ReadOnlyCollection<AsyncLock> BotsSyncRoots { get; private set; }
        public CancellationTokenSource NodeCancellationToken { get; }

        public SpamEngine Engine { get; private set; }
        public Bot ServiceBot { get; }
        public ReadOnlyCollection<Post> Posts { get; private set; }
        private List<Bot> _botsList;
        public ReadOnlyCollection<Bot> Bots
        {
            get
            {
                return new ReadOnlyCollection<Bot>(_botsList);
            }
        }

        public bool Initialized { get; private set; }

        public SpamNode()
        {
            _botCheckTimer = null;

            BotsSyncRoots = new ReadOnlyCollection<AsyncLock>(new List<AsyncLock>());
            NodeCancellationToken = new CancellationTokenSource();

            Engine = null;
            ServiceBot = new Bot();
            Posts = new ReadOnlyCollection<Post>(new List<Post>());
            _botsList = new List<Bot>();
        }

        public async Task Initialize(SpamEngine engine,
            int botsCount, PostType postsType = PostType.Popular,
            int postsCount = 10, int postsOffset = 0)
        {
            if (botsCount == 0
                || postsCount == 0
                || postsOffset > int.MaxValue - postsCount)
            {
                Initialized = false;
                return;
            }

            if (engine == null)
            {
                Initialized = false;
                return;
            }

            try
            {
                Engine = engine;

                await ServiceBot.Initialize()
                    .ConfigureAwait(false);

                if (!ServiceBot.Initialized)
                {
                    Initialized = false;
                    return;
                }

                await Engine.SaveBotAccount(ServiceBot)
                    .ConfigureAwait(false);

                List<Post> postsList = await PostUtils.GetPosts(ServiceBot, postsType, postsCount, postsOffset)
                    .ConfigureAwait(false);

                if (postsList.Count == 0)
                {
                    Initialized = false;
                    return;
                }

                Posts = new ReadOnlyCollection<Post>(postsList);

                List<AsyncLock> botsSyncRootsList = new List<AsyncLock>(botsCount);
                List<Bot> botsList = new List<Bot>(botsCount);

                for (int i = 0; i < botsCount; ++i)
                {
                    Bot bot = await CreateBot()
                        .ConfigureAwait(false);

                    if (bot?.Initialized != true)
                    {
                        Initialized = false;
                        return;
                    }

                    botsSyncRootsList.Add(new AsyncLock());
                    botsList.Add(bot);
                }

                BotsSyncRoots = new ReadOnlyCollection<AsyncLock>(botsSyncRootsList);
                _botsList = botsList;

                _botCheckTimer = new Timer(BotCheckTimerCallback, this, -1L,
                    Convert.ToInt64(TimeSpan.FromMinutes(3).TotalMilliseconds));
            }
            catch (Exception)
            {
                Initialized = false;
                return;
            }

            Initialized = true;
        }

        public void Start()
        {
            if (!Engine.Initialized)
            {
                return;
            }

            Task.Run(() =>
            {
                while (true)
                {
                    NodeCancellationToken.Token.ThrowIfCancellationRequested();

                    SendComments();
                }
            }, NodeCancellationToken.Token);
        }

        public void Cancel()
        {
            NodeCancellationToken.Cancel();
        }

        public Post GetRandomPost()
        {
            if (Posts.Count == 0)
                return null;

            return Posts[RandomUtils.RandomInt(0, Posts.Count)];
        }

        public async Task<Bot> CreateBot()
        {
            Bot bot = null;

            do
            {
                Post post = GetRandomPost();
                Comment comment = post?.GetRandomComment();

                if (comment == null)
                    continue;

                int userId = comment.UserId;

                bot = await BotUtils.CopyProfile(userId)
                    .ConfigureAwait(false);
            } while (bot?.Initialized == false);

            await Engine.SaveBotAccount(bot)
                .ConfigureAwait(false);

            return bot;
        }

        public void SendComments()
        {
            Parallel.ForEach(_botsList, async (bot, _, index) =>
            {
                int i = Convert.ToInt32(index);

                Post post = GetRandomPost();

                if (post == null)
                    return;

                if (!post.CommentsIsOpen)
                    return;

                int postId = post.Id;

                string text = Engine.GetRandomCommentText();

                if (text == null)
                    return;

                using (await BotsSyncRoots[i].LockAsync(NodeCancellationToken.Token)
                    .ConfigureAwait(false))
                {
                    await PostUtils.SendComment(bot, postId, text)
                        .ConfigureAwait(false);
                }
            });
        }

#pragma warning disable SS001 // Async methods should return a Task to make them awaitable
#pragma warning disable U2U1003 // Avoid declaring methods used in delegate constructors static
        private static void BotCheckTimerCallback(object state)
        {
            try
            {
                if (!(state is SpamNode node) || node._botsList.Count == 0)
                    return;

                //for (var i = 0; i < node.Bots.Count; ++i)
                //{
                //    bool isBanned = await node.Bots[i].IsBanned()
                //        .ConfigureAwait(false);

                //    if (!isBanned)
                //        continue;

                //    using (await node.SyncRoot.LockAsync(node.NodeCancellationToken.Token)
                //        .ConfigureAwait(false))
                //    {
                //        node.Bots[i] = await node.CreateBot()
                //            .ConfigureAwait(false);
                //    }
                //}

                Parallel.ForEach(node._botsList, async (bot, _, index) =>
                {
                    int i = Convert.ToInt32(index);

                    bool isBanned = await bot.IsBanned()
                        .ConfigureAwait(false);

                    if (!isBanned)
                        return;

                    using (await node.BotsSyncRoots[i].LockAsync(node.NodeCancellationToken.Token)
                        .ConfigureAwait(false))
                    {
                        node._botsList[i] = await node.CreateBot()
                            .ConfigureAwait(false);
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
#pragma warning restore SS001 // Async methods should return a Task to make them awaitable
#pragma warning restore U2U1003 // Avoid declaring methods used in delegate constructors static
    }
}
