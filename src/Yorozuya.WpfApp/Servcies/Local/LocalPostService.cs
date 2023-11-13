using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Yorozuya.WpfApp.Common;
using Yorozuya.WpfApp.Common.ResponseData;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.Servcies.Local;

public class LocalPostService : IPostService
{
    public LocalPostService(IUserService userService)
    {
        _userService = userService;
    }

    readonly IUserService _userService;

    //readonly HttpClient _httpClient = new()
    //{
    //    BaseAddress = new("http://127.0.0.1:4523/m1/3553693-0-default/")
    //};

    public async Task<bool> GetIsLikedAsync(Reply reply)
    {
        //var userId = _userService.UserInfo?.Id;
        //var response = await _httpClient.GetFromJsonAsync<Response<IsLikedData>>($"api/post/isLiked?userId={userId}&replyId={reply.Id}");
        //ArgumentNullException.ThrowIfNull(response);
        //return response.Data!.IsLiked;
        ArgumentNullException.ThrowIfNull(_userService.UserInfo);
        await Task.Delay(1000);
        return _localLikes.Exists(l => l.ReplyId == reply.Id && l.UserId == _userService.UserInfo.Id);
    }

    readonly List<Reply> _localReplies = new()
    {
        new () { Id = 0, UserId = 0, CreateTime = "2023.08.31 11:45:14", Content = "回答0",Likes = 831 },
        new () { Id = 1, UserId = 1, CreateTime = "2022.08.31 11:45:14", Content = "回答1",Likes = 123, IsAccepted = true },
        new () { Id = 2, UserId = 2, CreateTime = "2021.08.31 11:45:14", Content = "回答2",Likes = 233 },
    };

    readonly List<Like> _localLikes = new()
    {
        new(){ UserId = 0, ReplyId = 0 },
        new(){ UserId = 0, ReplyId = 2 },
    };

    public async Task<IEnumerable<Reply>?> GetPostRepliesAsync(Post post)
    {
        //var response = await _httpClient.GetFromJsonAsync<Response<PostRepliesData>>($"api/post/allReplies?postId={post.Id}");
        //return response == null ? null : response.Data!.ReplyList;
        await Task.Delay(1000);
        return _localReplies;
    }

    public bool GetIsUserPost(Post post)
    {
        ArgumentNullException.ThrowIfNull(_userService.UserInfo);
        return post.AskerId == _userService.UserInfo.Id;
    }

    public bool GetIsUserReply(Reply reply)
    {
        ArgumentNullException.ThrowIfNull(_userService.UserInfo);
        return reply.UserId == _userService.UserInfo.Id;
    }

    public async Task AcceptReplyAsync(Post post, Reply reply)
    {
        await Task.Delay(1000);
        reply.IsAccepted = true;
    }

    public async Task ReplyPostAsync(Post post, Reply reply)
    {
        await Task.Delay(1000);
        reply.CreateTime = DateTime.Now.ToString();
        _localReplies.Add(reply);
    }

    public async Task LikeAsync(Reply reply)
    {
        await Task.Delay(1000);
        ArgumentNullException.ThrowIfNull(_userService.UserInfo);
        reply.Likes++;
        _localLikes.Add(new() { ReplyId = reply.Id, UserId = _userService.UserInfo.Id });
    }

    public async Task CancelLikeAsync(Reply reply)
    {
        await Task.Delay(1000);
        ArgumentNullException.ThrowIfNull(_userService.UserInfo);
        reply.Likes--;
        _localLikes.RemoveAll(l => l.ReplyId == reply.Id && l.UserId == _userService.UserInfo.Id);
    }

    public async Task DeletePostAsync(Post post)
    {
        await Task.Delay(1000);
        post.DelTag = 1;
    }

    public async Task DeleteReplyAsync(Reply reply)
    {
        await Task.Delay(1000);
        reply.DelTag = 1;
        _localReplies.Remove(reply);
    }


}
