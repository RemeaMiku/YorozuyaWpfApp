using EverythingHouse.WpfApp.Models;

namespace EverythingHouse.WpfApp.Servcies.Contracts;

public interface IUserService
{
    public UserInfo? UserInfo { get; protected set; }
}
