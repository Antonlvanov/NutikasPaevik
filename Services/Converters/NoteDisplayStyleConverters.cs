using Microsoft.Maui.Controls;
using NutikasPaevik.Enums;
using System;
using System.Diagnostics;
using System.Globalization;

namespace NutikasPaevik
{
    public class NoteDisplayStyleToTemplateConverter : IValueConverter
    {
        public DataTemplate StickerTemplate { get; set; }
        public DataTemplate ModernTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NoteDisplayStyle style)
            {
                return style == NoteDisplayStyle.Sticker ? StickerTemplate : ModernTemplate;
            }
            return ModernTemplate; // По умолчанию
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class NoteColorToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Debug.WriteLine($"NoteColorToImageConverter: Converting value: {value}");
                if (value is NoteColor color)
                {
                    string imageName = color switch
                    {
                        NoteColor.Blue => "bluenote.png",
                        NoteColor.Red => "rednote.png",
                        NoteColor.Green => "greennote.png",
                        NoteColor.Yellow => "yellownote.png",
                        _ => "bluenote.png" // Используем bluenote как запасное
                    };
                    Debug.WriteLine($"NoteColorToImageConverter: Returning image: {imageName}");
                    return ImageSource.FromFile(imageName);
                }
                Debug.WriteLine("NoteColorToImageConverter: Value is not a NoteColor, returning bluenote.png");
                return ImageSource.FromFile("bluenote.png");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NoteColorToImageConverter Error: {ex.Message}\n{ex.StackTrace}");
                return ImageSource.FromFile("bluenote.png"); // Фallback на существующее изображение
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class NoteColorToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NoteColor color)
            {
                return color switch
                {
                    NoteColor.Red => Colors.LightPink,
                    NoteColor.Blue => Colors.LightSkyBlue,
                    NoteColor.Green => Colors.LightGreen,
                    NoteColor.Yellow => Colors.LightYellow,
                    _ => Colors.Yellow
                };
            }
            return Colors.Yellow;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}