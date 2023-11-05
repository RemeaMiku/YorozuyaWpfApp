using System;
using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Controls;

namespace EverythingHouse.WpfApp.Common.Converters;

public class BooleanToInfoBarServerityConverter : IValueConverter
{
    #region Public Methods

    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isReady)
            return isReady ? InfoBarSeverity.Success : InfoBarSeverity.Informational;
        throw new ArgumentException($"{nameof(value)} is not a valid Boolean value.", nameof(value));
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is InfoBarSeverity severity)
            return severity == InfoBarSeverity.Success;
        throw new ArgumentException($"{nameof(value)} is not a valid Visibility value.", nameof(value));
    }

    #endregion Public Methods
}
