using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace EverythingHouse.WpfApp.Common.Converters;

public class ReplyStateToBrushConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var replyState = (ReplyState)value;
        return replyState switch
        {
            ReplyState.IsAccepted => App.Current.Resources["MikuGreenBrush"],
            ReplyState.IsMostLiked => App.Current.Resources["MikuRedBrush"],
            ReplyState.IsAcceptedAndMostLiked => App.Current.Resources["MikuMeaRedBlueGreenHorizontalBrush"],
            _ => App.Current.Resources["MeaBlueBrush"],
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
