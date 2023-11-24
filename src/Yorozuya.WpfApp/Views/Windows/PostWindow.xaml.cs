// Author : RemeaMiku (Wuhan University) E-mail : remeamiku@whu.edu.cn
using System.Threading.Tasks;
using System.Windows;
using Yorozuya.WpfApp.ViewModels.Windows;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using System.ComponentModel;
using Wpf.Ui.Mvvm.Contracts;
using Yorozuya.WpfApp.Servcies.Contracts;
using Yorozuya.WpfApp.Common.Helpers;

namespace Yorozuya.WpfApp.Views.Windows;

/// <summary>
/// PostWindow.xaml 的交互逻辑
/// </summary>
public partial class PostWindow : UiWindow
{
    #region Public Constructors

    public PostWindow(PostWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = this;
        ViewModel = viewModel;
        App.Current.ServiceProvider.GetRequiredKeyedService<ISnackbarService>(nameof(PostWindowViewModel)).SetSnackbarControl(Snackbar);
        App.Current.ServiceProvider.GetRequiredKeyedService<ILeftRightButtonDialogService>(nameof(PostWindowViewModel)).SetDialogControl(Dialog);
        ViewModel.WindowOpened += (_, _) => { WindowReactivator.Reactive(this); };
    }

    #endregion Public Constructors

    #region Public Properties

    public PostWindowViewModel ViewModel { get; }

    #endregion Public Properties

    #region Protected Methods

    protected override void OnClosing(CancelEventArgs e)
    {
        Hide();
        e.Cancel = true;
    }

    #endregion Protected Methods

    #region Private Methods

    private void OnMainWindowButtonClicked(object sender, RoutedEventArgs e)
    {
        var mainWindow = App.Current.ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
        if (mainWindow.WindowState == WindowState.Minimized)
            mainWindow.WindowState = WindowState.Normal;
        mainWindow.Focus();
    }

    private async void OnCopyButtonClickedAsync(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(ViewModel.CurrentReply!.Content))
            return;
        Wpf.Ui.Common.Clipboard.SetText(ViewModel.CurrentReply.Content);
        CopyButton.Icon = SymbolRegular.Checkmark24;
        CopyButton.Appearance = ControlAppearance.Success;
        await Task.Delay(3000);
        CopyButton.Icon = SymbolRegular.Copy24;
        CopyButton.Appearance = ControlAppearance.Transparent;
    }

    #endregion Private Methods
}