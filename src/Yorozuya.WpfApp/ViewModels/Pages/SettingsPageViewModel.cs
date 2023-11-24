// Author : RemeaMiku (Wuhan University) E-mail : remeamiku@whu.edu.cn
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.ViewModels.Pages;

public partial class SettingsPageViewModel(IUserService userService, ILeftRightButtonDialogService dialogService) : BaseViewModel
{
    #region Public Properties

    public UserInfo? UserInfo => _userService.UserInfo;

    #endregion Public Properties

    #region Public Methods

    public ILeftRightButtonDialogService GetDialogService() => _dialogService;

    #endregion Public Methods

    #region Private Fields

    private readonly IUserService _userService = userService;

    private readonly ILeftRightButtonDialogService _dialogService = dialogService;

    #endregion Private Fields

    #region Private Methods

    [RelayCommand]
    private void Login()
    {
        //TODO: Login
    }

    [RelayCommand]
    private async Task Logout()
    {
        await _dialogService.ShowDialogAsync("确定退出当前账号吗？", "警告", "取消", "退出");
        if (_dialogService.GetIsRightButtonClicked())
        {
            _userService.UserLogout();
            OnPropertyChanged(nameof(UserInfo));
        }
    }

    #endregion Private Methods
}