using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace NutikasPaevik
{
    public class CalendarHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double pageHeight)
            {
                double availableHeight = pageHeight - 200;
                double rowHeight = availableHeight / 6;
                return rowHeight;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}