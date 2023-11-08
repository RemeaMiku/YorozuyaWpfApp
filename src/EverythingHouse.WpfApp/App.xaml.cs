using System;
using System.Windows;
using EverythingHouse.WpfApp.ViewModels.Pages;
using EverythingHouse.WpfApp.ViewModels.Windows;
using EverythingHouse.WpfApp.Views.Pages;
using EverythingHouse.WpfApp.Views.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace EverythingHouse.WpfApp;

/// <summary>
/// App.xaml 的交互逻辑
/// </summary>
public partial class App : Application
{
    // 当前App实例
    public static new App Current => (App)Application.Current;

    // IoC容器
    public IServiceProvider ServiceProvider { get; private set; } = new ServiceCollection()
        .AddSingleton<HomePageViewModel>()
        .AddSingleton<PersonPageViewModel>()
        .AddSingleton<SettingsPageViewModel>()
        .AddSingleton<QuestionWindowViewModel>()
        .AddSingleton<MainWindowViewModel>()
        .AddSingleton<HomePage>()
        .AddSingleton<PersonPage>()
        .AddSingleton<SettingsPage>()
        .AddSingleton<QuestionWindow>()
        .AddSingleton<MainWindow>()
        .BuildServiceProvider();

    // 重写启动方法
    protected override void OnStartup(StartupEventArgs e)
    {
        // 从容器中获取MainWindow并显示
        ServiceProvider.GetRequiredService<MainWindow>().Show();
        ServiceProvider.GetRequiredService<QuestionWindow>().Show();
    }

}