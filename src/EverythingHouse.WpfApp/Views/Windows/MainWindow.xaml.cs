using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using EverythingHouse.WpfApp.Extensions;
using EverythingHouse.WpfApp.ViewModels.Windows;
using EverythingHouse.WpfApp.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Markup;
using Wpf.Ui.Mvvm.Interfaces;

namespace EverythingHouse.WpfApp.Views.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : UiWindow, IHasViewModel<MainWindowViewModel>
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        InitializeNavigateDictionary();
        Loaded += OnMainWindowLoaded;
        Theme.Changed += OnThemeChanged;
        _currentNavigateButton = HomeButton;
        ViewModel = viewModel;
    }

    public MainWindowViewModel ViewModel { get; }

    void OnThemeChanged(ThemeType currentTheme, Color systemAccent)
    {
        ResetAllNavigateButtons();
        SetNavigateButton(_currentNavigateButton);
    }

    void OnMainWindowLoaded(object sender, RoutedEventArgs e)
    {
        if (Theme.GetSystemTheme() == SystemThemeType.Dark)
            OnThemeButtonClicked(sender, e);
        else
            Theme.Apply(ThemeType.Light, WindowBackdropType, true, true);
        OnNavigateButtonClicked(HomeButton, e);
    }

    readonly Dictionary<Wpf.Ui.Controls.Button, (int Index, Type PageType)> _navigateDictionary = new();

    Wpf.Ui.Controls.Button _currentNavigateButton;

    void InitializeNavigateDictionary()
    {
        _navigateDictionary.Add(HomeButton, (0, typeof(HomePage)));
        _navigateDictionary.Add(PersonButton, (1, typeof(PersonPage)));
        _navigateDictionary.Add(SettingsButton, (2, typeof(SettingsPage)));
    }

    void OnThemeButtonClicked(object sender, RoutedEventArgs e)
    {
        if (Theme.GetAppTheme() == ThemeType.Light)
        {
            Theme.Apply(ThemeType.Dark, WindowBackdropType, true, true);
            ThemeIcon.Symbol = SymbolRegular.WeatherSunny24;
        }
        else
        {
            Theme.Apply(ThemeType.Light, WindowBackdropType, true, true);
            ThemeIcon.Symbol = SymbolRegular.WeatherMoon24;
        }
    }

    static readonly TimeSpan _navigateDuration = TimeSpan.FromSeconds(0.2);

    DateTime _lastNavigateTime = DateTime.MinValue;

    void ResetAllNavigateButtons()
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

    static void SetNavigateButton(Wpf.Ui.Controls.Button button)
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

    async Task DisplayPageSwitchAnimationAsync(Wpf.Ui.Controls.Button button)
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

    async void OnNavigateButtonClicked(object sender, RoutedEventArgs e)
    {
        var newButton = (Wpf.Ui.Controls.Button)sender;
        if ((DateTime.Now - _lastNavigateTime) < 2 * _navigateDuration)
            return;
        _lastNavigateTime = DateTime.Now;
        ResetAllNavigateButtons();
        SetNavigateButton(newButton);
        await DisplayPageSwitchAnimationAsync(newButton);
    }

    void OnExpandButtonClicked(object sender, RoutedEventArgs e)
    {
        var duration = TimeSpan.FromSeconds(0.4);
        var easingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut };
        if (MenuPanel.Width == MenuPanel.MinWidth)
            MenuPanel.BeginAnimation(WidthProperty, new DoubleAnimation(MenuPanel.MaxWidth, duration) { EasingFunction = easingFunction });
        else
            MenuPanel.BeginAnimation(WidthProperty, new DoubleAnimation(MenuPanel.MinWidth, duration) { EasingFunction = easingFunction });
    }

    protected override void OnClosed(EventArgs e)
    {
        App.Current.Shutdown();
    }
}
