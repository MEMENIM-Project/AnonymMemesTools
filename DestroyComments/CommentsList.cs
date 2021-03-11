using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DestroyComments.Utils;
using Memenim.Core.Api;

namespace DestroyComments
{
    public class CommentsList
    {
        public int PostId { get; private set; }
        public ReadOnlyCollection<Comment> Comments { get; private set; }

        public bool Initialized { get; private set; }

        public CommentsList()
        {
            PostId = -1;
            Comments = new ReadOnlyCollection<Comment>(new List<Comment>());
        }

        public async Task Initialize(int postId, int commentsCount)
        {
            if (postId < 0)
            {
                Initialized = false;
                return;
            }

            try
            {
                var result = await PostApi.GetComments(postId, commentsCount)
                    .ConfigureAwait(false);

                if (result.IsError)
                {
                    LogUtils.LogError(result.Message,
                        $"postId = {postId}");
                    Initialized = false;
                    return;
                }

                PostId = postId;

                List<Comment> commentsList = new List<Comment>(result.Data.Count);

                foreach (var comment in result.Data)
                {
                    commentsList.Add(new Comment(comment));
                }

                Comments = new ReadOnlyCollection<Comment>(commentsList);
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
            if (Comments.Count == 0)
                return null;

            return Comments[RandomUtils.RandomInt(0, Comments.Count)];
        }
    }
}
