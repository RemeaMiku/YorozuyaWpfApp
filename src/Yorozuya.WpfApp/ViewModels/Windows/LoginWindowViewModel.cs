using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui.Mvvm.Contracts;
using Yorozuya.WpfApp.Common;
using Yorozuya.WpfApp.Extensions;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.ViewModels.Windows;

public partial class LoginWindowViewModel : BaseValidatorViewModel
{
    readonly IUserService _userService;
    readonly ISnackbarService _snackbarService;
    readonly IMessenger _messenger;

    public ISnackbarService SnackbarService => _snackbarService;

    public LoginWindowViewModel(IUserService userService, ISnackbarService snackbarService, IMessenger messenger)
    {
        _userService = userService;
        _snackbarService = snackbarService;
        _messenger = messenger;
        messenger.Register<LoginWindowViewModel, string>(this, (viewModel, token) =>
        {
            if (token == MessageTokens.RequestUserLogin)
                viewModel.ReplyLoginRequest();
        });
    }

    public void ReplyLoginRequest()
    {
        UserName = default;
        Password = default;
        Field = default;
        Gender = default;
        ClearErrors();
        MoveToFieldGenderPanelCommand.NotifyCanExecuteChanged();
        LoginCommand.NotifyCanExecuteChanged();
        LoginRequested?.Invoke(this, EventArgs.Empty);
    }

    public EventHandler? LoginRequested;

    public EventHandler? UserLoggedIn;

    public EventHandler<string>? NavigateRequsted;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToFieldGenderPanelCommand))]
    [Required(ErrorMessage = "用户名不能为空")]
    [MinLength(1, ErrorMessage = "用户名长度不得小于1")]
    [MaxLength(10, ErrorMessage = "用户名长度不得大于10")]
    string? _userName;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveToFieldGenderPanelCommand))]
    [Required(ErrorMessage = "密码不能为空")]
    [MinLength(8, ErrorMessage = "密码长度不得小于8")]
    [MaxLength(16, ErrorMessage = "密码长度不得大于16")]
    string? _password;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(MoveToCheckInfomationPanelCommand))]
    [Required(ErrorMessage = "领域不能为空")]
    string? _field;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(DisplayGender))]
    [NotifyCanExecuteChangedFor(nameof(MoveToCheckInfomationPanelCommand))]
    [Required(ErrorMessage = "性别不能为空")]
    int? _gender;

    public string? DisplayGender => Gender switch
    {
        0 => "女",
        1 => "男",
        _ => default,
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayPassword))]
    bool _isPasswordShown = false;

    public string DisplayPassword => IsPasswordShown && Password is not null ? Password : "******";

    [ObservableProperty]
    string? _busyMessage;

    public bool IsUserNameAndPasswordValid => !GetErrors(nameof(UserName)).Any() && !GetErrors(nameof(Password)).Any();

    public bool IsFieldAndGenderValid => !GetErrors(nameof(Field)).Any() && !GetErrors(nameof(Gender)).Any();

    [RelayCommand(CanExecute = nameof(IsUserNameAndPasswordValid))]
    async Task LoginAsync()
    {
        ValidateProperty(UserName, nameof(UserName));
        ValidateProperty(Password, nameof(Password));
        if (!IsUserNameAndPasswordValid)
        {
            LoginCommand.NotifyCanExecuteChanged();
            return;
        }
        try
        {
            IsBusy = true;
            BusyMessage = "正在尝试让你登录...";
            NavigateRequsted?.Invoke(this, "TryLogin");
            _messenger.Send(await _userService.UserLoginAsync(UserName!, Password!));
            NavigateRequsted?.Invoke(this, "Successed");
            UserLoggedIn?.Invoke(this, EventArgs.Empty);
            _messenger.Send(MessageTokens.UserLoggedIn);
            UserName = default;
            Password = default;
            Field = default;
            Gender = default;
            ClearErrors();
            MoveToFieldGenderPanelCommand.NotifyCanExecuteChanged();
            LoginCommand.NotifyCanExecuteChanged();
        }
        catch (Exception)
        {
            _snackbarService.ShowErrorMessage("登录失败", "请检查用户名和密码是否正确");
            NavigateRequsted?.Invoke(this, "NotLogined");
        }
        finally
        {
            IsBusy = false;
        }
    }


    [RelayCommand(CanExecute = nameof(IsUserNameAndPasswordValid))]
    void MoveToFieldGenderPanel()
    {
        ValidateProperty(UserName, nameof(UserName));
        ValidateProperty(Password, nameof(Password));
        if (IsUserNameAndPasswordValid)
            NavigateRequsted?.Invoke(this, "Register");
        else
            MoveToFieldGenderPanelCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    void MoveToUsernamePasswordPanel()
    {
        Field = default;
        Gender = default;
        ClearErrors();
        MoveToCheckInfomationPanelCommand.NotifyCanExecuteChanged();
        NavigateRequsted?.Invoke(this, "Back");
    }

    [RelayCommand(CanExecute = nameof(IsFieldAndGenderValid))]
    void MoveToCheckInfomationPanel()
    {
        ValidateProperty(Field, nameof(Field));
        ValidateProperty(Gender, nameof(Gender));
        if (IsFieldAndGenderValid)
            NavigateRequsted?.Invoke(this, "Continue");
        else
            MoveToCheckInfomationPanelCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    void Cancel()
    {
        NavigateRequsted?.Invoke(this, "Cancel");
    }

    [RelayCommand]
    async Task RegisterAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "正在尝试注册账号...";
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
        catch (Exception)
        {
            _snackbarService.ShowErrorMessage("注册失败", "用户名已被注册");
            NavigateRequsted?.Invoke(this, "NotRegistered");
        }
        finally
        {
            IsBusy = false;
        }
    }

    //TODO:发送登录消息

    //TODO:接收登录请求
}
