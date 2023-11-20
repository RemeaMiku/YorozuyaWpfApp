using System;
using System.Threading.Tasks;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.Servcies.Local;

public class LocalUserService : IUserService
{
    UserInfo? _localUserInfo = new() { Id = 0, Username = "Developer", Field = ".NET" };

    public UserInfo? UserInfo
    {
        get => _localUserInfo;
        set => _localUserInfo = value;
    }

    public Task<UserInfo> UserLoginAsync()
    {
        throw new NotImplementedException();
    }

    public Task<UserInfo> UserRegisterAsync()
    {
        throw new NotImplementedException();
    }
}
