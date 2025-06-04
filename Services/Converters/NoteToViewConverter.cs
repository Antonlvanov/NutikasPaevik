using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using NutikasPaevik.Database;
using NutikasPaevik.Enums;
using System;
using System.Diagnostics;
using System.Globalization;

namespace NutikasPaevik
{
    public class NoteToViewConverter : IValueConverter
    {
        private readonly NoteColorToImageConverter _imageConverter = new NoteColorToImageConverter();
        private readonly NoteColorToColorConverter _colorConverter = new NoteColorToColorConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Note note)
            {
                var style = AppSettings.Instance.NoteDisplayStyle;
                return style == NoteDisplayStyle.Sticker ? CreateStickerView(note) : CreateModernView(note);
            }
            return new Label { Text = "Error: Invalid Note" };
        }

        //public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    if (value is Note note)
        //    {
        //        var style = AppSettings.Instance.NoteDisplayStyle;
        //        var noteView = style == NoteDisplayStyle.Sticker ? CreateStickerView(note) : CreateModernView(note);
        //        return noteView;
        //    }
        //    return new Label { Text = "Error: Invalid Note" };
        //}

        private View CreateStickerView(Note note)
        {
            try
            {
                var frame = new Frame
                {
                    CornerRadius = 10,
                    BackgroundColor = Colors.Transparent,
                    Margin = new Thickness(5,0,5,5),
                    Padding = new Thickness(0),
                    HasShadow = false,
                    BorderColor = Colors.Transparent,
                    MinimumHeightRequest = 100,
                    MaximumHeightRequest = 400
                };

                var grid = new Grid();
                var image = new Image
                {
                    Aspect = Aspect.Fill,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    Source = ImageSource.FromFile("bluenote.png")
                };
                image.SetBinding(Image.SourceProperty, new Binding("NoteColor", converter: _imageConverter));

                var stackLayout = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Padding = new Thickness(0, 7, 0, 10)
                };

                var titleLabel = new Label
                {
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#333"),
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.StartAndExpand,
                    LineBreakMode = LineBreakMode.WordWrap,
                    Padding = new Thickness(15, 6, 15, 0)
                };
                titleLabel.SetBinding(Label.TextProperty, new Binding("Title"));

                var timeLabel = new Label
                {
                    FontSize = 12,
                    TextColor = Color.FromArgb("#666"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(0,0,4,0),
                    LineBreakMode = LineBreakMode.TailTruncation,
                };
                timeLabel.SetBinding(Label.TextProperty, new Binding("ModifyTime", stringFormat: "{0:HH:mm}"));

                var line = new BoxView
                {
                    HeightRequest = 1,
                    BackgroundColor = Colors.Gray,
                    Margin = new Thickness(3, 2, 3, 2)
                };

                var contentLabel = new Label
                {
                    FontSize = 14,
                    TextColor = Color.FromArgb("#666"),
                    MaxLines = 14,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.Start,
                    LineBreakMode = LineBreakMode.WordWrap,
                    Padding = new Thickness(15, 0, 15, 5)
                };
                contentLabel.SetBinding(Label.TextProperty, new Binding("Content"));

                stackLayout.Children.Add(timeLabel);
                stackLayout.Children.Add(titleLabel);
                stackLayout.Children.Add(line);
                stackLayout.Children.Add(contentLabel);

                grid.Children.Add(image);
                grid.Children.Add(stackLayout);
                frame.Content = grid;

                Debug.WriteLine("NoteToViewConverter: StickerView created successfully.");
                return frame;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NoteToViewConverter CreateStickerView Error: {ex.Message}\n{ex.StackTrace}");
                return new Label { Text = "Error: StickerView creation failed" };
            }
        }

        private View CreateModernView(Note note)
        {
            try
            {
                Debug.WriteLine($"NoteToViewConverter: Creating ModernView for note: {note.Title}");
                var border = new Border
                {
                    Margin = new Thickness(5),
                    StrokeThickness = 0,
                    MinimumHeightRequest = 100,
                    MaximumHeightRequest = 400
                };
                border.SetBinding(VisualElement.BackgroundColorProperty, new Binding("NoteColor", converter: _colorConverter));

                var strokeShape = new RoundRectangle { CornerRadius = 10 };
                border.StrokeShape = strokeShape;

                var shadow = new Shadow
                {
                    Brush = Color.FromArgb("#999999"),
                    Offset = new Point(3, 3),
                    Radius = 5,
                    Opacity = 0.3f
                };
                border.Shadow = shadow;

                var stackLayout = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Fill,
                    HorizontalOptions = LayoutOptions.Fill,
                    Padding = new Thickness(0, 7)
                };

                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto }
                    },
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Start
                };

                var titleLabel = new Label
                {
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#333"),
                    HorizontalOptions = LayoutOptions.Start,
                    Padding = new Thickness(15, 0, 0, 0),
                    LineBreakMode = LineBreakMode.TailTruncation,
                };
                titleLabel.SetBinding(Label.TextProperty, new Binding("Title"));

                var timeLabel = new Label
                {
                    FontSize = 12,
                    TextColor = Color.FromArgb("#666"),
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start,
                    Padding = new Thickness(5, 3, 15, 0)
                };
                timeLabel.SetBinding(Label.TextProperty, new Binding("ModifyTime", stringFormat: "{0:HH:mm}"));

                grid.Add(titleLabel, 0, 0); // Title in first column
                grid.Add(timeLabel, 1, 0);  // Time in second column

                var line = new BoxView
                {
                    HeightRequest = 1,
                    BackgroundColor = Colors.Gray,
                    Margin = new Thickness(0, 3, 0, 3)
                };

                var contentLabel = new Label
                {
                    FontSize = 14,
                    TextColor = Color.FromArgb("#666"),
                    MaxLines = 14,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.Start,
                    LineBreakMode = LineBreakMode.WordWrap,
                    Padding = new Thickness(15, 0, 15, 5)
                };
                contentLabel.SetBinding(Label.TextProperty, new Binding("Content"));

                stackLayout.Children.Add(grid);
                stackLayout.Children.Add(line);
                stackLayout.Children.Add(contentLabel);

                border.Content = stackLayout;

                Debug.WriteLine("NoteToViewConverter: ModernView created successfully.");
                return border;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NoteToViewConverter CreateModernView Error: {ex.Message}\n{ex.StackTrace}");
                return new Label { Text = "Error: ModernView creation failed" };
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HalfWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is double width)
                {
                    var totalHorizontalSpacing = 10 + 20;
                    var result = (width - totalHorizontalSpacing) / 2;
                    return result < 50 ? 50 : result;
                }
                return 50;
            }
            catch (Exception ex)
            {
                return 50;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}