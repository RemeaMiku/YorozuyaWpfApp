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
using System.Configuration;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

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
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        ServiceProvider.GetRequiredService<PostWindow>();
        ApplyAppTheme(AppTheme);
        ApplyBackdropType(WindowBackdropType);
        ApplyAppFont(AppFont);
        mainWindow.Show();
    }

    private string? _appTheme;
    private BackgroundType? _windowBackgroundType;
    private string? _appFont;

    public string AppTheme
    {
        get
        {
            _appTheme ??= ReadAppThemeFromConfiguration();
            return _appTheme;
        }
    }

    public BackgroundType WindowBackdropType
    {
        get
        {
            _windowBackgroundType ??= ReadWindowBackdropTypeFromConfiguration();
            return _windowBackgroundType.Value;
        }
    }

    public string AppFont
    {
        get
        {
            _appFont ??= ReadAppFontFromConfiguration();
            return _appFont;
        }
    }

    public Configuration Configuration { get; } = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

    private BackgroundType ReadWindowBackdropTypeFromConfiguration()
    {
        var element = Configuration.AppSettings.Settings["Window Backdrop Type"];
        if (element is null)
            return BackgroundType.Acrylic;
        if (Enum.TryParse<BackgroundType>(element.Value, out var type))
            return type;
        return BackgroundType.Acrylic;
    }

    private string ReadAppThemeFromConfiguration()
    {
        var element = Configuration.AppSettings.Settings["App Theme"];
        if (element is null)
            return "System";
        if (element.Value == "System" || element.Value == "Light" || element.Value == "Dark")
            return element.Value;
        return "System";
    }

    private string ReadAppFontFromConfiguration()
    {
        var element = Configuration.AppSettings.Settings["App Font"];
        if (element is null)
            return "System";
        if (element.Value == "System" || element.Value == "Source Han Sans SC")
            return element.Value;
        return "System";
    }

    public void ApplyAppTheme(string theme)
    {
        switch (theme)
        {
            case "Light":
                Theme.Apply(ThemeType.Light, WindowBackdropType, true, true);
                break;
            case "Dark":
                Theme.Apply(ThemeType.Dark, WindowBackdropType, true, true);
                break;
            case "System":
                foreach (var window in Windows)
                    if (window is UiWindow uiWindow)
                        Watcher.Watch(uiWindow, WindowBackdropType, true, true);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public void WriteAppThemeToConfiguration(string theme)
    {
        _appTheme = theme;
        var element = Configuration.AppSettings.Settings["App Theme"];
        if (element is null)
            Configuration.AppSettings.Settings.Add("App Theme", theme);
        else
            element.Value = theme;
        Configuration.Save();
    }

    public void ApplyBackdropType(BackgroundType type)
    {
        foreach (var window in Windows)
            if (window is UiWindow uiWindow)
                uiWindow.WindowBackdropType = type;
    }

    public void WriteBackdropTypeToConfiguration(BackgroundType type)
    {
        _windowBackgroundType = type;
        var element = Configuration.AppSettings.Settings["Window Backdrop Type"];
        if (element is null)
            Configuration.AppSettings.Settings.Add("Window Backdrop Type", type.ToString());
        else
            element.Value = type.ToString();
        Configuration.Save();
    }

    public void ApplyAppFont(string font)
    {
        Resources["DefaultFontFamily"] = font switch
        {
            "System" => Resources["SystemFontFamily"],
            "Source Han Sans SC" => Resources["SourceHanSansSCFontFamily"],
            _ => throw new NotImplementedException(),
        };
    }

    public void WriteAppFontToConfiguration(string font)
    {
        _appFont = font;
        var element = Configuration.AppSettings.Settings["App Font"];
        if (element is null)
            Configuration.AppSettings.Settings.Add("App Font", font);
        else
            element.Value = font;
        Configuration.Save();
    }
}