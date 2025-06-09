using CommunityToolkit.Maui.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using NutikasPaevik.Database;
using NutikasPaevik.Database.Models;
using NutikasPaevik.Enums;
using NutikasPaevik.Services;
using System;
using System.Threading.Tasks;

namespace NutikasPaevik
{
    public class NoteFullPopup : Popup
    {
        private Note _note;
        private readonly DiaryViewModel _viewModel;
        private bool _isEditing;
        private StackLayout _noteContentLayout;
        private Entry _titleEntry;
        private Editor _contentEditor;
        private Picker _colorPicker;
        private Button _editSaveButton;
        private Border _modernBorder;

        private const int MaxContentLength = 1000; 
        private const int MaxEditorHeight = 300; 
        private const int MinEditorHeight = 100; 
        private const double LineHeight = 18;

        public NoteFullPopup(Note note, DiaryViewModel viewModel)
        {
            _note = note ?? throw new ArgumentNullException(nameof(note));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _isEditing = false;

            double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            Size = new Size(screenWidth * 0.73, -1);
            Color = Colors.Transparent;

            var contentView = CreateNoteView();
            Content = contentView;

            contentView.Scale = 0.8;
            contentView.ScaleTo(1.0, 250, Easing.CubicInOut);
        }

        private async void OnEditSaveClicked(object sender, EventArgs e)
        {
            try
            {
                if (_isEditing)
                {
                    if (string.IsNullOrWhiteSpace(_titleEntry.Text) || string.IsNullOrWhiteSpace(_contentEditor.Text))
                    {
                        await Application.Current.MainPage.DisplayAlert("Viga", "Täitke kõik väljad", "OK");
                        return;
                    }

                    var dbContext = App.Services.GetService<AppDbContext>();
                    if (dbContext == null)
                        throw new InvalidOperationException("AppDbContext not found");

                    var entry = dbContext.Entry(_note);
                    Note noteToUpdate;
                    if (entry.State == EntityState.Detached)
                    {
                        noteToUpdate = await dbContext.Notes.FirstOrDefaultAsync(n => n.SyncId == _note.SyncId || n.Id == _note.Id);
                        if (noteToUpdate != null)
                        {
                            noteToUpdate.Title = _titleEntry.Text;
                            noteToUpdate.Content = _contentEditor.Text;
                            noteToUpdate.ModifyTime = DateTime.Now;
                            noteToUpdate.NoteColor = (NoteColor)_colorPicker.SelectedIndex;
                            noteToUpdate.LastSyncTime = DateTime.Now;
                        }
                        else
                        {
                            _note.Title = _titleEntry.Text;
                            _note.Content = _contentEditor.Text;
                            _note.ModifyTime = DateTime.Now;
                            _note.NoteColor = (NoteColor)_colorPicker.SelectedIndex;
                            _note.LastSyncTime = DateTime.Now;
                            dbContext.Notes.Update(_note);
                            noteToUpdate = _note;
                        }
                    }
                    else
                    {
                        _note.Title = _titleEntry.Text;
                        _note.Content = _contentEditor.Text;
                        _note.ModifyTime = DateTime.Now;
                        _note.NoteColor = (NoteColor)_colorPicker.SelectedIndex;
                        _note.LastSyncTime = DateTime.Now;
                        noteToUpdate = _note;
                    }

                    await dbContext.SaveChangesAsync();
                    await SyncService.UploadNotesToServerAsync();

                    var updatedNote = await dbContext.Notes.FirstOrDefaultAsync(n => n.SyncId == noteToUpdate.SyncId || n.Id == noteToUpdate.Id);
                    if (updatedNote != null)
                    {
                        _note = updatedNote;
                        var index = _viewModel.Notes.IndexOf(_viewModel.Notes.FirstOrDefault(n => n.SyncId == _note.SyncId));
                        if (index >= 0)
                        {
                            _viewModel.Notes[index] = _note;
                        }
                        else
                        {
                            _viewModel.Notes.Add(_note);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Updated note not found in database after save.");
                    }

                    _isEditing = false;
                    _editSaveButton.ImageSource = Application.Current.Resources["EditIcon"] as string;
                    UpdateNoteContent(false);
                }
                else
                {
                    _isEditing = true;
                    _editSaveButton.ImageSource = Application.Current.Resources["ApplyIcon"] as string;
                    UpdateNoteContent(true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnEditSaveClicked error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Viga", $"Ei saanud märkmeid salvestada: {ex.Message}", "OK");
            }
        }

        private async Task OnDeleteClicked()
        {
            try
            {
                bool confirm = await Application.Current.MainPage.DisplayAlert("Kinnita", "Kas olete kindel, et soovite selle märkme kustutada?", "Jah", "Ei");
                if (confirm)
                {
                    await _viewModel.DeleteNote(_note);
                    Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnDeleteClicked error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Viga", $"Ei saanud märkmeid kustutada: {ex.Message}", "OK");
            }
        }

        private View CreateNoteView()
        {
            _noteContentLayout = new StackLayout { Spacing = 0 };
            UpdateNoteContent(false);

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
                    Children = { _noteContentLayout, buttonStack }
                }
            };

            return container;
        }

        private void UpdateNoteContent(bool isEditing)
        {
            _noteContentLayout.Children.Clear();
            var noteView = CreateModernView(isEditing);
            _noteContentLayout.Children.Add(noteView);
        }

        private View CreateStickerView(bool isEditing)
        {
            return new Label { Text = "StickerView not implemented", TextColor = Colors.Red };
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
                    BackgroundColor = new NoteColorToColorConverter().Convert(_note.NoteColor, null, null, null) as Color
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
                        Text = _note.Title,
                        Placeholder = "Pealkiri"
                    };

                    var line = new BoxView
                    {
                        HeightRequest = 1,
                        BackgroundColor = Colors.Gray,
                        Margin = new Thickness(0, 3, 0, 3)
                    };
                    _contentEditor = new Editor
                    {
                        FontSize = 14,
                        TextColor = Color.FromArgb("#666"),
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Start,
                        Margin = new Thickness(15, 0, 15, 5),
                        HeightRequest = MinEditorHeight,
                        Text = _note.Content,
                        Placeholder = "Sisu",
                        MaxLength = MaxContentLength
                    };
                    _contentEditor.TextChanged += OnContentEditorTextChanged;

                    OnContentEditorTextChanged(_contentEditor, new TextChangedEventArgs(null, _contentEditor.Text));

                    _colorPicker = new Picker
                    {
                        Title = "Värv",
                        ItemsSource = new[] { "Sinine", "Punane", "Roheline", "Kollane" },
                        SelectedIndex = (int)_note.NoteColor,
                        Margin = new Thickness(15, 5),
                        HorizontalOptions = LayoutOptions.Fill
                    };
                    _colorPicker.SelectedIndexChanged += OnColorPickerChanged;

                    stackLayout.Children.Add(_titleEntry);
                    stackLayout.Children.Add(line);
                    stackLayout.Children.Add(_contentEditor);
                    stackLayout.Children.Add(_colorPicker);
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
                        TextColor = Color.FromArgb("#333"),
                        HorizontalOptions = LayoutOptions.Start,
                        Padding = new Thickness(15, 0, 0, 0),
                        LineBreakMode = LineBreakMode.TailTruncation,
                        Text = _note.Title
                    };
                    var timeLabel = new Label
                    {
                        FontSize = 12,
                        TextColor = Color.FromArgb("#666"),
                        HorizontalOptions = LayoutOptions.End,
                        VerticalOptions = LayoutOptions.Start,
                        Padding = new Thickness(5, 3, 15, 0),
                        Text = _note.ModifyTime?.ToString("HH:mm")
                    };
                    grid.Add(titleLabel, 0, 0);
                    grid.Add(timeLabel, 1, 0);

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
                        Padding = new Thickness(15, 0, 15, 5),
                        Text = _note.Content
                    };

                    stackLayout.Children.Add(grid);
                    stackLayout.Children.Add(line);
                    stackLayout.Children.Add(contentLabel);
                }

                _modernBorder.Content = stackLayout;
                return _modernBorder;
            }
            catch (Exception ex)
            {
                return new Label { Text = "Error: ModernView creation failed", TextColor = Colors.Red };
            }
        }

        private async void OnContentEditorTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_contentEditor == null) return;

            try
            {
                if (e.NewTextValue.Length > MaxContentLength)
                {
                    _contentEditor.Text = e.OldTextValue;
                    await Application.Current.MainPage.DisplayAlert("Viga", $"Sisu ei tohi ületada {MaxContentLength} tähemärki.", "OK");
                    return;
                }

                int lineCount = string.IsNullOrEmpty(e.NewTextValue) ? 1 : e.NewTextValue.Split('\n').Length;
                const int maxLines = 20;
                if (lineCount > maxLines)
                {
                    _contentEditor.Text = e.OldTextValue;
                    await Application.Current.MainPage.DisplayAlert("Viga", $"Sisu ei tohi ületada {maxLines} rida.", "OK");
                    return;
                }

                double newHeight = Math.Max(MinEditorHeight, lineCount * LineHeight);
                newHeight = Math.Min(newHeight, MaxEditorHeight);

                _contentEditor.HeightRequest = newHeight;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnContentEditorTextChanged error: {ex.Message}");
            }
        }

        private void OnColorPickerChanged(object sender, EventArgs e)
        {
            if (_isEditing && _colorPicker.SelectedIndex >= 0)
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