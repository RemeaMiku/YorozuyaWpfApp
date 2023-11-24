// Author : RemeaMiku (Wuhan University) E-mail : remeamiku@whu.edu.cn
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Yorozuya.WpfApp.Extensions;
using Yorozuya.WpfApp.ViewModels.Windows;
using Yorozuya.WpfApp.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;

namespace Yorozuya.WpfApp.Views.Windows;

/// <summary>
/// MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : UiWindow
{
    #region Public Constructors

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        InitializeNavigateDictionary();
        Loaded += OnMainWindowLoaded;
        Theme.Changed += OnThemeChanged;
        _currentNavigateButton = HomeButton;
        ViewModel = viewModel;
    }

    #endregion Public Constructors

    #region Public Properties

    public MainWindowViewModel ViewModel { get; }

    #endregion Public Properties

    /// <summary>
    /// 主题切换时手动更新导航按钮的颜色
    /// </summary>
    /// <param name="currentTheme"> </param>
    /// <param name="systemAccent"> </param>

    #region Private Fields

    private static readonly TimeSpan _navigateDuration = TimeSpan.FromSeconds(0.2);

    private readonly Dictionary<Wpf.Ui.Controls.Button, (int Index, Type PageType)> _navigateDictionary = [];

    private Wpf.Ui.Controls.Button _currentNavigateButton;

    private DateTime _lastNavigateTime = DateTime.MinValue;

    #endregion Private Fields

    #region Private Methods

    /// <summary>
    /// 设置当前导航按钮的颜色
    /// </summary>
    /// <param name="button"> </param>
    private static void SetNavigateButton(Wpf.Ui.Controls.Button button)
    {
        button.IconFilled = true;
        button.Background = button.MouseOverBackground;
        var easingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut };
        var foregroundAnimation = new ColorAnimation((Color)App.Current.Resources["MikuGreen"], 2 * _navigateDuration) { EasingFunction = easingFunction };
        var storyboard = new Storyboard();
        storyboard.Children.Add(foregroundAnimation);
        Storyboard.SetTarget(foregroundAnimation, button);
        Storyboard.SetTargetProperty(foregroundAnimation, new("Foreground.Color"));
        storyboard.Begin(button);
    }

    private void OnThemeChanged(ThemeType currentTheme, Color systemAccent)
    {
        ResetAllNavigateButtons();
        SetNavigateButton(_currentNavigateButton);
        if (currentTheme == ThemeType.Light)
        {
            ThemeIcon.Symbol = SymbolRegular.WeatherMoon24;
            ThemeText.Text = "切换至深色模式";
        }
        else
        {
            ThemeIcon.Symbol = SymbolRegular.WeatherSunny24;
            ThemeText.Text = "切换至浅色模式";
        }
    }

    /// <summary>
    /// 判断系统主题，如果是深色主题则自动切换为深色主题；否则使用浅色主题。设置导航按钮的颜色
    /// </summary>
    /// <param name="sender"> </param>
    /// <param name="e"> </param>

    private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
    {
        OnNavigateButtonClicked(HomeButton, e);
    }

    /// <summary>
    /// 导航按钮与索引和页面类型的对应关系。 索引用于判断导航按钮的上下顺序用于确定切换动画方向，页面类型用于从容器中获取页面实例
    /// </summary>
    /// <summary>
    /// 当前导航按钮
    /// </summary>
    private void InitializeNavigateDictionary()
    {
        _navigateDictionary.Add(HomeButton, (0, typeof(HomePage)));
        _navigateDictionary.Add(PersonButton, (1, typeof(PersonPage)));
        _navigateDictionary.Add(SettingsButton, (2, typeof(SettingsPage)));
    }

    /// <summary>
    /// 点击主题按钮时切换主题并更新按钮图标
    /// </summary>
    /// <param name="sender"> </param>
    /// <param name="e"> </param>

    private void OnThemeButtonClicked(object sender, RoutedEventArgs e)
    {
        if (Theme.GetAppTheme() == ThemeType.Light)
            Theme.Apply(ThemeType.Dark, WindowBackdropType, true, true);
        else
            Theme.Apply(ThemeType.Light, WindowBackdropType, true, true);
    }

    /// <summary>
    /// 页面切换动画的持续时间
    /// </summary>
    /// <summary>
    /// 上一次导航按钮点击的时间，用于防止快速点击导航按钮导致的动画异常
    /// </summary>
    /// <summary>
    /// 重新设置所有导航按钮的颜色
    /// </summary>

    private void ResetAllNavigateButtons()
    {
        foreach ((var button, _) in _navigateDictionary)
        {
            button.IconFilled = false;
            button.Background = new SolidColorBrush(Colors.Transparent);
            if (Theme.GetAppTheme() == ThemeType.Light)
                button.Foreground = new SolidColorBrush(Colors.Black);
            else
                button.Foreground = new SolidColorBrush(Colors.White);
        }
    }

    /// <summary>
    /// 播放页面切换动画
    /// </summary>
    /// <param name="button"> </param>
    /// <returns> </returns>
    private async Task DisplayPageSwitchAnimationAsync(Wpf.Ui.Controls.Button button)
    {
        var oldPage = (Page)Frame.Content;
        (var oldIndex, _) = _navigateDictionary[_currentNavigateButton];
        (var newIndex, var newPageType) = _navigateDictionary[button];
        var newPage = (Page)App.Current.ServiceProvider.GetRequiredService(newPageType);
        if (oldPage == newPage)
            return;
        var easingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut };
        var slideDistance = 500;
        var sign = Math.Sign(newIndex - oldIndex);
        if (oldPage is not null)
            await oldPage.SlideAndFadeOutAsync(_navigateDuration, new(0, 0, 0, sign * slideDistance), easingFunction);
        Frame.Content = newPage;
        _currentNavigateButton = button;
        await newPage.SlideAndFadeInAsync(_navigateDuration, new(0, 0, 0, -sign * slideDistance), easingFunction);
    }

    /// <summary>
    /// 点击导航按钮时切换页面并播放动画
    /// </summary>
    /// <param name="sender"> </param>
    /// <param name="e"> </param>
    private async void OnNavigateButtonClicked(object sender, RoutedEventArgs e)
    {
        var newButton = (Wpf.Ui.Controls.Button)sender;
        if ((DateTime.Now - _lastNavigateTime) < 2 * _navigateDuration)
            return;
        _lastNavigateTime = DateTime.Now;
        ResetAllNavigateButtons();
        SetNavigateButton(newButton);
        await DisplayPageSwitchAnimationAsync(newButton);
    }

    /// <summary>
    /// 点击导航面板的展开按钮时切换导航面板的宽度
    /// </summary>
    /// <param name="sender"> </param>
    /// <param name="e"> </param>

    private void OnExpandButtonClicked(object sender, RoutedEventArgs e)
    {
        var duration = TimeSpan.FromSeconds(0.4);
        var easingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut };
        if (MenuPanel.Width == MenuPanel.MinWidth)
            MenuPanel.BeginAnimation(WidthProperty, new DoubleAnimation(MenuPanel.MaxWidth, duration) { EasingFunction = easingFunction });
        else
            MenuPanel.BeginAnimation(WidthProperty, new DoubleAnimation(MenuPanel.MinWidth, duration) { EasingFunction = easingFunction });
    }

    #endregion Private Methods
}