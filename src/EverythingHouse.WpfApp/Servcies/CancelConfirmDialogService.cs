using System;
using System.Threading.Tasks;
using EverythingHouse.WpfApp.Servcies.Contracts;
using Wpf.Ui.Controls.Interfaces;

namespace EverythingHouse.WpfApp.Servcies;

public class CancelConfirmDialogService : ICancelConfirmDialogService
{

    IDialogControl? _dialogControl;

    bool _isConfirmed;

    public async Task ShowDialogAsync(string message, string? title = null, string? leftButtonText = null, string? rightButtonText = null)
    {
        ArgumentNullException.ThrowIfNull(_dialogControl);
        _dialogControl.ButtonLeftName = leftButtonText ?? _dialogControl.ButtonLeftName;
        _dialogControl.ButtonRightName = rightButtonText ?? _dialogControl.ButtonRightName;
        await _dialogControl.ShowAndWaitAsync(title ?? string.Empty, message);
    }

    public void Initialize(IDialogControl dialogControl)
    {
        if (_dialogControl is not null)
            throw new InvalidOperationException("The service has already been initialized.");
        _dialogControl = dialogControl;
        _dialogControl.ButtonLeftClick += OnDialogControlLeftButtonClicked;
        _dialogControl.ButtonRightClick += OnDialogControlRightButtonClicked;
    }

    private void OnDialogControlRightButtonClicked(object sender, System.Windows.RoutedEventArgs e)
    {
        _isConfirmed = true;
        _dialogControl!.Hide();
    }

    private void OnDialogControlLeftButtonClicked(object sender, System.Windows.RoutedEventArgs e)
    {
        _isConfirmed = false;
        _dialogControl!.Hide();
    }

    public bool GetIsConfirmed() => _isConfirmed;

}
