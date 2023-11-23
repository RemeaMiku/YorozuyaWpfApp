using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.Servcies.Local;

public class LocalUserService : IUserService
{
    readonly UserInfo _localUserInfo = new() { Id = 0, Username = "Developer", Password = "Password", Field = ".NET" };

    public UserInfo? UserInfo { get; private set; }
    public string? Token { get; private set; }

    public bool IsUserLoggedIn => throw new NotImplementedException();

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

    public async Task<UserInfo> UserLoginAsync(string username, string password)
    {
        await Task.Delay(500);
        if (username == _localUserInfo.Username && password == _localUserInfo.Password)
        {
            UserInfo = _localUserInfo;
            //本地Token就是UserId
            Token = UserInfo.Id.ToString();
            return UserInfo;
        }
        throw new InvalidOperationException("登录失败");
    }

    public void UserLogout()
    {
        if (IsUserLoggedIn)
        {
            UserInfo = default;
            Token = default;
        }
        else
            throw new InvalidOperationException("User not logged in.");
    }

    public Task UserRegisterAsync(string username, string password, string field, int gender)
    {
        throw new NotImplementedException();
    }
}
