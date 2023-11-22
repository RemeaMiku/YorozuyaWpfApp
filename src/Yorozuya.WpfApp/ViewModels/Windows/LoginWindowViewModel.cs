using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Yorozuya.WpfApp.ViewModels.Windows;

public partial class LoginWindowViewModel : BaseValidatorViewModel
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MinLength(1, ErrorMessage = "用户名长度不得小于1")]
    [MaxLength(10, ErrorMessage = "用户名长度不得大于10")]
    string _userName = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MinLength(8, ErrorMessage = "用户名长度不得小于8")]
    [MaxLength(16, ErrorMessage = "用户名长度不得大于16")]
    string _password = string.Empty;
}
