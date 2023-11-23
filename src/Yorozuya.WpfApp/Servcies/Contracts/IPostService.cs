using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yorozuya.WpfApp.Models;

namespace Yorozuya.WpfApp.Servcies.Contracts;

public interface IPostService
{
    public Task<IEnumerable<Post>?> GetPostsByFieldAsync(string field);

    public Task<IEnumerable<Reply>?> GetPostRepliesAsync(long postId);

    public Task<IEnumerable<Post>?> GetUserPostsAsync(string token);

    public Task<IEnumerable<Reply>?> GetUserRepliesAsync(string token);

    public Task<bool> GetIsLikedAsync(string token, long replyId);

    public Task AcceptReplyAsync(string token, long replyId);

    public Task<Reply> PublishReplyAsync(string token, long postId, string content);

    public Task LikeAsync(string token, long replyId);

    public Task CancelLikeAsync(string token, long replyId);

    public Task DeletePostAsync(string token, long postId);

    public Task DeleteReplyAsync(string token, long replyId);

    public Task<Post> PublishPostAsync(string token, string title, string content, string field);
}
