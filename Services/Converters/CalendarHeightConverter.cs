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
                // Вычитаем примерную высоту заголовков (месяц ~50, дни недели ~40)
                double availableHeight = pageHeight - 200;
                // Делим на 6 строк (6 недель)
                double rowHeight = availableHeight / 6;
                // Возвращаем общую высоту для CollectionView (6 строк)
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