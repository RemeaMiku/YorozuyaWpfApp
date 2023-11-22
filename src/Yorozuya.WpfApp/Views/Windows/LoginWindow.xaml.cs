using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using Wpf.Ui.Appearance;
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
    readonly ISnackbarService _snackbarService;

    public LoginWindow(LoginWindowViewModel viewModel, ISnackbarService snackbarService)
    {
        InitializeComponent();
        _snackbarService = snackbarService;
        ViewModel = viewModel;
        snackbarService.SetSnackbarControl(Snackbar);
        DataContext = this;
        ApplyBackground(App.Current.LoginBackgroundImage);
        Theme.Changed += OnThemeChanged;
        _navigateDictionary = new Dictionary<object, (bool IsFromLeft, FrameworkElement InElement, FrameworkElement OutElement)>()
        {
            { RegisterButton, (false, FieldGenderPanel, UsernamePasswordPanel) },
            { BackButton, (true, UsernamePasswordPanel, FieldGenderPanel) },
            { ContinueButton, (false, CheckInfomationPanel, FieldGenderPanel) },
            { CancelButton, (true, UsernamePasswordPanel, CheckInfomationPanel) },
        }.ToFrozenDictionary();
    }

    public LoginWindowViewModel ViewModel { get; }

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
            _snackbarService.ShowErrorMessage("背景图片加载失败", "可能是不支持此格式，请重新选择");
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        Hide();
        e.Cancel = true;
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

    bool _isSwitching = false;

    private readonly FrozenDictionary<object, (bool IsFromLeft, FrameworkElement InElement, FrameworkElement OutElement)> _navigateDictionary;

    private async void OnNavigateButtonClicked(object sender, RoutedEventArgs e)
    {
        if (_isSwitching)
            return;
        _isSwitching = true;
        var duration = TimeSpan.FromSeconds(0.5);
        var ease = new QuarticEase { EasingMode = EasingMode.EaseInOut };
        var distance = 300;
        (var isFromLeft, var inElement, var outElement) = _navigateDictionary[sender];
        var inMargin = new Thickness(distance, 0, 0, 0);
        var outMargin = new Thickness(0, 0, distance, 0);
        if (isFromLeft)
            (inMargin, outMargin) = (outMargin, inMargin);
        await outElement.SlideAndFadeOutAsync(duration, outMargin, ease);
        inElement.Visibility = Visibility.Visible;
        await inElement.SlideAndFadeInAsync(duration, inMargin, ease);
        outElement.Visibility = Visibility.Collapsed;
        _isSwitching = false;
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        var passwordBox = (Wpf.Ui.Controls.PasswordBox)sender;
        ViewModel.Password = passwordBox.Password;
    }
}
