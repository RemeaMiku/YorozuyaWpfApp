using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace EverythingHouse.WpfApp.ViewModels;

/// <summary>
/// 带有验证器的视图模型基类，继承自 <see cref="ObservableValidator"/>
/// </summary>
public partial class BaseValidatorViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool _isBusy = false;

    public bool IsNotBusy => !IsBusy;
}
