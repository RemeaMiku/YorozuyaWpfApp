// Author : RemeaMiku (Wuhan University) E-mail : remeamiku@whu.edu.cn
using System;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Yorozuya.WpfApp.Common;
using Yorozuya.WpfApp.Common.Exceptions;
using Yorozuya.WpfApp.Models;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.ViewModels.Pages;

public partial class SettingsPageViewModel : BaseViewModel
{
    #region Public Constructors

    public SettingsPageViewModel(IUserService userService, [FromKeyedServices(nameof(SettingsPageViewModel))] ILeftRightButtonDialogService dialogService, IMessenger messenger)
    {
        _userService = userService;
        _dialogService = dialogService;
        _messenger = messenger;
        messenger.Register<SettingsPageViewModel, string>(this, (viewModel, message) =>
        {
            if (message == StringMessages.UserLogined)
                viewModel.ReplyUserLoggedIn();
        });
    }

    #endregion Public Constructors

    #region Private Fields

    private readonly IUserService _userService;

    private readonly ILeftRightButtonDialogService _dialogService;

    private readonly IMessenger _messenger;

    [ObservableProperty]
    private UserInfo? _userInfo;

    #endregion Private Fields

    #region Private Methods

    private void ReplyUserLoggedIn()
    {
        UserInfo = _userService.UserInfo;
    }

    [RelayCommand]
    private void Login()
    {
        _messenger.Send(StringMessages.RequestUserLogin);
    }

    [RelayCommand]
    private async Task Logout()
    {
        await _dialogService.ShowDialogAsync("确定退出当前账号吗？", "警告", "取消", "退出");
        if (!_dialogService.GetIsRightButtonClicked())
            return;
        _userService.UserLogout();
        _messenger.Send(StringMessages.UserLogouted);
        _messenger.Send(StringMessages.RequestUserLogin);
    }

    #endregion Private Methods
}