using Yorozuya.WpfApp.Models;

namespace Yorozuya.WpfApp.Servcies.Contracts;

public interface IUserService
{
    public UserInfo? UserInfo { get; protected set; }
}
