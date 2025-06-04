using Microsoft.Maui.Controls;
using NutikasPaevik.Enums;
using System;
using System.Globalization;

namespace NutikasPaevik
{
    public class EventTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EventType eventType)
            {
                return eventType switch
                {
                    EventType.Event => Application.Current.Resources["EventIcon"] as string ?? "event_black.png",
                    EventType.Task => Application.Current.Resources["TaskIcon"] as string ?? "task_black.png",
                };
            }
            return "event_black.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}