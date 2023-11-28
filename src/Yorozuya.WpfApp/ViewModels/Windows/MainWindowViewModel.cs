using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Yorozuya.WpfApp.Servcies.Contracts;

namespace Yorozuya.WpfApp.ViewModels.Windows;

public partial class MainWindowViewModel([FromKeyedServices(nameof(MainWindowViewModel))] ILeftRightButtonDialogService dialogService) : BaseViewModel
{
    readonly ILeftRightButtonDialogService _dialogService = dialogService;
    [ObservableProperty]
    bool _doNotShowExitDialog = false;

    [ObservableProperty]
    bool _confirmCloseWindow = true;

    public EventHandler? CloseWindowRequested;

    public EventHandler? HideWindowRequested;

    [RelayCommand]
    async Task Close()
    {
        if (!DoNotShowExitDialog)
        {
            await _dialogService.ShowDialogAsync(string.Empty, default, "取消", "确认");
            if (!_dialogService.GetIsRightButtonClicked())
                return;
        }
        if (ConfirmCloseWindow)
            CloseWindowRequested?.Invoke(this, EventArgs.Empty);
        else
            HideWindowRequested?.Invoke(this, EventArgs.Empty);
    }
}