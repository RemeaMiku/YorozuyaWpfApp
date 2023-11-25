using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.Servcies.Local;

public class LocalUserService : IUserService
{
    readonly List<UserInfo> _localUserInfos =
        [
           new() { Id = 0, Username = "Developer", Password = "Password", Field = ".NET" },
        ];

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
        return UserInfo is not null;
    }

    public async Task UserLoginAsync(string username, string password)
    {
        await Task.Delay(500);
        var userInfo = _localUserInfos.SingleOrDefault(u => u.Username == username && u.Password == password) ?? throw new InvalidOperationException("登录失败");
        UserInfo = userInfo;
        //本地Token就是UserId
        Token = UserInfo.Id.ToString();
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
        await Task.Delay(500);
        if (_localUserInfos.Exists(u => u.Username == username))
            throw new InvalidOperationException("注册失败");
        _localUserInfos.Add(new()
        {
            Id = _localUserInfos.Count,
            Username = username,
            Password = password,
            Field = field,
            Gender = gender,
            CreateTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"),
            UpdateTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"),
        });
    }
}
