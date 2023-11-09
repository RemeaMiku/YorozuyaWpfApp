using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EverythingHouse.WpfApp.ViewModels.Windows;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;

namespace EverythingHouse.WpfApp.Views.Windows;

/// <summary>
/// QuestionWindow.xaml 的交互逻辑
/// </summary>
public partial class PostWindow : UiWindow
{
    public PostWindow(PostWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = this;
        ViewModel = viewModel;
    }

    public PostWindowViewModel ViewModel { get; }

    void OnMainWindowButtonClicked(object sender, RoutedEventArgs e)
    {
        var mainWindow = App.Current.ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
        if (mainWindow.WindowState == WindowState.Minimized)
            mainWindow.WindowState = WindowState.Normal;
        mainWindow.Focus();
    }

    async void OnCopyButtonClickedAsync(object sender, RoutedEventArgs e)
    {
        if (ViewModel.CurrentReply is null)
            return;
        System.Windows.Clipboard.SetDataObject(ViewModel.CurrentReply.Content);
        CopyButton.Icon = SymbolRegular.Checkmark24;
        CopyButton.Appearance = ControlAppearance.Success;
        await Task.Delay(3000);
        CopyButton.Icon = SymbolRegular.Copy24;
        CopyButton.Appearance = ControlAppearance.Transparent;
    }

}
