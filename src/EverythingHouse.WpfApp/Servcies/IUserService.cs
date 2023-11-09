using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EverythingHouse.WpfApp.Models;

namespace EverythingHouse.WpfApp.Servcies;

public interface IUserService
{
    public UserInfo? UserInfo { get; protected set; }
}
