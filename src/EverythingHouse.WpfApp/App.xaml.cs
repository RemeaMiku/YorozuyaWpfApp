using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace EverythingHouse.WpfApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // 当前App实例
    public static new App Current => (App)Application.Current;

    // IoC容器
    public IServiceProvider ServiceProvider { get; private set; } = new ServiceCollection()
        .AddSingleton<MainWindow>()
        .BuildServiceProvider();

    // 重写启动方法
    protected override void OnStartup(StartupEventArgs e)
    {
        // 从容器中获取MainWindow并显示
        ServiceProvider.GetRequiredService<MainWindow>().Show();
    }

}