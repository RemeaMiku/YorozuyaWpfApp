using System;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.Servcies.Local;

public class LocalUserService : IUserService
{
    readonly UserInfo _localUserInfo = new() { Id = 0, Username = "Developer" };

    public UserInfo? UserInfo
    {
        get => _localUserInfo;
        set => throw new NotImplementedException();
    }
}
