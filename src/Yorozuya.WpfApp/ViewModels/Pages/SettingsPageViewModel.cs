// Author : RemeaMiku (Wuhan University) E-mail : remeamiku@whu.edu.cn
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Yorozuya.WpfApp.Common;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.ViewModels.Pages;

public partial class SettingsPageViewModel(IUserService userService, [FromKeyedServices(nameof(SettingsPageViewModel))] ILeftRightButtonDialogService dialogService, IMessenger messenger) : BaseViewModel
{
    #region Public Properties

    public UserInfo? UserInfo => _userService.UserInfo;

    #endregion Public Properties

    #region Private Fields

    private readonly IUserService _userService = userService;

    private readonly ILeftRightButtonDialogService _dialogService = dialogService;

    private readonly IMessenger _messenger = messenger;

    #endregion Private Fields

    #region Private Methods

    [RelayCommand]
    private void Login()
    {
        _messenger.Send(Messages.RequestUserLogin);
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