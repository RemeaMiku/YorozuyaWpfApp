using System;
using System.Globalization;
using System.Windows.Data;

namespace Yorozuya.WpfApp.Common.Converters;

/// <summary>
/// 可空类型对象转换为可见性
/// 如果对象为 null，则返回 Collapsed，否则返回 Visible
/// </summary>
public class NullableToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is null ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
