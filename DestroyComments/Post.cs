using System;
using System.Threading.Tasks;
using DestroyComments.Utils;
using Memenim.Core.Api;
using Memenim.Core.Schema;

namespace DestroyComments
{
    public class Post
    {
        private PostSchema Data { get; set; }

        public int Id { get; private set; }
        public CommentsList CommentsList { get; }
        public bool CommentsIsOpen
        {
            get
            {
                return Data?.open_comments == 1;
            }
        }

        public bool Initialized { get; private set; }

        public Post()
        {
            Data = null;
            Id = -1;
            CommentsList = new CommentsList();
        }

        public async Task Initialize(Bot bot, int id)
        {
            if (id < 0)
            {
                Initialized = false;
                return;
            }

            if (bot?.Initialized != true)
            {
                Initialized = false;
                return;
            }

            try
            {
                var result = await PostApi.GetById(bot.Token, id)
                    .ConfigureAwait(false);

                if (result.error)
                {
                    if (result.message ==
                        "[cluster_block_exception] blocked by: [FORBIDDEN/12/index read-only / allow delete (api)];")
                    {
                        LogUtils.LogWarning(result.message,
                            $"botId = {bot.Id}, botLogin = {bot.Login}, id = {Id}");
                    }
                    else
                    {
                        LogUtils.LogError(result.message,
                            $"botId = {bot.Id}, botLogin = {bot.Login}, id = {Id}");
                        Initialized = false;
                        return;
                    }
                }

                if (result.data == null)
                {
                    Initialized = false;
                    return;
                }

                Data = result.data;
                Id = result.data.id;

                await CommentsList.Initialize(Id, result.data.comments.count)
                    .ConfigureAwait(false);

                if (!CommentsList.Initialized)
                {
                    Initialized = false;
                    return;
                }
            }
            catch (Exception)
            {
                Initialized = false;
                return;
            }

            Initialized = true;
        }

        public Comment GetRandomComment()
        {
            return CommentsList.GetRandomComment();
        }
    }
}
