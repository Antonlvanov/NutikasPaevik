using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace NutikasPaevik.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                if (status.ToLower().Contains("success") || status.ToLower().Contains("успех"))
                    return Colors.Green;
                if (status.ToLower().Contains("fail") || status.ToLower().Contains("error") || status.ToLower().Contains("ошибка"))
                    return Colors.Red;
            }
            // Цвет по умолчанию для обычных сообщений или если DynamicResource не сработает
            // Лучше использовать DynamicResource, если определены цвета для статусов в App.xaml
            return Application.Current.RequestedTheme == AppTheme.Dark ? Colors.LightGray : Colors.DarkGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}