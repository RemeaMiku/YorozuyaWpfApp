using System;
using System.Threading.Tasks;
using Yorozuya.WpfApp.Models;

namespace Yorozuya.WpfApp.Servcies.Contracts;

public interface IUserService
{
    public bool TryGetUserInfo(out UserInfo? userInfo);

    public bool TryGetToken(out string? token);

    public UserInfo? UserInfo { get; }

    public string? Token { get; }

    public bool IsUserLoggedIn => UserInfo is not null && Token is not null;

    public void UserLogout();

    public Task UserRegisterAsync(string username, string password, string field, int gender);

    public Task<UserInfo> UserLoginAsync(string username, string password);
}
