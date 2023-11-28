using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using CommunityToolkit.Mvvm.Messaging;
using Yorozuya.WpfApp.Extensions;
using Yorozuya.WpfApp.ViewModels.Pages;

namespace Yorozuya.WpfApp.Views.Pages;

/// <summary>
/// HomePage.xaml 的交互逻辑
/// </summary>
public partial class HomePage : Page
{
    public HomePage(HomePageViewModel viewModel, IMessenger messenger)
    {
        InitializeComponent();
        DataContext = this;
        ViewModel = viewModel;

        messenger.Register<HomePage, string, string>(this, nameof(HomePage),  async (homePage, message) =>
        {
            var duration = TimeSpan.FromMilliseconds(500);
            var marginTop = new Thickness(0, 500, 0, 0);
            var marginLeft = new Thickness(800, 0, 0, 0);
            var easing = new QuadraticEase { EasingMode = EasingMode.EaseInOut };
            switch (message)
            {
                case HomePageViewModel.UiStatus.StartSearch:
                    if(homePage.SearchResultGrid.Visibility == Visibility.Visible) break;
                    homePage.SearchResultGrid.Visibility = Visibility.Visible;
                    await homePage.SearchResultGrid.SlideAndFadeInAsync(duration, marginTop, easing);
                    break;
                case HomePageViewModel.UiStatus.EndSearch:
                    if (homePage.SearchResultGrid.Visibility == Visibility.Collapsed) break;
                    await homePage.SearchResultGrid.SlideAndFadeOutAsync(duration, marginTop, easing);
                    homePage.SearchResultGrid.Visibility = Visibility.Collapsed;
                    break;
                case HomePageViewModel.UiStatus.StartPost:
                    if (homePage.AddNewPostGrid.Visibility == Visibility.Visible) break;
                    homePage.AddNewPostGrid.Visibility = Visibility.Visible;
                    await homePage.AddNewPostGrid.SlideAndFadeInAsync(duration, marginLeft, easing);
                    break;
                case HomePageViewModel.UiStatus.EndPost:
                    if (homePage.AddNewPostGrid.Visibility == Visibility.Collapsed) break;
                    await homePage.AddNewPostGrid.SlideAndFadeOutAsync(duration, marginLeft, easing);
                    homePage.AddNewPostGrid.Visibility = Visibility.Collapsed;
                    break;
            }
        });
    }

    public HomePageViewModel ViewModel { get; }
}
