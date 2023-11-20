using System;
using System.Threading.Tasks;
using Yorozuya.WpfApp.Servcies.Contracts;
using Wpf.Ui.Controls;
using System.Windows;

namespace Yorozuya.WpfApp.Servcies;

public class LeftRightButtonDialogService : ILeftRightButtonDialogService
{

    Dialog? _dialog;

    bool _isRightButtonClicked;

    public async Task ShowDialogAsync(string message, string? title = null, string? leftButtonText = null, string? rightButtonText = null)
    {
        ArgumentNullException.ThrowIfNull(_dialog);
        _dialog.ButtonLeftVisibility = leftButtonText is null ? Visibility.Collapsed : Visibility.Visible;
        _dialog.ButtonRightVisibility = rightButtonText is null ? Visibility.Collapsed : Visibility.Visible;
        _dialog.ButtonLeftName = leftButtonText ?? string.Empty;
        _dialog.ButtonRightName = rightButtonText ?? string.Empty;
        await _dialog.ShowAndWaitAsync(title ?? string.Empty, message);
    }

    public void SetDialogControl(Dialog dialog)
    {
        if (_dialog is not null)
            throw new InvalidOperationException("The service has already been initialized.");
        _dialog = dialog;
        _dialog.ButtonLeftClick += OnDialogControlLeftButtonClicked;
        _dialog.ButtonRightClick += OnDialogControlRightButtonClicked;
    }

    private void OnDialogControlRightButtonClicked(object sender, RoutedEventArgs e)
    {
        _isRightButtonClicked = true;
        _dialog!.Hide();
    }

    private void OnDialogControlLeftButtonClicked(object sender, RoutedEventArgs e)
    {
        _isRightButtonClicked = false;
        _dialog!.Hide();
    }

    public bool GetIsRightButtonClicked() => _isRightButtonClicked;

}
