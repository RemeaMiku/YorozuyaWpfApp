using System;
using System.Globalization;
using System.Windows.Data;

namespace Yorozuya.WpfApp.Common.Converters;

public class ReplyStateToBrushConverter : IValueConverter
{

    #region Public Methods

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

    #endregion Public Methods
}
