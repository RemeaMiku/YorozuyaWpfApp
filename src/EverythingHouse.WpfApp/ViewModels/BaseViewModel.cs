using CommunityToolkit.Mvvm.ComponentModel;

namespace EverythingHouse.WpfApp.ViewModels;

/// <summary>
/// 默认视图模型基类，继承自  <see cref="ObservableObject"/> 。如果需要数据验证器请使用 <see cref="BaseValidatorViewModel"/>
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool _isBusy;

    bool IsNotBusy => !IsBusy;
}
