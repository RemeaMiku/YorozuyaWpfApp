using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Contracts;

namespace Yorozuya.WpfApp.Extensions;

public static class ISnackbarServiceExtensions
{
    public static bool ShowErrorMessage(this ISnackbarService snackbarService, string title, string message)
        => snackbarService.Show(title, message, SymbolRegular.Dismiss24, ControlAppearance.Danger);

    /// <summary>
    /// 显示错误消息
    /// </summary>
    /// <param name="snackbarService"></param>
    /// <param name="condition"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static bool ShowErrorMessageIf(this ISnackbarService snackbarService, string title, Func<bool> condition, string message)
    {
        if (condition.Invoke())
            return snackbarService.Show(title, message, SymbolRegular.Dismiss24, ControlAppearance.Danger);
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
