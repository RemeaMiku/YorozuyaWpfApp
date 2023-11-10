using System.Windows.Controls;
using EverythingHouse.WpfApp.ViewModels.Pages;

namespace EverythingHouse.WpfApp.Views.Pages;

/// <summary>
/// HomePage.xaml 的交互逻辑
/// </summary>
public partial class PersonPage : Page
{
    public PersonPage(PersonPageViewModel viewModel)
    {
        InitializeComponent();
        DataContext = this;
        ViewModel = viewModel;
    }

    public PersonPageViewModel ViewModel { get; }
}
