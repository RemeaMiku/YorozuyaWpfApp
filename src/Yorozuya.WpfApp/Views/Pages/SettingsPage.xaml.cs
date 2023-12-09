// Author : RemeaMiku (Wuhan University) E-mail : remeamiku@whu.edu.cn
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Appearance;
using Yorozuya.WpfApp.Servcies.Contracts;
using Yorozuya.WpfApp.ViewModels.Pages;

namespace Yorozuya.WpfApp.Views.Pages;

/// <summary>
/// SettingsPage.xaml 的交互逻辑
/// </summary>
public partial class SettingsPage : Page
{
    #region Public Constructors

    public SettingsPage(SettingsPageViewModel viewModel)
    {
        InitializeComponent();
        DataContext = this;
        ViewModel = viewModel;
        Loaded += OnSettingsPageLoaded;
        App.Current.ServiceProvider.GetRequiredKeyedService<ILeftRightButtonDialogService>(nameof(SettingsPageViewModel)).SetDialogControl(Dialog);
    }

    #endregion Public Constructors

    #region Public Properties

    public SettingsPageViewModel ViewModel { get; }

    #endregion Public Properties

    #region Private Fields

    private BackgroundType _nowBackgroundType;

    #endregion Private Fields

    #region Private Methods

    private void OnSettingsPageLoaded(object sender, RoutedEventArgs e)
    {
        ThemeBox.SelectedValue = App.Current.AppTheme;
        FontFamilyBox.SelectedValue = App.Current.AppFont;
        WindowBackdropTypeBox.SelectedValue = _nowBackgroundType = App.Current.WindowBackdropType;
    }

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
        if (App.Current.ApplyBackdropType(type))
        {
            _nowBackgroundType = type;
            App.Current.WriteBackdropTypeToConfiguration(type);
        }
        else
        {
            WindowBackdropTypeBox.SelectedValue = _nowBackgroundType;
        }
    }

    #endregion Private Methods
}