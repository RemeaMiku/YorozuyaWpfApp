using System;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Contracts;

namespace Yorozuya.WpfApp.Extensions;

public static class ISnackbarServiceExtensions
{
    public static bool ShowErrorMessage(this ISnackbarService snackbarService, string title, string message)
        => snackbarService.Show(title, message, SymbolRegular.ErrorCircle24, ControlAppearance.Danger);

    public static bool ShowSuccessMessage(this ISnackbarService snackbarService, string title, string message)
        => snackbarService.Show(title, message, SymbolRegular.CheckmarkCircle24, ControlAppearance.Success);

    public static bool ShowErrorMessageIf(this ISnackbarService snackbarService, string title, Func<bool> condition, string message)
    {
        if (condition.Invoke())
            return snackbarService.Show(title, message, SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
        return false;
    }

    public static bool ShowSuccessMessageIf(this ISnackbarService snackbarService, string title, Func<bool> condition, string message)
    {
        if (condition.Invoke())
            return snackbarService.Show(title, message, SymbolRegular.CheckmarkCircle24, ControlAppearance.Success);
        return false;
    }

    public static bool ShowErrorMessageIfAny(this ISnackbarService snackbarService, string title, params (Func<bool> Condition, string Message)[] conditionMessages)
    {
        foreach ((var condition, var message) in conditionMessages)
            if (ShowErrorMessageIf(snackbarService, title, condition, message))
                return true;
        return false;
    }
}
