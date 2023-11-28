// Author : RemeaMiku (Wuhan University) E-mail : remeamiku@whu.edu.cn
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Mvvm.Contracts;
using Yorozuya.WpfApp.Common;
using Yorozuya.WpfApp.Common.Exceptions;
using Yorozuya.WpfApp.Extensions;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.ViewModels.Windows;

public partial class LoginWindowViewModel : BaseValidatorViewModel
{

    public EventHandler? LoginRequested;

    public EventHandler? UserLoggedIn;

    public EventHandler<string>? NavigateRequsted;

    public LoginWindowViewModel(IUserService userService, [FromKeyedServices(nameof(LoginWindowViewModel))] ISnackbarService snackbarService, IMessenger messenger)
    {
        _userService = userService;
        _snackbarService = snackbarService;
        _messenger = messenger;
        messenger.Register<LoginWindowViewModel, string>(this, (viewModel, message) =>
        {
            if (message == StringMessages.RequestUserLogin)
                viewModel.ReplyLoginRequest();
        });
    }

    public static List<string> Fields => Common.Field.Fields.ToList();

    public string? DisplayGender => Gender switch
    {
        0 => "女",
        1 => "男",
        _ => default,
    };

    public string DisplayPassword => IsPasswordShown && Password is not null ? Password : "******";

    public bool IsUserNameAndPasswordValid => !GetErrors(nameof(UserName)).Any() && !GetErrors(nameof(Password)).Any();

    public bool IsFieldAndGenderValid => !GetErrors(nameof(Field)).Any() && !GetErrors(nameof(Gender)).Any();

    private void ReplyLoginRequest()
    {
        Field = default;
        Gender = default;
        ClearErrors();
        MoveToFieldGenderPanelCommand.NotifyCanExecuteChanged();
        LoginCommand.NotifyCanExecuteChanged();
        LoginRequested?.Invoke(this, EventArgs.Empty);
    }

    private readonly IUserService _userService;

    private readonly ISnackbarService _snackbarService;

    private readonly IMessenger _messenger;

    const string _defaultWindowTitle = "登录/注册";

    [ObservableProperty]
    string _windowTitle = _defaultWindowTitle;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToFieldGenderPanelCommand))]
    [Required(ErrorMessage = "不能为空")]
    [RegularExpression("^[a-zA-Z][a-zA-Z0-9_]{4,15}$", ErrorMessage = "字母开头，长度在5~16之间，仅允许字母、数字和下划线")]
    private string? _userName;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToFieldGenderPanelCommand))]
    [Required(ErrorMessage = "不能为空")]
    [RegularExpression("^[a-zA-Z]\\w{5,17}$", ErrorMessage = "字母开头，长度在6~18之间，仅允许字符、数字和下划线")]
    private string? _password;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(MoveToCheckInfomationPanelCommand))]
    [Required(ErrorMessage = "不能为空")]
    [Length(1, 20, ErrorMessage = "领域长度必须在1~20之间")]
    private string? _field;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(DisplayGender))]
    [NotifyCanExecuteChangedFor(nameof(MoveToCheckInfomationPanelCommand))]
    [Required(ErrorMessage = "不能为空")]
    [AllowedValues(0, 1)]
    private int? _gender;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayPassword))]
    private bool _isPasswordShown = false;

    [ObservableProperty]
    private string? _busyMessage;

    private void HandleExceptions(string title, Exception ex)
    {
        _snackbarService.ShowErrorMessageIfAny(title,
            (() => ex is ApiResponseException, ex.Message),
            (() => ex is HttpRequestException, $"请检查网络设置：{ex.Message}"),
            (() => true, $"出现了一些问题：{ex.Message}"));
    }

    [RelayCommand(CanExecute = nameof(IsUserNameAndPasswordValid))]
    private async Task LoginAsync()
    {
        ValidateProperty(UserName, nameof(UserName));
        ValidateProperty(Password, nameof(Password));
        if (!IsUserNameAndPasswordValid)
        {
            MoveToFieldGenderPanelCommand.NotifyCanExecuteChanged();
            LoginCommand.NotifyCanExecuteChanged();
            return;
        }
        try
        {
            IsBusy = true;
            BusyMessage = "正在尝试让你登录...";
            WindowTitle = "正在登录";
            NavigateRequsted?.Invoke(this, "TryLogin");
            await _userService.UserLoginAsync(UserName!, Password!);
            WindowTitle = _defaultWindowTitle;
            NavigateRequsted?.Invoke(this, "Successed");
            UserLoggedIn?.Invoke(this, EventArgs.Empty);
            _messenger.Send(StringMessages.UserLogined);
            UserName = default;
            Password = default;
            Field = default;
            Gender = default;
            ClearErrors();
            MoveToFieldGenderPanelCommand.NotifyCanExecuteChanged();
            LoginCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            HandleExceptions("登录失败", ex);
            NavigateRequsted?.Invoke(this, "NotLogined");
            WindowTitle = _defaultWindowTitle;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsUserNameAndPasswordValid))]
    private void MoveToFieldGenderPanel()
    {
        ValidateProperty(UserName, nameof(UserName));
        ValidateProperty(Password, nameof(Password));
        if (!IsUserNameAndPasswordValid)
        {
            MoveToFieldGenderPanelCommand.NotifyCanExecuteChanged();
            LoginCommand.NotifyCanExecuteChanged();
            return;
        }
        WindowTitle = "完善信息";
        NavigateRequsted?.Invoke(this, "Register");
    }

    [RelayCommand]
    private void MoveToUsernamePasswordPanel()
    {
        Field = default;
        Gender = default;
        ClearErrors();
        MoveToCheckInfomationPanelCommand.NotifyCanExecuteChanged();
        WindowTitle = _defaultWindowTitle;
        NavigateRequsted?.Invoke(this, "Back");
    }

    [RelayCommand(CanExecute = nameof(IsFieldAndGenderValid))]
    private void MoveToCheckInfomationPanel()
    {
        ValidateProperty(Field, nameof(Field));
        ValidateProperty(Gender, nameof(Gender));
        if (IsFieldAndGenderValid)
        {
            WindowTitle = "确认信息";
            NavigateRequsted?.Invoke(this, "Continue");
        }
        else
            MoveToCheckInfomationPanelCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void Cancel()
    {
        NavigateRequsted?.Invoke(this, "Cancel");
        WindowTitle = _defaultWindowTitle;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "正在尝试注册账号...";
            WindowTitle = "正在注册";
            NavigateRequsted?.Invoke(this, "TryRegister");
            await _userService.UserRegisterAsync(UserName!, Password!, Field!, (int)Gender!);
            NavigateRequsted?.Invoke(this, "Successed");
            _snackbarService.ShowSuccessMessage("注册成功", $"欢迎 {UserName} 加入万事屋！");
            Password = default;
            Field = default;
            Gender = default;
            ClearErrors();
            MoveToFieldGenderPanelCommand.NotifyCanExecuteChanged();
            LoginCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            HandleExceptions("注册失败", ex);
            WindowTitle = _defaultWindowTitle;
            NavigateRequsted?.Invoke(this, "NotRegistered");
        }
        finally
        {
            IsBusy = false;
        }
    }

}