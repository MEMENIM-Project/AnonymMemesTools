using System;
using Memenim.Core.Schema;

namespace DestroyComments
{
    public class Comment
    {
        private CommentSchema Data { get; set; }

        public int Id { get; }
        public int? UserId { get; }
        public string UserNickname { get; }

        public Comment(CommentSchema data)
        {
            if (data == null)
                return;

            Data = data;
            Id = data.Id;
            UserId = data.User.Id;
            UserNickname = data.User.Nickname;
        }
    }
}
