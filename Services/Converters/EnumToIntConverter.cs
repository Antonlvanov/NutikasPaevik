using Microsoft.Maui.Controls;
using NutikasPaevik.Enums;
using System;
using System.Diagnostics;
using System.Globalization;

namespace NutikasPaevik
{
    public class EnumToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                return (int)(object)enumValue;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return Enum.ToObject(targetType, intValue);
            }
            return Enum.GetValues(targetType).GetValue(0);
        }
    }
}