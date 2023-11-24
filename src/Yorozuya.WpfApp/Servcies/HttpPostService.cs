using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.Servcies;

public class HttpPostService : IPostService
{
    public static Uri BaseAddress { get; } = new("http://127.0.0.1:4523/m1/3553693-0-default/");

    private readonly HttpClient _httpClient = new() { BaseAddress = BaseAddress };

    public Task AcceptReplyAsync(string token, long replyId)
    {
        throw new NotImplementedException();
    }

    public Task CancelLikeAsync(string token, long replyId)
    {
        throw new NotImplementedException();
    }

    public Task DeletePostAsync(string token, long postId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteReplyAsync(string token, long replyId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> GetIsLikedAsync(string token, long replyId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Reply>?> GetPostRepliesAsync(long postId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Post>?> GetPostsByFieldAsync(string field)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Post>?> GetUserPostsAsync(string token)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Reply>?> GetUserRepliesAsync(string token)
    {
        throw new NotImplementedException();
    }

    public Task LikeAsync(string token, long replyId)
    {
        throw new NotImplementedException();
    }

    public Task<Post> PublishPostAsync(string token, string title, string content, string field)
    {
        throw new NotImplementedException();
    }

    public Task<Reply> PublishReplyAsync(string token, long postId, string content)
    {
        throw new NotImplementedException();
    }
}
