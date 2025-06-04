using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using NutikasPaevik.Database;
using NutikasPaevik.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutikasPaevik
{
    public class AddNotePopup : Popup
    {
        private Entry _titleEntry;
        private Editor _contentEntry;
        private Picker _colorPicker;
        private Border _modernBorder;

        private const int MaxContentLength = 1000;
        private const int MaxEditorHeight = 300;
        private const int MinEditorHeight = 100;
        private const double LineHeight = 18;

        public AddNotePopup()
        {
            try
            {
                Debug.WriteLine("NotePopup: Initializing...");

                double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
                Size = new Size(screenWidth * 0.73, -1);
                Color = Colors.Transparent;

                var contentView = CreateNoteView();
                Content = contentView;

                contentView.Scale = 0.8;
                contentView.ScaleTo(1.0, 250, Easing.CubicInOut);

                Debug.WriteLine("NotePopup: Initialization complete.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NotePopup Initialization Error: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("NotePopup: Save button clicked.");
                if (string.IsNullOrWhiteSpace(_titleEntry.Text) || string.IsNullOrWhiteSpace(_contentEntry.Text))
                {
                    await Application.Current.MainPage.DisplayAlert("Viga", "Täitke kõik väljad", "OK");
                    return;
                }

                NoteColor selectedColor = (NoteColor)_colorPicker.SelectedIndex;
                var note = new Note
                {
                    SyncId = Guid.NewGuid().ToString(),
                    UserID = UserService.Instance.UserId,
                    Title = _titleEntry.Text,
                    Content = _contentEntry.Text,
                    NoteColor = selectedColor,
                    CreationTime = DateTime.Now,
                    ModifyTime = DateTime.Now,
                    Status = "created"
                };
                Close(note);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NotePopup Save Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Viga", $"Ei saanud märkmeid salvestada: {ex.Message}", "OK");
            }
        }

        private View CreateNoteView()
        {
            var noteContentLayout = new StackLayout { Spacing = 0 };

            var noteView = CreateModernView();
            noteContentLayout.Children.Add(noteView);

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
                    Children = { noteContentLayout, buttonStack }
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
                    BackgroundColor = new NoteColorToColorConverter().Convert(NoteColor.Blue, null, null, null) as Color
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
                    Padding = new Thickness(0, 7)
                };

                _titleEntry = new Entry
                {
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#333"),
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(15, 0, 15, 0),
                    Placeholder = "Pealkiri"
                };

                var line = new BoxView
                {
                    HeightRequest = 1,
                    BackgroundColor = Colors.Gray,
                    Margin = new Thickness(0, 3, 0, 3)
                };

                _contentEntry = new Editor
                {
                    FontSize = 14,
                    TextColor = Color.FromArgb("#666"),
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(15, 0, 15, 5),
                    HeightRequest = MinEditorHeight,
                    Placeholder = "Sisu",
                    MaxLength = MaxContentLength
                };
                _contentEntry.TextChanged += OnContentEntryTextChanged;

                _colorPicker = new Picker
                {
                    Title = "Värv",
                    ItemsSource = new[] { "Sinine", "Punane", "Roheline", "Kollane" },
                    SelectedIndex = new Random().Next(0, 4), // Случайный начальный цвет
                    Margin = new Thickness(15, 5),
                    HorizontalOptions = LayoutOptions.Fill
                };
                _colorPicker.SelectedIndexChanged += OnColorPickerChanged;

                stackLayout.Children.Add(_titleEntry);
                stackLayout.Children.Add(line);
                stackLayout.Children.Add(_contentEntry);
                stackLayout.Children.Add(_colorPicker);

                _modernBorder.Content = stackLayout;

                var newColor = (NoteColor)_colorPicker.SelectedIndex;
                var color = new NoteColorToColorConverter().Convert(newColor, null, null, null) as Color;
                _modernBorder.BackgroundColor = color;
                return _modernBorder;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateModernView Error: {ex.Message}");
                return new Label { Text = "Error: ModernView creation failed", TextColor = Colors.Red };
            }
        }



        private async void OnContentEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_contentEntry == null) return;

            try
            {
                if (e.NewTextValue.Length > MaxContentLength)
                {
                    _contentEntry.Text = e.OldTextValue;
                    await Application.Current.MainPage.DisplayAlert("Viga", $"Sisu ei tohi ületada {MaxContentLength} tähemärki.", "OK");
                    return;
                }

                int lineCount = string.IsNullOrEmpty(e.NewTextValue) ? 1 : e.NewTextValue.Split('\n').Length;
                const int maxLines = 20;
                if (lineCount > maxLines)
                {
                    _contentEntry.Text = e.OldTextValue;
                    await Application.Current.MainPage.DisplayAlert("Viga", $"Sisu ei tohi ületada {maxLines} rida.", "OK");
                    return;
                }

                double newHeight = Math.Max(MinEditorHeight, lineCount * LineHeight);
                newHeight = Math.Min(newHeight, MaxEditorHeight);

                _contentEntry.HeightRequest = newHeight;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnContentEntryTextChanged Error: {ex.Message}");
            }
        }

        private void OnColorPickerChanged(object sender, EventArgs e)
        {
            if (_colorPicker.SelectedIndex >= 0)
            {
                var newColor = (NoteColor)_colorPicker.SelectedIndex;
                var color = new NoteColorToColorConverter().Convert(newColor, null, null, null) as Color;
                if (color != null && _modernBorder != null)
                {
                    _modernBorder.BackgroundColor = color;
                }
            }
        }
    }
}
