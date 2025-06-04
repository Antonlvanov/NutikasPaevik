using Microsoft.Maui.Controls;
using NutikasPaevik.Enums;
using System;
using System.Globalization;
using AppTheme = NutikasPaevik.Enums.AppTheme;

namespace NutikasPaevik
{
    public class EventTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EventType eventType)
            {
                return eventType switch
                {
                    EventType.Event => Application.Current.Resources["EventColor"] as Color ?? Color.FromArgb("#87CEEB"),
                    EventType.Task => Application.Current.Resources["TaskColor"] as Color ?? Color.FromArgb("#98FB98"),
                };
            }
            return Color.FromArgb("#FFFFFF");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}