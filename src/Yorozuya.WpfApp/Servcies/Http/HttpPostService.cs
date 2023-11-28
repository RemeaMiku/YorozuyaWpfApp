using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Yorozuya.WpfApp.Common;
using Yorozuya.WpfApp.Common.Helpers;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.Servcies.Http;

public class HttpPostService(HttpClient httpClient) : BaseHttpService(httpClient), IPostService
{
    public async Task AcceptReplyAsync(string token, long replyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var message = new HttpRequestMessage(HttpMethod.Post, "api/post/accept")
        {
            Content = new MultipartFormDataContent()
            {
                { new StringContent(replyId.ToString()), "replyId" }
            }
        };
        AddAuthorization(message, token);
        await ApiResponseMessageHandler.HandleNoDataApiResponseMessage(await _httpClient.SendAsync(message));
    }

    public async Task CancelLikeAsync(string token, long replyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var message = new HttpRequestMessage(HttpMethod.Delete, $"api/post/cancelLike?replyId={replyId}");
        AddAuthorization(message, token);
        await ApiResponseMessageHandler.HandleNoDataApiResponseMessage(await _httpClient.SendAsync(message));
    }


    public async Task DeletePostAsync(string token, long postId)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var message = new HttpRequestMessage(HttpMethod.Delete, $"api/post/remove?postId={postId}");
        AddAuthorization(message, token);
        await ApiResponseMessageHandler.HandleNoDataApiResponseMessage(await _httpClient.SendAsync(message));
    }

    public async Task DeleteReplyAsync(string token, long replyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var message = new HttpRequestMessage(HttpMethod.Delete, $"api/post/deleteReply?replyId={replyId}");
        AddAuthorization(message, token);
        await ApiResponseMessageHandler.HandleNoDataApiResponseMessage(await _httpClient.SendAsync(message));
    }

    public async Task<bool> GetIsLikedAsync(string token, long replyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var message = new HttpRequestMessage(HttpMethod.Get, $"api/post/isLiked?replyId={replyId}");
        AddAuthorization(message, token);
        var httpResponseMessage = await _httpClient.SendAsync(message);
        httpResponseMessage.EnsureSuccessStatusCode();
        var apiResonse = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<Dictionary<string, JsonElement>>>();
        ArgumentNullException.ThrowIfNull(apiResonse);
        apiResonse.EnsureSuccessStatusCode();
        ArgumentNullException.ThrowIfNull(apiResonse.Data);
        return apiResonse.Data["isLiked"].GetInt32() > 0;
    }

    public async Task<IEnumerable<Reply>?> GetPostRepliesAsync(long postId)
        => await ApiResponseMessageHandler.HandleIEnumerbleModelDataApiResponseMessage<Reply>("replyList", await _httpClient.GetAsync($"api/post/postReplies?postId={postId}"));


    public async Task<IEnumerable<Post>?> GetPostsByFieldAsync(string field)
        => await ApiResponseMessageHandler.HandleIEnumerbleModelDataApiResponseMessage<Post>("postList", await _httpClient.GetAsync($"api/post/getPostsByField?field={field}"));

    public async Task<IEnumerable<Post>?> GetUserPostsAsync(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var message = new HttpRequestMessage(HttpMethod.Get, "api/post/history");
        AddAuthorization(message, token);
        var response = await _httpClient.SendAsync(message);
        try
        {
            return await ApiResponseMessageHandler.HandleIEnumerbleModelDataApiResponseMessage<Post>("postList",
                response);
        }
        catch (Exception _)
        {
            return new List<Post>();
        }
    }

    public async Task<IEnumerable<Reply>?> GetUserRepliesAsync(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var message = new HttpRequestMessage(HttpMethod.Get, "api/post/userReplies");
        AddAuthorization(message, token);
        var response = await _httpClient.SendAsync(message);
        try
        {
            return await ApiResponseMessageHandler.HandleIEnumerbleModelDataApiResponseMessage<Reply>("replyList",
                response);
        }
        catch (Exception e)
        {
            return new List<Reply>();
        }
    }

    public async Task LikeAsync(string token, long replyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var message = new HttpRequestMessage(HttpMethod.Post, "api/post/like")
        {
            Content = new MultipartFormDataContent()
            {
                { new StringContent(replyId.ToString()), "replyId" }
            }
        };
        AddAuthorization(message, token);
        await ApiResponseMessageHandler.HandleNoDataApiResponseMessage(await _httpClient.SendAsync(message));
    }


    public async Task<Post> PublishPostAsync(string token, string title, string content, string field)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var message = new HttpRequestMessage(HttpMethod.Post, "api/post/push")
        {
            Content = new MultipartFormDataContent()
            {
                { new StringContent(title), "title" },
                { new StringContent(content), "content" },
                { new StringContent(field), "field" },
            }
        };
        AddAuthorization(message, token);
        return await ApiResponseMessageHandler.HandleModelDataApiResponseMessage<Post>(await _httpClient.SendAsync(message));
    }

    public async Task<Reply> PublishReplyAsync(string token, long postId, string content)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var message = new HttpRequestMessage(HttpMethod.Post, "api/post/reply")
        {
            Content = new MultipartFormDataContent()
            {
                { new StringContent(postId.ToString()), "postId" },
                { new StringContent(content), "content" },
            }
        };
        AddAuthorization(message, token);
        return await ApiResponseMessageHandler.HandleModelDataApiResponseMessage<Reply>(await _httpClient.SendAsync(message));
    }

    public async Task<IEnumerable<Post>?> GetPostById(long postId)
    {
        var message = new HttpRequestMessage(HttpMethod.Get,
            "api/post/getPostByPostId?postId=" + Uri.EscapeDataString($"{postId}"));
        return await ApiResponseMessageHandler.HandleIEnumerbleModelDataApiResponseMessage<Post>(
            "postList", await _httpClient.SendAsync(message));
    }

    public async Task<IEnumerable<Post>?> GetPostByTitle(string title)
    {
        var message = new HttpRequestMessage(HttpMethod.Get,
            "api/post/getPostByTitle?title=" + Uri.EscapeDataString($"{title}"));
        try
        {
            return await ApiResponseMessageHandler.HandleIEnumerbleModelDataApiResponseMessage<Post>(
                "postList", await _httpClient.SendAsync(message));
        }
        catch (Exception _)
        {
            return new List<Post>();
        }
    }
}
