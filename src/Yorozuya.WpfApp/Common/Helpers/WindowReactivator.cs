using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Yorozuya.WpfApp.Common.Helpers;

public class WindowReactivator
{
    #region Public Methods

    public static void Reactive(Window window)
    {
        if (window.WindowState == WindowState.Minimized)
            window.WindowState = WindowState.Normal;
        if (!window.IsVisible)
            window.Show();
        window.Activate();
    }

    #endregion Public Methods
}
