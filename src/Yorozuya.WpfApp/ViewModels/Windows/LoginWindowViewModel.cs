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
    [Length(1, 20)]
    [ObservableProperty]
    public string _userName = string.Empty;

    [Length(8, 16)]
    [ObservableProperty]
    public string _password = string.Empty;
}
