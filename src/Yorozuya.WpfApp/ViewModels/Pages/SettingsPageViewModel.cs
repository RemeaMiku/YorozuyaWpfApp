using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.ViewModels.Pages;

public partial class SettingsPageViewModel(IUserService userService, ILeftRightButtonDialogService dialogService) : BaseViewModel
{
    readonly IUserService _userService = userService;
    readonly ILeftRightButtonDialogService _dialogService = dialogService;

    public UserInfo? UserInfo => _userService.UserInfo;

    [RelayCommand]
    void Login()
    {

    }

    [RelayCommand]
    async Task Logout()
    {
        await _dialogService.ShowDialogAsync("确定退出当前账号吗？", "警告", "取消", "退出");
        if (_dialogService.GetIsRightButtonClicked())
        {
            _userService.UserLogout();
            OnPropertyChanged(nameof(UserInfo));
        }
    }

    public ILeftRightButtonDialogService GetDialogService() => _dialogService;
}
