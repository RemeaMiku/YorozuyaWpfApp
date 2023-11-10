using System.Windows.Controls;
using EverythingHouse.WpfApp.ViewModels.Pages;

namespace EverythingHouse.WpfApp.Views.Pages;

/// <summary>
/// SettingsPage.xaml 的交互逻辑
/// </summary>
public partial class SettingsPage : Page
{
    public SettingsPage(SettingsPageViewModel viewModel)
    {
        InitializeComponent();
        DataContext = this;
        ViewModel = viewModel;
    }

    public SettingsPageViewModel ViewModel { get; }
}
