using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.Servcies.Local;

public class LocalPostService(IUserService userService) : IPostService
{
    readonly IUserService _userService = userService;
    //public async Task<bool> GetIsLikedAsync(Reply reply)
    //{
    //    ArgumentNullException.ThrowIfNull(_userService.UserInfo);
    //    await Task.Delay(1000);
    //    return _localLikes.Exists(l => l.ReplyId == reply.Id && l.UserId == _userService.UserInfo.Id);
    //}

    readonly List<Reply> _localReplies =
    [
        new() { Id = 0, PostId = 0, UserId = 0, CreateTime = "2023.08.31 11:45:14", Content = "回答0", Likes = 831 },
        new() { Id = 1, PostId = 0, UserId = 1, CreateTime = "2022.08.31 11:45:14", Content = "回答1", Likes = 123, IsAccepted = 1 },
        new() { Id = 2, PostId = 1, UserId = 2, CreateTime = "2021.08.31 11:45:14", Content = "回答2", Likes = 233 },
    ];

    readonly List<Like> _localLikes =
    [
        new() { UserId = 0, ReplyId = 0 },
        new() { UserId = 0, ReplyId = 2 },
    ];

    readonly List<Post> _localPosts =
    [
        new() { AskerId = 0, Content = "问题0xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", CreateTime = "2023.08.31 11:45:14", DelTag = 0, Id = 0, Title = "问题0", Views = 123, Field = "Test" },
        new() { AskerId = 1, Content = "问题1", CreateTime = "2023.08.31 11:45:14", DelTag = 0, Id = 1, Title = "问题1", Views = 831, Field = "Test" },
        new() { AskerId = 2, Content = "问题2", CreateTime = "2023.08.31 11:45:14", DelTag = 0, Id = 2, Title = "问题2", Views = 250, Field = "Test" },
        new() { AskerId = 3, Content = "问题3", CreateTime = "2023.08.31 11:45:14", DelTag = 0, Id = 3, Title = "问题3", Views = 39, Field = "Test" },
    ];

    public async Task AcceptReplyAsync(string token, long replyId)
    {
        ArgumentNullException.ThrowIfNull(token);
        _localReplies.Single(r => r.Id == replyId).IsAccepted = 1;
        await Task.Delay(500);
    }

    public async Task CancelLikeAsync(string token, long replyId)
    {
        ArgumentNullException.ThrowIfNull(token);
        _localLikes.Remove(_localLikes.Single(l => l.UserId == 0 && l.ReplyId == replyId));
        await Task.Delay(500);
    }

    public async Task DeletePostAsync(string token, long postId)
    {
        ArgumentNullException.ThrowIfNull(token);
        _localPosts.Remove(_localPosts.Single(p => p.Id == postId && p.AskerId == long.Parse(token)));
        await Task.Delay(500);
    }

    public async Task DeleteReplyAsync(string token, long replyId)
    {
        ArgumentNullException.ThrowIfNull(token);
        _localReplies.Remove(_localReplies.Single(r => r.Id == replyId && r.UserId == long.Parse(token)));
        await Task.Delay(500);
    }

    public async Task<bool> GetIsLikedAsync(string token, long replyId)
    {
        ArgumentNullException.ThrowIfNull(token);
        await Task.Delay(500);
        return _localLikes.Exists(l => l.UserId == long.Parse(token) && l.ReplyId == replyId);
    }

    public async Task<IEnumerable<Reply>?> GetPostRepliesAsync(long postId)
    {
        await Task.Delay(500);
        return _localReplies.Where(r => r.PostId == postId);
    }

    public async Task<IEnumerable<Post>?> GetPostsByFieldAsync(string field)
    {
        await Task.Delay(500);
        return _localPosts.Where(p => p.Field == field);
    }

    public async Task<IEnumerable<Post>?> GetUserPostsAsync(string token)
    {
        ArgumentNullException.ThrowIfNull(token);
        await Task.Delay(500);
        return _localPosts.Where(p => p.AskerId == long.Parse(token));
    }

    public async Task<IEnumerable<Reply>?> GetUserRepliesAsync(string token)
    {
        ArgumentNullException.ThrowIfNull(token);
        await Task.Delay(500);
        return _localReplies.Where(r => r.UserId == long.Parse(token));
    }

    public async Task LikeAsync(string token, long replyId)
    {
        ArgumentNullException.ThrowIfNull(token);
        await Task.Delay(500);
        _localLikes.Add(new() { ReplyId = replyId, UserId = 0 });
    }

    public async Task<Post> PublishPostAsync(string token, string title, string content, string field)
    {
        ArgumentNullException.ThrowIfNull(token);
        await Task.Delay(500);
        var post = new Post()
        {
            Id = _localPosts.MaxBy(p => p.Id)!.Id + 1,
            AskerId = long.Parse(token),
            Title = title,
            Content = content,
            Field = field,
            CreateTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"),
            UpdateTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss")
        };
        _localPosts.Add(post);
        return post;
    }

    public async Task<Reply> PublishReplyAsync(string token, long postId, string content)
    {
        ArgumentNullException.ThrowIfNull(token);
        await Task.Delay(500);
        var reply = new Reply()
        {
            Id = _localReplies.MaxBy(r => r.Id)!.Id + 1,
            PostId = postId,
            UserId = long.Parse(token),
            Content = content,
            CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        _localReplies.Add(reply);
        return reply;
    }

    //public async Task<IEnumerable<Reply>?> GetPostRepliesAsync(Post post)
    //{
    //    //var response = await _httpClient.GetFromJsonAsync<Response<PostRepliesData>>($"api/post/allReplies?postId={post.Id}");
    //    //return response == null ? null : response.Data!.ReplyList;
    //    ArgumentNullException.ThrowIfNull(_userService.UserInfo);
    //    await Task.Delay(1000);
    //    post.Views++;
    //    return _localReplies.Where(p => p.PostId == post.Id);
    //}

    //public bool GetIsUserPost(Post post)
    //{
    //    ArgumentNullException.ThrowIfNull(_userService.UserInfo);
    //    return post.AskerId == _userService.UserInfo.Id;
    //}

    //public bool GetIsUserReply(Reply reply)
    //{
    //    ArgumentNullException.ThrowIfNull(_userService.UserInfo);
    //    return reply.UserId == _userService.UserInfo.Id;
    //}

    //public async Task AcceptReplyAsync(Post post, Reply reply)
    //{
    //    ArgumentNullException.ThrowIfNull(_userService.UserInfo);
    //    await Task.Delay(1000);
    //    reply.IsAccepted = 1;
    //}

    //public async Task<Reply> ReplyPostAsync(Post post, string replyContent)
    //{
    //    ArgumentNullException.ThrowIfNull(_userService.UserInfo);
    //    await Task.Delay(1000);
    //    var reply = new Reply() { Content = replyContent, CreateTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"), PostId = post.Id, UserId = _userService.UserInfo.Id };
    //    _localReplies.Add(reply);
    //    return reply;
    //}

    //public async Task LikeAsync(Reply reply)
    //{
    //    ArgumentNullException.ThrowIfNull(_userService.UserInfo);
    //    await Task.Delay(1000);
    //    //reply.Likes++;
    //    _localLikes.Add(new() { ReplyId = reply.Id, UserId = _userService.UserInfo.Id });
    //}

    //public async Task CancelLikeAsync(Reply reply)
    //{
    //    ArgumentNullException.ThrowIfNull(_userService.UserInfo);
    //    await Task.Delay(1000);
    //    //reply.Likes--;
    //    _localLikes.RemoveAll(l => l.ReplyId == reply.Id && l.UserId == _userService.UserInfo.Id);
    //}

    //public async Task DeletePostAsync(Post post)
    //{
    //    ArgumentNullException.ThrowIfNull(_userService.UserInfo);
    //    if (post.AskerId != _userService.UserInfo.Id)
    //        throw new InvalidOperationException("");
    //    await Task.Delay(1000);
    //    post.DelTag = 1;
    //}

    //public async Task DeleteReplyAsync(Reply reply)
    //{
    //    ArgumentNullException.ThrowIfNull(_userService.UserInfo);
    //    if (reply.UserId != _userService.UserInfo.Id)
    //        throw new InvalidOperationException("");
    //    await Task.Delay(1000);
    //    reply.DelTag = 1;
    //    _localReplies.Remove(reply);
    //}

    //public async Task<IEnumerable<Post>?> GetPostsByFieldAsync(string filed)
    //{
    //    ArgumentNullException.ThrowIfNull(_userService.UserInfo);
    //    await Task.Delay(500);
    //    var temp = new List<Post>();
    //    Random random = new();
    //    for (int i = 0; i <= 10; ++i)
    //    {
    //        temp.Add(new Post()
    //        {
    //            AskerId = i,
    //            Content = $"aya kawaii {random.Next()} 文文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏",
    //            CreateTime = DateTime.Now.ToString(CultureInfo.CurrentCulture),
    //            DelTag = 0,
    //            Field = filed,
    //            Id = i,
    //            Title = $"Is aya kawaii? {i} {filed} 文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏文文可爱捏",
    //            UpdateTime = DateTime.Now.ToString(CultureInfo.CurrentCulture),
    //            Views = 233 * i,
    //        });
    //    }
    //    return temp;
    //}

    //public async Task<IEnumerable<Post>?> GetUserPostsAsync(string token)
    //{
    //    await Task.Delay(500);
    //    return new[]
    //    {
    //        new Post
    //        {
    //            AskerId = 0, Content = "问题0", CreateTime = "2023.08.31 11:45:14", DelTag = 0, Id = 0, Title = "问题0",
    //            Views = 123, Field = "Test"
    //        },
    //        new Post
    //        {
    //            AskerId = 1, Content = "问题1", CreateTime = "2023.08.31 11:45:14", DelTag = 0, Id = 1, Title = "问题1",
    //            Views = 831, Field = "Test"
    //        },
    //        new Post
    //        {
    //            AskerId = 2, Content = "问题2", CreateTime = "2023.08.31 11:45:14", DelTag = 0, Id = 2, Title = "问题2",
    //            Views = 250, Field = "Test"
    //        },
    //    };
    //}

    //public async Task<IEnumerable<Reply>?> GetUserRepliesAsync(string token)
    //{
    //    await Task.Delay(500);
    //    return new[]
    //    {
    //        new Reply
    //        {
    //            Id = 0, PostId = 0, UserId = 0, CreateTime = "2023.08.31 11:45:14", Content = "回答0", Likes = 831
    //        },
    //        new Reply
    //        {
    //            Id = 1, PostId = 0, UserId = 1, CreateTime = "2022.08.31 11:45:14", Content = "回答1", Likes = 123,
    //            IsAccepted = 1
    //        },
    //        new Reply
    //        {
    //            Id = 2, PostId = 1, UserId = 2, CreateTime = "2021.08.31 11:45:14", Content = "回答2", Likes = 233
    //        },
    //    };
    //}
}
