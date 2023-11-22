using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Yorozuya.WpfApp.ViewModels;

public abstract partial class BaseRecipientViewModel : ObservableRecipient
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool _isBusy = false;

    public bool IsNotBusy => !IsBusy;

    public BaseRecipientViewModel() : base()
    {

    }

    public BaseRecipientViewModel(IMessenger messenger) : base(messenger)
    {

    }
}