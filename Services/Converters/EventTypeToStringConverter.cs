using Microsoft.Maui.Controls;
using NutikasPaevik.Enums;
using System;
using System.Globalization;

namespace NutikasPaevik
{
    public class EventTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EventType eventType)
            {
                return eventType switch
                {
                    EventType.Task => "Ülesanne",
                    EventType.Event => "Sündmus",
                    _ => eventType.ToString()
                };
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}