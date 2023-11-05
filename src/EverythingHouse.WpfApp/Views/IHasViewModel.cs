using CommunityToolkit.Mvvm.ComponentModel;

namespace EverythingHouse.WpfApp.Views;

public interface IHasViewModel<TViewModel> where TViewModel : ObservableObject
{
    public TViewModel ViewModel { get; }
}
