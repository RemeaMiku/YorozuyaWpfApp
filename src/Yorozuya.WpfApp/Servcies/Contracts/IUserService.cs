using System;
using System.Threading.Tasks;
using Yorozuya.WpfApp.Models;

namespace Yorozuya.WpfApp.Servcies.Contracts;

public interface IUserService
{
    public UserInfo? UserInfo { get; protected set; }
    public string Token { get; protected set; }

    public bool IsUserLoggedIn => UserInfo is not null;

    public void UserLogout()
    {
        if (IsUserLoggedIn)
            UserInfo = default;
        else
            throw new InvalidOperationException("User not logged in.");
    }

    public Task<UserInfo> UserRegisterAsync();

    public Task<UserInfo> UserLoginAsync();
}
