using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EverythingHouse.WpfApp.Common;
using EverythingHouse.WpfApp.Common.ResponseData;
using EverythingHouse.WpfApp.Models;

namespace EverythingHouse.WpfApp.Servcies.DesignTime;

public class LocalPostService : IPostService
{
    public LocalPostService(IUserService userService)
    {
        _userService = userService;
    }

    readonly IUserService _userService;

    readonly HttpClient _httpClient = new()
    {
        BaseAddress = new("http://127.0.0.1:4523/m1/3553693-0-default/")
    };

    public async Task<bool?> GetIsLikedAsync(Reply reply)
    {
        var userId = _userService.UserInfo?.Id;
        var response = await _httpClient.GetFromJsonAsync<Response<IsLikedData>>($"api/post/isLiked?userId={userId}&replyId={reply.Id}");
        return response == null ? null : response.Data!.IsLiked;
    }

    public async Task<IEnumerable<Reply>?> GetPostRepliesAsync(Post post)
    {
        var response = await _httpClient.GetFromJsonAsync<Response<PostRepliesData>>($"api/post/allReplies?postId={post.Id}");
        return response == null ? null : response.Data!.ReplyList;
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

    public Task AcceptReplyAsync(Post post, Reply reply)
    {
        throw new NotImplementedException();
    }

    public Task ReplyPostAsync(Post post, Reply reply)
    {
        throw new NotImplementedException();
    }

    public Task LikeReplyAsync(Reply reply)
    {
        throw new NotImplementedException();
    }

    public Task CancelLikeAsync(Reply reply)
    {
        throw new NotImplementedException();
    }

    public Task DeletePostAsync(Post post)
    {
        throw new NotImplementedException();
    }

    public Task DeleteReplyAsync(Reply reply)
    {
        throw new NotImplementedException();
    }
}
