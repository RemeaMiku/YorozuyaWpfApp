using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using EverythingHouse.WpfApp.Extensions;
using EverythingHouse.WpfApp.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Interfaces;

namespace EverythingHouse.WpfApp.Views.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : UiWindow
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeNavigateDictionary();
        Loaded += OnMainWindowLoaded;
    }

    void OnMainWindowLoaded(object sender, RoutedEventArgs e)
    {
        OnNavigateButtonClicked(HomeButton, e);
        if (!Theme.IsAppMatchesSystem())
            OnThemeButtonClicked(sender, e);
    }

    readonly Dictionary<Wpf.Ui.Controls.Button, (int Index, Type PageType)> _navigateDictionary = new();

    int _currentIndex = 0;

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

    async void OnNavigateButtonClicked(object sender, RoutedEventArgs e)
    {
        foreach ((var button, _) in _navigateDictionary)
        {
            button.IconFilled = false;
            button.Background = new SolidColorBrush(Colors.Transparent);
        }
        var currentButton = (Wpf.Ui.Controls.Button)sender;
        currentButton.Background = currentButton.MouseOverBackground;
        currentButton.IconFilled = true;
        var oldPage = (Page)Frame.Content;
        var oldIndex = _currentIndex;
        (var newIndex, var newPageType) = _navigateDictionary[currentButton];
        var newPage = (Page)App.Current.ServiceProvider.GetRequiredService(newPageType);
        if (oldPage == newPage)
            return;
        var duration = TimeSpan.FromSeconds(0.2);
        var easingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut };
        var slideDistance = 500;
        var sign = Math.Sign(newIndex - oldIndex);
        if (oldPage is not null)
            await oldPage.SlideAndFadeOutAsync(duration, new(0, 0, 0, sign * slideDistance), easingFunction);
        Frame.Content = newPage;
        _currentIndex = newIndex;
        await newPage.SlideAndFadeInAsync(duration, new(0, 0, 0, -sign * slideDistance), easingFunction);
    }

    void OnExpandButtonClicked(object sender, RoutedEventArgs e)
    {
        var duration = TimeSpan.FromSeconds(0.2);
        var easingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut };
        if (MenuPanel.Width == MenuPanel.MinWidth)
            MenuPanel.BeginAnimation(WidthProperty, new DoubleAnimation(MenuPanel.MaxWidth, duration) { EasingFunction = easingFunction });
        else
            MenuPanel.BeginAnimation(WidthProperty, new DoubleAnimation(MenuPanel.MinWidth, duration) { EasingFunction = easingFunction });
    }
}
