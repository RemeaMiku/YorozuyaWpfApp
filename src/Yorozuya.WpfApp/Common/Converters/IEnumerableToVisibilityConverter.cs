using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Yorozuya.WpfApp.Common.Converters;

/// <summary>
/// 可列举对象转换为可见性
/// 如果对象为长度为 0，则返回 Collapsed，否则返回 Visible
/// </summary>
public class IEnumerableToVisibilityConverter : IValueConverter
{
    #region Public Methods

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var enumerableValue = (IEnumerable<object>)value;
        return enumerableValue is null || enumerableValue.Any() ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    #endregion Public Methods
}
