using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Yorozuya.WpfApp.Common;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.Servcies;

public class HttpPostService(HttpClient httpClient) : IPostService
{
    private readonly HttpClient _httpClient = httpClient;

    private static void AddAuthorization(HttpRequestMessage message, string token)
        => message.Headers.Authorization = new("Bearer", token);

    private static async Task HandleNoDataHttpResponseMessage(HttpResponseMessage httpResponseMessage)
    {
        httpResponseMessage.EnsureSuccessStatusCode();
        var apiResonse = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<object>>();
        ArgumentNullException.ThrowIfNull(apiResonse);
        apiResonse.EnsureSuccessStatusCode();
    }

    private static async Task<TModel> HandleDataHttpResponseMessage<TModel>(HttpResponseMessage httpResponseMessage)
    {
        httpResponseMessage.EnsureSuccessStatusCode();
        var apiResonse = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<TModel>>();
        ArgumentNullException.ThrowIfNull(apiResonse);
        apiResonse.EnsureSuccessStatusCode();
        ArgumentNullException.ThrowIfNull(apiResonse.Data);
        return apiResonse.Data;
    }

    private static async Task<IEnumerable<TModel>?> HandleIEnumerbleDataHttpResponseMessage<TModel>(string key, HttpResponseMessage httpResponseMessage)
    {
        httpResponseMessage.EnsureSuccessStatusCode();
        var apiResonse = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<Dictionary<string, JsonElement>>>();
        ArgumentNullException.ThrowIfNull(apiResonse);
        apiResonse.EnsureSuccessStatusCode();
        ArgumentNullException.ThrowIfNull(apiResonse.Data);
        return apiResonse.Data[key].Deserialize<IEnumerable<TModel>>();
    }

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
        await HandleNoDataHttpResponseMessage(await _httpClient.SendAsync(message));
    }

    public async Task CancelLikeAsync(string token, long replyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var message = new HttpRequestMessage(HttpMethod.Delete, $"api/post/cancelLike?replyId={replyId}");
        AddAuthorization(message, token);
        await HandleNoDataHttpResponseMessage(await _httpClient.SendAsync(message));
    }


    public async Task DeletePostAsync(string token, long postId)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var message = new HttpRequestMessage(HttpMethod.Delete, $"api/post/remove?postId={postId}");
        AddAuthorization(message, token);
        await HandleNoDataHttpResponseMessage(await _httpClient.SendAsync(message));
    }

    public async Task DeleteReplyAsync(string token, long replyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var message = new HttpRequestMessage(HttpMethod.Delete, $"api/post/deleteReply?replyId={replyId}");
        AddAuthorization(message, token);
        await HandleNoDataHttpResponseMessage(await _httpClient.SendAsync(message));
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
        return apiResonse.Data["isLiked"].GetBoolean();
    }

    public async Task<IEnumerable<Reply>?> GetPostRepliesAsync(long postId)
        => await HandleIEnumerbleDataHttpResponseMessage<Reply>("replyList", await _httpClient.GetAsync($"api/post/postReplies?postId={postId}"));


    public async Task<IEnumerable<Post>?> GetPostsByFieldAsync(string field)
        => await HandleIEnumerbleDataHttpResponseMessage<Post>("postList", await _httpClient.GetAsync($"api/post/getPostsByField?field={field}"));

    public async Task<IEnumerable<Post>?> GetUserPostsAsync(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var message = new HttpRequestMessage(HttpMethod.Get, "api/post/history");
        AddAuthorization(message, token);
        return await HandleIEnumerbleDataHttpResponseMessage<Post>("postList", await _httpClient.SendAsync(message));
    }

    public Task<IEnumerable<Reply>?> GetUserRepliesAsync(string token)
    {
        throw new NotImplementedException();
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
        await HandleNoDataHttpResponseMessage(await _httpClient.SendAsync(message));
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
        return await HandleDataHttpResponseMessage<Post>(await _httpClient.SendAsync(message));
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
        return await HandleDataHttpResponseMessage<Reply>(await _httpClient.SendAsync(message));
    }
}
