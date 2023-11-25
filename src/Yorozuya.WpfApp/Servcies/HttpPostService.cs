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
        apiResonse.EnsureSuccess();
    }

    private static async Task<TModel> HandleDataHttpResponseMessage<TModel>(HttpResponseMessage httpResponseMessage)
    {
        httpResponseMessage.EnsureSuccessStatusCode();
        var apiResonse = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<TModel>>();
        ArgumentNullException.ThrowIfNull(apiResonse);
        apiResonse.EnsureSuccess();
        ArgumentNullException.ThrowIfNull(apiResonse.Data);
        return apiResonse.Data;
    }

    private static async Task<IEnumerable<TModel>?> HandleIEnumerbleDataHttpResponseMessage<TModel>(string name, HttpResponseMessage httpResponseMessage)
    {
        httpResponseMessage.EnsureSuccessStatusCode();
        var apiResonse = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<Dictionary<string, JsonElement>>>();
        ArgumentNullException.ThrowIfNull(apiResonse);
        apiResonse.EnsureSuccess();
        ArgumentNullException.ThrowIfNull(apiResonse.Data);
        return apiResonse.Data[name].Deserialize<IEnumerable<TModel>>();
    }

    public async Task AcceptReplyAsync(string token, long replyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var message = new HttpRequestMessage(HttpMethod.Put, "api/post/accept")
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
        throw new NotImplementedException();
    }


    public async Task DeletePostAsync(string token, long postId)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteReplyAsync(string token, long replyId)
    {
        throw new NotImplementedException();
    }


    public Task<bool> GetIsLikedAsync(string token, long replyId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Reply>?> GetPostRepliesAsync(long postId)
        => await HandleIEnumerbleDataHttpResponseMessage<Reply>("replyList", await _httpClient.GetAsync($"api/post/postReplies?postId={postId}"));


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

    public async Task LikeAsync(string token, long replyId)
    {
        throw new NotImplementedException();
    }


    public async Task<Post> PublishPostAsync(string token, string title, string content, string field)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var multipartFormDataContentcontent = new MultipartFormDataContent()
        {
            { new StringContent(title), "title" },
            { new StringContent(content), "content" },
            { new StringContent(field), "field" },
        };
        multipartFormDataContentcontent.Headers.Add("Authorization", token);
        return await HandleDataHttpResponseMessage<Post>(await _httpClient.PostAsync("api/post/push", multipartFormDataContentcontent));
    }

    public async Task<Reply> PublishReplyAsync(string token, long postId, string content)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        var multipartFormDataContentcontent = new MultipartFormDataContent()
        {
            { new StringContent(postId.ToString()), "postId" },
            { new StringContent(content), "content" },
        };
        multipartFormDataContentcontent.Headers.Add("Authorization", token);
        return await HandleDataHttpResponseMessage<Reply>(await _httpClient.PostAsync("api/post/reply", multipartFormDataContentcontent));
    }
}
