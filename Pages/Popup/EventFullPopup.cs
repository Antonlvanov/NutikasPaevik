using CommunityToolkit.Maui.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using NutikasPaevik.Database;
using NutikasPaevik.Enums;
using NutikasPaevik.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NutikasPaevik
{
    public class EventFullPopup : Popup
    {
        private Event _event;
        private readonly PlannerViewModel _viewModel;
        private bool _isEditing;
        private StackLayout _eventContentLayout;
        private Entry _titleEntry;
        private Editor _descriptionEditor;
        private DatePicker _datePicker;
        private TimePicker _startTimePicker;
        private TimePicker _endTimePicker;
        private Picker _typePicker;
        private Button _editSaveButton;
        private Border _modernBorder;

        private const int MaxTitleLength = 100;
        private const int MaxDescriptionLength = 1000;
        private const int MaxEditorHeight = 300;
        private const int MinEditorHeight = 100;
        private const double LineHeight = 18;

        public EventFullPopup(Event evnt, PlannerViewModel viewModel)
        {
            try
            {
                _event = evnt ?? throw new ArgumentNullException(nameof(evnt));
                _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
                _isEditing = false;

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
                Debug.WriteLine($"EventFullPopup Initialization Error: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private View CreateEventView()
        {
            _eventContentLayout = new StackLayout { Spacing = 0 };
            UpdateEventContent(false);

            _editSaveButton = new Button
            {
                ImageSource = Application.Current.Resources["EditIcon"] as string,
                BackgroundColor = Colors.Transparent,
                CornerRadius = 5,
                Margin = new Thickness(10),
                WidthRequest = 50,
                HeightRequest = 50,
                Padding = new Thickness(12)
            };
            _editSaveButton.Clicked += OnEditSaveClicked;

            var deleteButton = new Button
            {
                ImageSource = Application.Current.Resources["DeleteIcon"] as string,
                BackgroundColor = Colors.Transparent,
                CornerRadius = 5,
                Margin = new Thickness(10),
                WidthRequest = 50,
                HeightRequest = 50,
                Padding = new Thickness(12)
            };
            deleteButton.Clicked += async (s, e) => await OnDeleteClicked();

            var buttonStack = new HorizontalStackLayout
            {
                Spacing = 10,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Children = { _editSaveButton, deleteButton }
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
                    Children = { _eventContentLayout, buttonStack }
                }
            };

            return container;
        }

        private void UpdateEventContent(bool isEditing)
        {
            _eventContentLayout.Children.Clear();
            var eventView = CreateModernView(isEditing);
            _eventContentLayout.Children.Add(eventView);
        }

        private View CreateModernView(bool isEditing)
        {
            try
            {
                _modernBorder = new Border
                {
                    Margin = new Thickness(5),
                    StrokeThickness = 0,
                    MinimumHeightRequest = 100,
                    BackgroundColor = new EventTypeToColorConverter().Convert(_event.Type, null, null, null) as Color
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

                if (isEditing)
                {
                    _titleEntry = new Entry
                    {
                        FontSize = 15,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#333"),
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Start,
                        Margin = new Thickness(15, 0, 15, 0),
                        Text = _event.Title,
                        Placeholder = "Sündmuse nimi",
                        MaxLength = MaxTitleLength
                    };

                    var line = new BoxView
                    {
                        HeightRequest = 1,
                        BackgroundColor = Colors.Gray,
                        Margin = new Thickness(0, 3, 0, 3)
                    };

                    _descriptionEditor = new Editor
                    {
                        FontSize = 14,
                        TextColor = Color.FromArgb("#666"),
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Start,
                        Margin = new Thickness(15, 0, 15, 5),
                        HeightRequest = MinEditorHeight,
                        Text = _event.Description,
                        Placeholder = "Kirjeldus",
                        MaxLength = MaxDescriptionLength
                    };
                    _descriptionEditor.TextChanged += OnDescriptionEditorTextChanged;

                    OnDescriptionEditorTextChanged(_descriptionEditor, new TextChangedEventArgs(null, _descriptionEditor.Text));

                    _datePicker = new DatePicker
                    {
                        Date = _event.Date,
                        Margin = new Thickness(15, 5),
                        HorizontalOptions = LayoutOptions.Fill
                    };

                    _startTimePicker = new TimePicker
                    {
                        Time = _event.StartTime.TimeOfDay,
                        Margin = new Thickness(15, 5),
                        HorizontalOptions = LayoutOptions.Fill
                    };

                    _endTimePicker = new TimePicker
                    {
                        Time = _event.EndTime.TimeOfDay,
                        Margin = new Thickness(15, 5),
                        HorizontalOptions = LayoutOptions.Fill
                    };

                    _typePicker = new Picker
                    {
                        Title = "Tüüp",
                        ItemsSource = Enum.GetNames(typeof(EventType)).ToList(),
                        SelectedItem = _event.Type.ToString(),
                        Margin = new Thickness(15, 5),
                        HorizontalOptions = LayoutOptions.Fill
                    };

                    stackLayout.Children.Add(_titleEntry);
                    stackLayout.Children.Add(line);
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
                }
                else
                {
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
                        TextColor = Colors.Black,
                        HorizontalOptions = LayoutOptions.Start,
                        Padding = new Thickness(15, 0, 0, 0),
                        LineBreakMode = LineBreakMode.TailTruncation,
                        Text = _event.Title
                    };

                    var timeLabel = new Label
                    {
                        FontSize = 12,
                        TextColor = Colors.Black,
                        HorizontalOptions = LayoutOptions.End,
                        VerticalOptions = LayoutOptions.Start,
                        Padding = new Thickness(5, 3, 15, 0),
                        Text = _event.ModifyTime.ToString("HH:mm") ?? _event.CreationTime.ToString("HH:mm")
                    };

                    grid.Add(titleLabel, 0, 0);
                    grid.Add(timeLabel, 1, 0);

                    var line = new BoxView
                    {
                        HeightRequest = 1,
                        BackgroundColor = Colors.Black,
                        Margin = new Thickness(0, 3, 0, 3)
                    };

                    var descLabel = new Label
                    {
                        FontSize = 14,
                        TextColor = Colors.Gray,
                        Padding = new Thickness(15, 0, 15, 5),
                        Text = "Kirjeldus:"
                    };

                    var description = new Label
                    {
                        FontSize = 13,
                        TextColor = Colors.Black,
                        MaxLines = 8,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.Start,
                        LineBreakMode = LineBreakMode.WordWrap,
                        Padding = new Thickness(15, 0, 15, 5),
                        Text = _event.Description
                    };

                    var dateLabel = new Label
                    {
                        FontSize = 14,
                        TextColor = Colors.Black,
                        Padding = new Thickness(15, 0, 15, 5),
                        Text = $"Kuupäev: {_event.Date.ToString("dd MMMM yyyy")}" 
                    };

                    var startTimeLabel = new Label
                    {
                        FontSize = 14,
                        TextColor = Colors.Black,
                        Padding = new Thickness(15, 0, 15, 5),
                        Text = $"Algusaeg: {_event.StartTime:HH:mm}"
                    };

                    var endTimeLabel = new Label
                    {
                        FontSize = 14,
                        TextColor = Colors.Black,
                        Padding = new Thickness(15, 0, 15, 5),
                        Text = $"Lõpuaeg: {_event.EndTime:HH:mm}"
                    };

                    var typeLabel = new Label
                    {
                        FontSize = 14,
                        TextColor = Colors.Black,
                        Padding = new Thickness(15, 0, 15, 5),
                        Text = $"Tüüp: {_event.Type}"
                    };

                    stackLayout.Children.Add(grid);
                    stackLayout.Children.Add(line);
                    stackLayout.Children.Add(description);
                    stackLayout.Children.Add(dateLabel);
                    stackLayout.Children.Add(startTimeLabel);
                    stackLayout.Children.Add(endTimeLabel);
                    stackLayout.Children.Add(typeLabel);
                }

                _modernBorder.Content = stackLayout;
                return _modernBorder;
            }
            catch (Exception ex)
            {
                return new Label { Text = "Error: ModernView creation failed", TextColor = Colors.Red };
            }
        }

        private void OnDescriptionEditorTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_descriptionEditor == null) return;

            try
            {
                if (e.NewTextValue.Length > MaxDescriptionLength)
                {
                    _descriptionEditor.Text = e.OldTextValue;
                    Application.Current.MainPage.DisplayAlert("Viga", $"Kirjeldus ei tohi ületada {MaxDescriptionLength} tähemärki.", "OK");
                    return;
                }

                int lineCount = string.IsNullOrEmpty(e.NewTextValue) ? 1 : e.NewTextValue.Split('\n').Length;
                const int maxLines = 20;
                if (lineCount > maxLines)
                {
                    _descriptionEditor.Text = e.OldTextValue;
                    Application.Current.MainPage.DisplayAlert("Viga", $"Kirjeldus ei tohi ületada {maxLines} rida.", "OK");
                    return;
                }

                double newHeight = Math.Max(MinEditorHeight, lineCount * LineHeight);
                newHeight = Math.Min(newHeight, MaxEditorHeight);
                _descriptionEditor.HeightRequest = newHeight;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnDescriptionEditorTextChanged error: {ex.Message}");
            }
        }

        private async void OnEditSaveClicked(object sender, EventArgs e)
        {
            try
            {
                if (_isEditing)
                {
                    if (string.IsNullOrWhiteSpace(_titleEntry.Text) || string.IsNullOrWhiteSpace(_descriptionEditor.Text))
                    {
                        await Application.Current.MainPage.DisplayAlert("Viga", "Täitke kõik väljad", "OK");
                        return;
                    }

                    var dbContext = App.Services.GetService<AppDbContext>();
                    if (dbContext == null)
                        throw new InvalidOperationException("AppDbContext not found");

                    var entry = dbContext.Entry(_event);
                    Event eventToUpdate;
                    if (entry.State == EntityState.Detached)
                    {
                        eventToUpdate = await dbContext.Events.FirstOrDefaultAsync(e => e.SyncId == _event.SyncId || e.Id == _event.Id);
                        if (eventToUpdate != null)
                        {
                            eventToUpdate.Title = _titleEntry.Text;
                            eventToUpdate.Description = _descriptionEditor.Text;
                            eventToUpdate.Date = _datePicker.Date;
                            eventToUpdate.StartTime = new DateTime(_datePicker.Date.Year, _datePicker.Date.Month, _datePicker.Date.Day, _startTimePicker.Time.Hours, _startTimePicker.Time.Minutes, 0);
                            eventToUpdate.EndTime = new DateTime(_datePicker.Date.Year, _datePicker.Date.Month, _datePicker.Date.Day, _endTimePicker.Time.Hours, _endTimePicker.Time.Minutes, 0);
                            eventToUpdate.Type = (EventType)Enum.Parse(typeof(EventType), _typePicker.SelectedItem.ToString());
                            eventToUpdate.ModifyTime = DateTime.Now;
                        }
                        else
                        {
                            _event.Title = _titleEntry.Text;
                            _event.Description = _descriptionEditor.Text;
                            _event.Date = _datePicker.Date;
                            _event.StartTime = new DateTime(_datePicker.Date.Year, _datePicker.Date.Month, _datePicker.Date.Day, _startTimePicker.Time.Hours, _startTimePicker.Time.Minutes, 0);
                            _event.EndTime = new DateTime(_datePicker.Date.Year, _datePicker.Date.Month, _datePicker.Date.Day, _endTimePicker.Time.Hours, _endTimePicker.Time.Minutes, 0);
                            _event.Type = (EventType)Enum.Parse(typeof(EventType), _typePicker.SelectedItem.ToString());
                            _event.ModifyTime = DateTime.Now;
                            dbContext.Events.Update(_event);
                            eventToUpdate = _event;
                        }
                    }
                    else
                    {
                        _event.Title = _titleEntry.Text;
                        _event.Description = _descriptionEditor.Text;
                        _event.Date = _datePicker.Date;
                        _event.StartTime = new DateTime(_datePicker.Date.Year, _datePicker.Date.Month, _datePicker.Date.Day, _startTimePicker.Time.Hours, _startTimePicker.Time.Minutes, 0);
                        _event.EndTime = new DateTime(_datePicker.Date.Year, _datePicker.Date.Month, _datePicker.Date.Day, _endTimePicker.Time.Hours, _endTimePicker.Time.Minutes, 0);
                        _event.Type = (EventType)Enum.Parse(typeof(EventType), _typePicker.SelectedItem.ToString());
                        _event.ModifyTime = DateTime.Now;
                        eventToUpdate = _event;
                    }

                    await dbContext.SaveChangesAsync();
                    await SyncService.UploadEventsToServerAsync();

                    var updatedEvent = await dbContext.Events.FirstOrDefaultAsync(e => e.SyncId == eventToUpdate.SyncId || e.Id == eventToUpdate.Id);
                    if (updatedEvent != null)
                    {
                        _event = updatedEvent;
                        var index = _viewModel.Events.IndexOf(_viewModel.Events.FirstOrDefault(e => e.SyncId == _event.SyncId));
                        if (index >= 0)
                        {
                            _viewModel.Events[index] = _event;
                        }
                        else
                        {
                            _viewModel.Events.Add(_event);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Updated event not found in database after save.");
                    }

                    _isEditing = false;
                    _editSaveButton.ImageSource = Application.Current.Resources["EditIcon"] as string;
                    UpdateEventContent(false);
                    _viewModel.UpdateEventsByDate();
                    _viewModel.OnPropertyChanged(nameof(EventsByDate));

                }
                else
                {
                    _isEditing = true;
                    _editSaveButton.ImageSource = Application.Current.Resources["ApplyIcon"] as string;
                    UpdateEventContent(true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnEditSaveClicked error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Viga", $"Ei saanud sündmust salvestada: {ex.Message}", "OK");
            }
        }

        private async Task OnDeleteClicked()
        {
            try
            {
                bool confirm = await Application.Current.MainPage.DisplayAlert("Kinnita", "Kas olete kindel, et soovite selle sündmuse kustutada?", "Jah", "Ei");
                if (confirm)
                {
                    await _viewModel.DeleteEvent(_event);
                    Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnDeleteClicked error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Viga", $"Ei saanud sündmust kustutada: {ex.Message}", "OK");
            }
        }
    }
}