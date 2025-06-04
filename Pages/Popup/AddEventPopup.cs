using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using NutikasPaevik.Database;
using NutikasPaevik.Enums;
using NutikasPaevik.Services;
using System;
using System.Diagnostics;

namespace NutikasPaevik
{
    public class AddEventPopup : Popup
    {
        private Entry _titleEntry;
        private Editor _descriptionEditor;
        private DatePicker _datePicker;
        private TimePicker _startTimePicker;
        private TimePicker _endTimePicker;
        private Picker _typePicker;
        private Border _modernBorder;
        private DateTime _selectedDate;

        private const int MaxTitleLength = 100;
        private const int MaxDescriptionLength = 1000;

        public AddEventPopup(DateTime selectedDate)
        {
            try
            {
                _selectedDate = selectedDate;
                double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
                Size = new Size(screenWidth * 0.73, -1);
                Color = Colors.Transparent;

                var contentView = CreateEventView();
                Content = contentView;
                contentView.Scale = 0.8;
                contentView.ScaleTo(1.0, 250, Easing.CubicInOut);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AddEventPopup Initialization Error: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private View CreateEventView()
        {
            var eventContentLayout = new StackLayout { Spacing = 0 };
            var eventView = CreateModernView();
            eventContentLayout.Children.Add(eventView);

            var saveButton = new Button
            {
                ImageSource = Application.Current.Resources["ApplyIcon"] as string,
                BackgroundColor = Colors.Transparent,
                CornerRadius = 5,
                Margin = new Thickness(10),
                WidthRequest = 50,
                HeightRequest = 50,
                Padding = new Thickness(12)
            };
            saveButton.Clicked += OnSaveClicked;

            var cancelButton = new Button
            {
                ImageSource = Application.Current.Resources["DeleteIcon"] as string,
                BackgroundColor = Colors.Transparent,
                CornerRadius = 5,
                Margin = new Thickness(10),
                WidthRequest = 50,
                HeightRequest = 50,
                Padding = new Thickness(12)
            };
            cancelButton.Clicked += (s, e) => Close();

            var buttonStack = new HorizontalStackLayout
            {
                Spacing = 10,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Children = { saveButton, cancelButton }
            };

            var container = new Frame
            {
                BackgroundColor = Colors.Transparent,
                CornerRadius = 10,
                Margin = 10,
                Padding = 10,
                HasShadow = true,
                Content = new StackLayout
                {
                    Spacing = 10,
                    Children = { eventContentLayout, buttonStack }
                }
            };

            return container;
        }

        private View CreateModernView()
        {
            try
            {
                _modernBorder = new Border
                {
                    Margin = new Thickness(5),
                    StrokeThickness = 0,
                    MinimumHeightRequest = 100,
                    BackgroundColor = Color.FromArgb("#ADD8E6")
                };

                var strokeShape = new RoundRectangle { CornerRadius = 10 };
                _modernBorder.StrokeShape = strokeShape;

                var shadow = new Shadow
                {
                    Brush = Color.FromArgb("#999999"),
                    Offset = new Point(3, 3),
                    Radius = 5,
                    Opacity = 0.3f
                };
                _modernBorder.Shadow = shadow;

                var stackLayout = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.Fill,
                    Padding = new Thickness(0, 7),
                    Spacing = 10
                };

                _titleEntry = new Entry
                {
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#333"),
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(15, 0, 15, 0),
                    Placeholder = "Sündmuse nimi",
                    MaxLength = MaxTitleLength
                };

                _descriptionEditor = new Editor
                {
                    FontSize = 14,
                    TextColor = Color.FromArgb("#666"),
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(15, 0, 15, 5),
                    Placeholder = "Kirjeldus",
                    MaxLength = MaxDescriptionLength,
                    HeightRequest = 100
                };

                _datePicker = new DatePicker
                {
                    Date = _selectedDate,
                    Margin = new Thickness(15, 5),
                    HorizontalOptions = LayoutOptions.Fill
                };

                _startTimePicker = new TimePicker
                {
                    Time = new TimeSpan(DateTime.Now.Hour, 0, 0),
                    Margin = new Thickness(15, 5),
                    HorizontalOptions = LayoutOptions.Fill
                };

                _endTimePicker = new TimePicker
                {
                    Time = new TimeSpan(DateTime.Now.Hour + 1, 0, 0),
                    Margin = new Thickness(15, 5),
                    HorizontalOptions = LayoutOptions.Fill
                };

                _typePicker = new Picker
                {
                    Title = "Tüüp",
                    ItemsSource = Enum.GetNames(typeof(EventType)).ToList(),
                    SelectedIndex = 0,
                    Margin = new Thickness(15, 5),
                    HorizontalOptions = LayoutOptions.Fill
                };

                stackLayout.Children.Add(_titleEntry);
                stackLayout.Children.Add(new Label { Text = "Kirjeldus", FontSize = 14, Margin = new Thickness(15, 0, 0, 0) });
                stackLayout.Children.Add(_descriptionEditor);
                stackLayout.Children.Add(new Label { Text = "Kuupäev", FontSize = 14, Margin = new Thickness(15, 0, 0, 0) });
                stackLayout.Children.Add(_datePicker);
                stackLayout.Children.Add(new Label { Text = "Algusaeg", FontSize = 14, Margin = new Thickness(15, 0, 0, 0) });
                stackLayout.Children.Add(_startTimePicker);
                stackLayout.Children.Add(new Label { Text = "Lõpuaeg", FontSize = 14, Margin = new Thickness(15, 0, 0, 0) });
                stackLayout.Children.Add(_endTimePicker);
                stackLayout.Children.Add(new Label { Text = "Tüüp", FontSize = 14, Margin = new Thickness(15, 0, 0, 0) });
                stackLayout.Children.Add(_typePicker);

                _modernBorder.Content = stackLayout;
                return _modernBorder;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateModernView Error: {ex.Message}");
                return new Label { Text = "Error: ModernView creation failed", TextColor = Colors.Red };
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("AddEventPopup: Save button clicked.");
                if (string.IsNullOrWhiteSpace(_titleEntry.Text))
                {
                    await Application.Current.MainPage.DisplayAlert("Viga", "Sisestage sündmuse nimi", "OK");
                    return;
                }

                if (_startTimePicker.Time >= _endTimePicker.Time && _datePicker.Date == DateTime.Today)
                {
                    await Application.Current.MainPage.DisplayAlert("Viga", "Lõpuaeg peab olema hiljem kui algusaeg", "OK");
                    return;
                }

                var eventDate = _datePicker.Date;
                var startTime = _startTimePicker.Time;
                var endTime = _endTimePicker.Time;
                var startDateTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, startTime.Hours, startTime.Minutes, 0);
                var endDateTime = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, endTime.Hours, endTime.Minutes, 0);

                if (endDateTime <= startDateTime)
                {
                    await Application.Current.MainPage.DisplayAlert("Viga", "Lõpuaeg peab olema hiljem kui algusaeg", "OK");
                    return;
                }

                var newEvent = new Event
                {
                    SyncId = Guid.NewGuid().ToString(),
                    UserID = UserService.Instance.UserId,
                    Title = _titleEntry.Text,
                    Description = _descriptionEditor.Text,
                    Date = eventDate,
                    StartTime = startDateTime,
                    EndTime = endDateTime,
                    Type = (EventType)Enum.Parse(typeof(EventType), _typePicker.SelectedItem.ToString()),
                    CreationTime = DateTime.Now,
                    ModifyTime = DateTime.Now,
                    Status = "created"
                };

                Close(newEvent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AddEventPopup Save Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Viga", $"Ei saanud sündmust salvestada: {ex.Message}", "OK");
            }
        }
    }
}