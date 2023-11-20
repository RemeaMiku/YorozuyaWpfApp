using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Appearance;
using Yorozuya.WpfApp.ViewModels.Pages;

namespace Yorozuya.WpfApp.Views.Pages;

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
        Loaded += OnSettingsPageLoaded;
        viewModel.GetDialogService().SetDialogControl(Dialog);
    }

    private void OnSettingsPageLoaded(object sender, RoutedEventArgs e)
    {
        ThemeBox.SelectedValue = App.Current.AppTheme;
        FontFamilyBox.SelectedValue = App.Current.AppFont;
        WindowBackdropTypeBox.SelectedValue = App.Current.WindowBackdropType;
    }

    public SettingsPageViewModel ViewModel { get; }

    private void OnThemeBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var theme = (string)ThemeBox.SelectedValue;
        App.Current.WriteAppThemeToConfiguration(theme);
    }

    private void OnFontFamilyBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var font = (string)FontFamilyBox.SelectedValue;
        App.Current.ApplyAppFont(font);
        App.Current.WriteAppFontToConfiguration(font);
    }

    private void OnWindowBackdropTypeBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var type = (BackgroundType)WindowBackdropTypeBox.SelectedValue;
        App.Current.ApplyBackdropType(type);
        App.Current.WriteBackdropTypeToConfiguration(type);
    }
}
