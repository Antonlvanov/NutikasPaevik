using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace NutikasPaevik
{
    public class IsTodayToBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isToday && isToday)
            {
                return Application.Current.Resources["TextColor"];
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}