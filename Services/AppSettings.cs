using NutikasPaevik.Enums;
using NutikasPaevik.Services;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace NutikasPaevik
{
    public class AppSettings : INotifyPropertyChanged
    {
        private static AppSettings _instance;
        private NoteDisplayStyle _noteDisplayStyle;
        private Enums.AppTheme _selectedTheme = Enums.AppTheme.Dark;

        private AppSettings()
        {
            LoadSettings();
        }

        public static AppSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AppSettings();
                    Debug.WriteLine("AppSettings instance created.");
                }
                return _instance;
            }
        }

        public NoteDisplayStyle NoteDisplayStyle
        {
            get => _noteDisplayStyle;
            set
            {
                if (_noteDisplayStyle != value)
                {
                    _noteDisplayStyle = value;
                    Debug.WriteLine($"NoteDisplayStyle changed to: {_noteDisplayStyle}");
                    SaveSettings();
                    OnPropertyChanged(nameof(NoteDisplayStyle));
                }
            }
        }

        public Enums.AppTheme SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (_selectedTheme != value)
                {
                    _selectedTheme = value;
                    Debug.WriteLine($"SelectedTheme changed to: {_selectedTheme}");
                    SaveSettings();
                    UpdateApplicationTheme();
                    OnPropertyChanged(nameof(SelectedTheme));
                }
            }
        }

        public void UpdateApplicationTheme()
        {
            var app = Application.Current;
            if (app == null)
            {
                Debug.WriteLine("Application.Current is null, cannot update theme.");
                return;
            }

            app.Resources.Clear();
            app.Resources.MergedDictionaries.Clear();

            ResourceDictionary themeResources = _selectedTheme switch
            {
                Enums.AppTheme.Light => new LightTheme(),
                Enums.AppTheme.Dark => new DarkTheme(),
                Enums.AppTheme.Custom => new CustomTheme(),
                _ => new DarkTheme()
            };

            foreach (var resource in themeResources)
            {
                Debug.WriteLine($"Adding resource: {resource.Key}");
                app.Resources.Add(resource.Key, resource.Value);
            }
            Debug.WriteLine($"Application theme updated to: {_selectedTheme}");

            if (app.MainPage is Shell shell)
            {
                shell.FlyoutIsPresented = true;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    shell.FlyoutIsPresented = false;
                });
            }
        }

        public void SetTheme(Enums.AppTheme theme)
        {
            SelectedTheme = theme;
        }

        private void LoadSettings()
        {
            LoadNoteDisplayStyle();
            LoadTheme();
        }

        private void LoadNoteDisplayStyle()
        {
            var styleString = Preferences.Get("NoteDisplayStyle", NoteDisplayStyle.Modern.ToString());
            if (Enum.TryParse<NoteDisplayStyle>(styleString, out var style))
            {
                _noteDisplayStyle = style;
            }
            else
            {
                _noteDisplayStyle = NoteDisplayStyle.Modern;
            }
            Debug.WriteLine($"Loaded NoteDisplayStyle: {_noteDisplayStyle}");
        }

        private void LoadTheme()
        {
            var themeString = Preferences.Get("SelectedTheme", Enums.AppTheme.Dark.ToString());
            if (Enum.TryParse<Enums.AppTheme>(themeString, out var theme))
            {
                _selectedTheme = theme;
            }
            else
            {
                _selectedTheme = Enums.AppTheme.Dark;
            }
            Debug.WriteLine($"Loaded SelectedTheme: {_selectedTheme}");
        }

        private void SaveSettings()
        {
            Preferences.Set("NoteDisplayStyle", _noteDisplayStyle.ToString());
            Preferences.Set("SelectedTheme", _selectedTheme.ToString());
            Debug.WriteLine("Settings saved.");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Debug.WriteLine($"Property changed: {propertyName}");
        }
    }
}