using CommunityToolkit.Mvvm.ComponentModel;
using EverythingHouse.WpfApp.ViewModels;

namespace EverythingHouse.WpfApp.Views;

public interface IHasViewModel<TViewModel> where TViewModel : ObservableObject
{
    public TViewModel ViewModel { get; }
}
