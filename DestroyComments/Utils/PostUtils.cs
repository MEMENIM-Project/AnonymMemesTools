using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Memenim.Core.Api;
using Memenim.Core.Schema;

namespace DestroyComments.Utils
{
    public static class PostUtils
    {
        public static async Task<List<Post>> GetPosts(Bot bot, PostType type = PostType.Popular, int count = 20, int offset = 0)
        {
            if (bot?.Initialized != true)
                return new List<Post>();

            try
            {
                var result = await PostApi.Get(bot.Token, type, count, offset)
                    .ConfigureAwait(false);

                if (result.error)
                {
                    if (result.message ==
                        "[cluster_block_exception] blocked by: [FORBIDDEN/12/index read-only / allow delete (api)];")
                    {
                        LogUtils.LogWarning(result.message,
                            $"botId = {bot.Id}, botLogin = {bot.Login}, type = {type}, count = {count}, offset = {offset}");
                    }
                    else
                    {
                        LogUtils.LogError(result.message,
                            $"botId = {bot.Id}, botLogin = {bot.Login}, type = {type}, count = {count}, offset = {offset}");
                        return new List<Post>();
                    }
                }

                if (result.data.Count == 0)
                    return new List<Post>();

                List<Post> postsList = new List<Post>(result.data.Count);

                foreach (var postData in result.data)
                {
                    Post post = new Post();

                    await post.Initialize(bot, postData.id)
                        .ConfigureAwait(false);

                    if (!post.Initialized)
                        continue;

                    postsList.Add(post);
                }

                return postsList;
            }
            catch (Exception)
            {
                return new List<Post>();
            }
        }

        public static async Task<Post> GetPost(Bot bot, int id)
        {
            if (bot?.Initialized != true)
                return null;

            try
            {
                Post post = new Post();

                await post.Initialize(bot, id)
                    .ConfigureAwait(false);

                if (!post.Initialized)
                    return null;

                return post;
            }
            catch (Exception)
            {
                return null;
            }
        }



        public static async Task SendComment(Bot bot, int postId, string text)
        {
            if (bot?.Initialized != true)
                return;

            try
            {
                var result = await PostApi.AddComment(bot.Token, postId, text)
                    .ConfigureAwait(false);

                if (result.error)
                {
                    if (result.message ==
                        "[cluster_block_exception] blocked by: [FORBIDDEN/12/index read-only / allow delete (api)];")
                    {
                        LogUtils.LogWarning(result.message,
                            $"botId = {bot.Id}, botLogin = {bot.Login}, postId = {postId}");
                    }
                    else
                    {
                        LogUtils.LogError(result.message,
                            $"botId = {bot.Id}, botLogin = {bot.Login}, postId = {postId}");
                        return;
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }



        public static async Task<bool> SendTestComment(Bot bot)
        {
            if (bot?.Initialized != true)
                return false;

            try
            {
                List<Post> postsList = await GetPosts(bot, PostType.Popular, 10, 0)
                    .ConfigureAwait(false);

                if (postsList.Count == 0)
                    return false;

                Post post;

                do
                {
                    post = postsList[RandomUtils.RandomInt(0, postsList.Count)];
                } while (!post.CommentsIsOpen);

                int postId = post.Id;
                string text = RandomUtils.GenerateString(5, 10);

                var result = await PostApi.AddComment(bot.Token, postId, text)
                    .ConfigureAwait(false);

                if (result.error)
                {
                    if (result.message ==
                        "[cluster_block_exception] blocked by: [FORBIDDEN/12/index read-only / allow delete (api)];")
                    {
                        LogUtils.LogWarning(result.message,
                            $"botId = {bot.Id}, botLogin = {bot.Login}, postId = {postId}");
                    }
                    else
                    {
                        LogUtils.LogError(result.message,
                            $"botId = {bot.Id}, botLogin = {bot.Login}, postId = {postId}");
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static async Task<bool> SendTestLike(Bot bot)
        {
            if (bot?.Initialized != true)
                return false;

            try
            {
                List<Post> postsList = await GetPosts(bot, PostType.Popular, 10, 0)
                    .ConfigureAwait(false);

                if (postsList.Count == 0)
                    return false;

                Post post = postsList[RandomUtils.RandomInt(0, postsList.Count)];
                int postId = post.Id;

                var result = await PostApi.AddLike(bot.Token, postId)
                    .ConfigureAwait(false);

                if (result.error)
                {
                    if (result.message ==
                        "[cluster_block_exception] blocked by: [FORBIDDEN/12/index read-only / allow delete (api)];")
                    {
                        LogUtils.LogWarning(result.message,
                            $"botId = {bot.Id}, botLogin = {bot.Login}, postId = {postId}");
                    }
                    else
                    {
                        LogUtils.LogError(result.message,
                            $"botId = {bot.Id}, botLogin = {bot.Login}, postId = {postId}");
                        return false;
                    }
                }

                result = await PostApi.RemoveLike(bot.Token, postId)
                    .ConfigureAwait(false);

                if (result.error)
                {
                    if (result.message ==
                        "[cluster_block_exception] blocked by: [FORBIDDEN/12/index read-only / allow delete (api)];")
                    {
                        LogUtils.LogWarning(result.message,
                            $"botId = {bot.Id}, botLogin = {bot.Login}, postId = {postId}");
                    }
                    else
                    {
                        LogUtils.LogError(result.message,
                            $"botId = {bot.Id}, botLogin = {bot.Login}, postId = {postId}");
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
