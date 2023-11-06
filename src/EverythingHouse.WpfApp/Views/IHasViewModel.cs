using EverythingHouse.WpfApp.ViewModels;

namespace EverythingHouse.WpfApp.Views;

public interface IHasViewModel<TViewModel> where TViewModel : BaseViewModel
{
    public TViewModel ViewModel { get; }
}
