using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Yorozuya.WpfApp.Common.Converters;

/// <summary>
/// 反转的可空值转可见性转换器
/// 如果值为null，则返回 Visible，否则返回Collapsed
/// </summary>
public class ReversedNullableToVisibilityConveter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

