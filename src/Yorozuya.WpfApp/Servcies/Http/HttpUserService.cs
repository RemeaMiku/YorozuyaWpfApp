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
using Yorozuya.WpfApp.Common.Helpers;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.Servcies.Http;

public class HttpUserService(HttpClient httpClient) : BaseHttpService(httpClient), IUserService
{
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

    public async Task UserLoginAsync(string username, string password)
    {
        if (UserInfo is not null || Token is not null)
            throw new InvalidOperationException("The previous account have to be logged out before logging in new account.");
        var content = new MultipartFormDataContent
        {
            { new StringContent(username), "username" },
            { new StringContent(GetMd5Hash(password)), "password" }
        };
        var httpResponseMessage = await _httpClient.PostAsync("api/user/login", content);
        httpResponseMessage.EnsureSuccessStatusCode();
        var apiResonse = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<Dictionary<string, JsonElement>>>();
        ArgumentNullException.ThrowIfNull(apiResonse);
        apiResonse.EnsureSuccessStatusCode();
        ArgumentNullException.ThrowIfNull(apiResonse.Data);
        var token = apiResonse.Data["token"].GetString();
        var userInfo = apiResonse.Data["userInfo"].Deserialize<UserInfo>();
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(userInfo);
        Token = token;
        UserInfo = userInfo;
    }

    public void UserLogout()
    {
        if (UserInfo is null || Token is null)
            throw new InvalidOperationException("The user not logged in.");
        UserInfo = default;
        Token = default;
    }

    public async Task UserRegisterAsync(string username, string password, string field, int gender)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(username), "username" },
            { new StringContent(GetMd5Hash(password)), "password" },
            { new StringContent(field), "field" },
            { new StringContent(gender.ToString()), "gender" }
        };
        var httpResponseMessage = await _httpClient.PostAsync("api/user/register", content);
        httpResponseMessage.EnsureSuccessStatusCode();
        var apiResonse = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<object>>();
        ArgumentNullException.ThrowIfNull(apiResonse);
        apiResonse.EnsureSuccessStatusCode();
    }
}
