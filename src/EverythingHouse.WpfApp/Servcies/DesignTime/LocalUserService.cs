using System;
using EverythingHouse.WpfApp.Models;
using EverythingHouse.WpfApp.Servcies.Contracts;

namespace EverythingHouse.WpfApp.Servcies.DesignTime;

public class LocalUserService : IUserService
{
    readonly UserInfo _localUserInfo = new() { Id = 0 };

    public UserInfo? UserInfo
    {
        get => _localUserInfo;
        set => throw new NotImplementedException();
    }
}
