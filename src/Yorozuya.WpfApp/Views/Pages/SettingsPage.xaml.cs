using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Appearance;
using Yorozuya.WpfApp.ViewModels.Pages;
using Yorozuya.WpfApp.Views.Windows;

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
    }

    private void OnSettingsPageLoaded(object sender, RoutedEventArgs e)
    {
        ThemeBox.SelectedValue = App.Current.AppTheme;
        WindowBackdropTypeBox.SelectedValue = App.Current.WindowBackdropType;
    }

    public SettingsPageViewModel ViewModel { get; }

    private void OnWindowBackdropTypeBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var type = (BackgroundType)WindowBackdropTypeBox.SelectedValue;
        App.Current.ApplyBackdropType(type);
        App.Current.WriteBackdropTypeToConfiguration(type);
    }

    private void OnThemeBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var theme = (string)ThemeBox.SelectedValue;
        App.Current.WriteAppThemeToConfiguration(theme);
    }
}
