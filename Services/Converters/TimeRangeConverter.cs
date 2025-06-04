using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace NutikasPaevik
{
    public class TimeRangeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is DateTime startTime && values[1] is DateTime endTime)
            {
                return $"{startTime:HH:mm} - {endTime:HH:mm}";
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}