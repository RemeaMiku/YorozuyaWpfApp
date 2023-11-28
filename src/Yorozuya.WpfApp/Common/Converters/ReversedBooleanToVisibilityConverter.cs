using System;
using System.Globalization;
using System.Windows.Data;

namespace Yorozuya.WpfApp.Common.Converters;

/// <summary>
/// 反转的布尔值转换为可见性
/// 如果布尔值为真，则返回 Collapsed，否则返回 Visible
/// </summary>
public class ReversedBooleanToVisibilityConverter : IValueConverter
{
    #region Public Methods

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var booleanValue = (bool)value;
        return booleanValue ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var visibilityValue = (System.Windows.Visibility)value;
        return visibilityValue == System.Windows.Visibility.Collapsed;
    }

    #endregion Public Methods
}
