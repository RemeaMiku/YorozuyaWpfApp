using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Yorozuya.WpfApp.Common;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.Servcies;

public class HttpUserService : IUserService
{
    public static Uri BaseAddress { get; } = new("http://127.0.0.1:4523/m1/3553693-0-default/");

    private readonly HttpClient _httpClient = new() { BaseAddress = BaseAddress };

    public UserInfo? UserInfo { get; private set; }

    public string? Token { get; private set; }

    public bool TryGetToken([NotNullWhen(true)] out string? token)
    {
        token = Token;
        return Token is not null;
    }

    public bool TryGetUserInfo([NotNullWhen(true)] out UserInfo? userInfo)
    {
        userInfo = UserInfo;
        return Token is not null;
    }

    private static string GetMd5Hash(string input)
    {
        var data = MD5.HashData(Encoding.UTF8.GetBytes(input));
        var builder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
            builder.Append(data[i].ToString("x2"));
        return builder.ToString();
    }

    public async Task<UserInfo> UserLoginAsync(string username, string password)
    {
        var content = new MultipartFormDataContent();
        content.Headers.Add("ContentType", "multipart/form-data");
        content.Add(new StringContent(username), "username");
        content.Add(new StringContent(GetMd5Hash(password)), "password");
        var httpResponseMessage = await _httpClient.PostAsync("api/user/login", content);
        httpResponseMessage.EnsureSuccessStatusCode();
        var apiResonse = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<Dictionary<string, JsonElement>>>();
        ArgumentNullException.ThrowIfNull(apiResonse);
        apiResonse.EnsureSuccess();
        ArgumentNullException.ThrowIfNull(apiResonse.Data);
        Token = apiResonse.Data["token"].GetString();
        UserInfo = apiResonse.Data["userInfo"].Deserialize<UserInfo>();
        ArgumentNullException.ThrowIfNull(UserInfo);
        return UserInfo;
    }

    public void UserLogout()
    {
        if (UserInfo is null || Token is null)
            throw new InvalidOperationException("User not logged in.");
        UserInfo = default;
        Token = default;
    }

    public async Task UserRegisterAsync(string username, string password, string field, int gender)
    {
        var content = new MultipartFormDataContent();
        content.Headers.Add("ContentType", "multipart/form-data");
        content.Add(new StringContent(username), "username");
        content.Add(new StringContent(GetMd5Hash(password)), "password");
        content.Add(new StringContent(field), "field");
        content.Add(new StringContent(gender.ToString()), "gender");
        var httpResponseMessage = await _httpClient.PostAsync("api/user/register", content);
        httpResponseMessage.EnsureSuccessStatusCode();
        var apiResonse = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<object>>();
        ArgumentNullException.ThrowIfNull(apiResonse);
        apiResonse.EnsureSuccess();
    }
}
