using System;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;
using Yorozuya.WpfApp.Extensions;

namespace Yorozuya.WpfApp.Views.Windows;

/// <summary>
/// LoginWindow.xaml 的交互逻辑
/// </summary>
public partial class LoginWindow : UiWindow
{
    readonly ISnackbarService _snackbarService;

    public LoginWindow(ISnackbarService snackbarService)
    {
        InitializeComponent();
        _snackbarService = snackbarService;
        snackbarService.SetSnackbarControl(Snackbar);
        DataContext = this;
        ApplyBackground(App.Current.LoginBackgroundImage);
        Theme.Changed += OnThemeChanged;
    }

    private void OnThemeChanged(ThemeType currentTheme, Color systemAccent)
    {
        if (App.Current.LoginBackgroundImage == "Default")
        {
            BackgroundImage.Source = new BitmapImage(new($"/Assets/Images/DefaultLoginBackground-{currentTheme}.jpg", UriKind.Relative));
        }
    }

    private void ApplyBackground(string loginBackgroundImage)
    {
        try
        {
            if (loginBackgroundImage == "Default")
                BackgroundImage.Source = new BitmapImage(new($"/Assets/Images/DefaultLoginBackground-{Theme.GetAppTheme()}.jpg", UriKind.Relative));
            else
                BackgroundImage.Source = new BitmapImage(new(loginBackgroundImage));
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

}
