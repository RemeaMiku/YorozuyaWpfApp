// Author : RemeaMiku (Wuhan University) E-mail : remeamiku@whu.edu.cn
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;
using Yorozuya.WpfApp.Extensions;
using Yorozuya.WpfApp.ViewModels.Windows;

namespace Yorozuya.WpfApp.Views.Windows;

/// <summary>
/// LoginWindow.xaml 的交互逻辑
/// </summary>
public partial class LoginWindow : UiWindow
{
    #region Public Constructors

    public LoginWindow(LoginWindowViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        App.Current.ServiceProvider.GetRequiredKeyedService<ISnackbarService>(nameof(LoginWindowViewModel)).SetSnackbarControl(Snackbar);
        DataContext = this;
        ApplyBackground(App.Current.LoginBackgroundImage);
        Theme.Changed += OnThemeChanged;
        _navigateDictionary = new Dictionary<string, (bool IsFromLeft, FrameworkElement InElement, FrameworkElement OutElement)>()
        {
            { "Register", (false, FieldGenderPanel, UsernamePasswordPanel) },
            { "Back", (true, UsernamePasswordPanel, FieldGenderPanel) },
            { "Continue", (false, CheckInfomationPanel, FieldGenderPanel) },
            { "Cancel", (true, UsernamePasswordPanel, CheckInfomationPanel) },
            { "TryLogin",(false,BusyPanel,UsernamePasswordPanel) },
            { "TryRegister",(false,BusyPanel,CheckInfomationPanel) },
            { "Successed",(false,UsernamePasswordPanel,BusyPanel) },
            { "NotLogined",(true,UsernamePasswordPanel,BusyPanel) },
            { "NotRegistered",(true,UsernamePasswordPanel,BusyPanel)}
        }.ToFrozenDictionary();
        ViewModel.NavigateRequsted += async (_, e) =>
        {
            if (e == "Successed")
                PasswordBox.Password = string.Empty;
            await NavigateAsync(e);
        };
        ViewModel.LoginRequested += async (_, _) =>
        {
            Show();
            Focus();
            if (ViewModel.IsBusy)
                return;
            if (FieldGenderPanel.Visibility == Visibility.Visible)
                await NavigateAsync("Back");
            if (CheckInfomationPanel.Visibility == Visibility.Visible)
                await NavigateAsync("Cancel");
        };
        ViewModel.UserLoggedIn += (_, _) => { Hide(); };
    }

    #endregion Public Constructors

    #region Public Properties

    public LoginWindowViewModel ViewModel { get; }

    #endregion Public Properties

    #region Protected Methods

    protected override void OnClosing(CancelEventArgs e)
    {
        Hide();
        e.Cancel = true;
    }

    #endregion Protected Methods

    #region Private Fields

    private readonly FrozenDictionary<string, (bool IsFromLeft, FrameworkElement InElement, FrameworkElement OutElement)> _navigateDictionary;

    private bool _isNavigating = false;

    #endregion Private Fields

    #region Private Methods

    private async void OnThemeChanged(ThemeType currentTheme, Color systemAccent)
    {
        if (App.Current.LoginBackgroundImage == "Default")
            await ChangeBackgroundAsync(new BitmapImage(new($"/Assets/Images/DefaultLoginBackground-{currentTheme}.jpg", UriKind.Relative)));
    }

    private async void ApplyBackground(string loginBackgroundImage)
    {
        try
        {
            if (loginBackgroundImage == "Default")
                await ChangeBackgroundAsync(new BitmapImage(new($"/Assets/Images/DefaultLoginBackground-{Theme.GetAppTheme()}.jpg", UriKind.Relative)));
            else
                await ChangeBackgroundAsync(new BitmapImage(new(loginBackgroundImage)));
            App.Current.WriteLoginBackgroundImageToConfiguration(loginBackgroundImage);
        }
        catch (Exception)
        {
            Snackbar.Show("更换背景图片失败", "可能是图片格式不受支持", SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
        }
    }

    private void OnBackgroundSettingButtonClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择背景图片",
            RestoreDirectory = true,
            Multiselect = false,
            Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff;*.ico;*.svg;*.svgz;*.webp;*.jfif;*.pjpeg;*.pjp;*.avif;*.apng;*.jfif-tbnl;*.jpe;*.jfif-tbnl;*.jfi;*.cur;*.ani;*.ico;*.icns;*.xbm;*.dib;*.bmp;*.tif;*.tiff;*.gif;*.svg;*.svgz;*.webp;*.jfif;*.pjpeg;*.pjp;*.avif;*.apng;*.jfif-tbnl;*.jpe;*.jfif-tbnl;*.jfi;*.cur;*.ani;*.ico;*.icns;*.xbm;*.dib",
        };
        if ((bool)dialog.ShowDialog(this)!)
            ApplyBackground(dialog.FileName);
    }

    private void OnResetButtonClicked(object sender, RoutedEventArgs e) => ApplyBackground("Default");

    private async Task ChangeBackgroundAsync(ImageSource newImageSource)
    {
        BackgroundImage2.Source = BackgroundImage.Source;
        BackgroundImage.Source = newImageSource;
        var duration = TimeSpan.FromSeconds(0.5);
        BackgroundImage2.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, duration));
        BackgroundImage.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, duration));
        await Task.Delay(duration);
        BackgroundImage2.Source = default;
    }

    private async Task NavigateAsync(string args)
    {
        var duration = TimeSpan.FromSeconds(0.3);
        if (_isNavigating)
        {
            await Task.Delay(2 * duration);
            await NavigateAsync(args);
            return;
        }
        _isNavigating = true;
        (var isFromLeft, var inElement, var outElement) = _navigateDictionary[args];
        var ease = new QuarticEase { EasingMode = EasingMode.EaseInOut };
        var distance = 300;
        inElement.IsEnabled = false;
        outElement.IsEnabled = false;
        var inMargin = new Thickness(distance, 0, 0, 0);
        var outMargin = new Thickness(0, 0, distance, 0);
        if (isFromLeft)
            (inMargin, outMargin) = (outMargin, inMargin);
        await outElement.SlideAndFadeOutAsync(duration, outMargin, ease);
        outElement.Visibility = Visibility.Collapsed;
        inElement.Visibility = Visibility.Visible;
        await inElement.SlideAndFadeInAsync(duration, inMargin, ease);
        _isNavigating = false;
        outElement.Margin = new();
        outElement.Opacity = 1;
        inElement.IsEnabled = true;
        outElement.IsEnabled = true;
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        var passwordBox = (Wpf.Ui.Controls.PasswordBox)sender;
        ViewModel.Password = passwordBox.Password;
    }

    private void OnBackgroundMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Mouse.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    #endregion Private Methods   
}