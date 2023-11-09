using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EverythingHouse.WpfApp.Models;

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
