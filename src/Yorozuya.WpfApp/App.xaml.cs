using System;
using System.Windows;
using Yorozuya.WpfApp.Servcies;
using Yorozuya.WpfApp.Servcies.Contracts;
using Yorozuya.WpfApp.Servcies.Local;
using Yorozuya.WpfApp.ViewModels.Pages;
using Yorozuya.WpfApp.ViewModels.Windows;
using Yorozuya.WpfApp.Views.Pages;
using Yorozuya.WpfApp.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace Yorozuya.WpfApp;

/// <summary>
/// App.xaml 的交互逻辑
/// </summary>
public partial class App : Application
{
    // 当前App实例
    public static new App Current => (App)Application.Current;

    // IoC容器
    public IServiceProvider ServiceProvider { get; } = new ServiceCollection()
        .AddSingleton<IMessenger>(WeakReferenceMessenger.Default)
        .AddTransient<ISnackbarService, SnackbarService>()
        .AddTransient<ILeftRightButtonDialogService, LeftRightButtonDialogService>()
        .AddSingleton<IUserService, LocalUserService>()
        .AddSingleton<IPostService, LocalPostService>()
        .AddSingleton<HomePageViewModel>()
        .AddSingleton<PersonPageViewModel>()
        .AddSingleton<SettingsPageViewModel>()
        .AddSingleton<PostWindowViewModel>()
        .AddSingleton<MainWindowViewModel>()
        .AddSingleton<HomePage>()
        .AddSingleton<PersonPage>()
        .AddSingleton<SettingsPage>()
        .AddSingleton<PostWindow>()
        .AddSingleton<MainWindow>()
        .BuildServiceProvider();

    // 重写启动方法
    protected override void OnStartup(StartupEventArgs e)
    {
        // 从容器中获取MainWindow并显示
        ServiceProvider.GetRequiredService<MainWindow>().Show();
        ServiceProvider.GetRequiredService<PostWindow>().Hide();
    }

}