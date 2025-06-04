using CommunityToolkit.Maui.Views;
using Microsoft.EntityFrameworkCore;
using NutikasPaevik.Database;
using NutikasPaevik.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using System.Collections.Specialized;

namespace NutikasPaevik
{
    public class DiaryViewModel : INotifyPropertyChanged
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private ObservableCollection<Note> _notes;
        private ObservableCollection<NotesByDate> _notesByDate;
        private NotesByDate _notesForSelectedDate;
        private DateTime _selectedDate;
        private bool _needsRefresh;

        // all notes
        public ObservableCollection<Note> Notes
        {
            get => _notes;
            set
            {
                if (_notes != null)
                    _notes.CollectionChanged -= Notes_CollectionChanged;
                _notes = value;
                _notes.CollectionChanged += Notes_CollectionChanged;
                OnPropertyChanged(nameof(Notes));
                UpdateNotesByDate();
            }
        }

        // all notes with dates
        public ObservableCollection<NotesByDate> NotesByDate
        {
            get => _notesByDate;
            set
            {
                _notesByDate = value;
                OnPropertyChanged(nameof(NotesByDate));
            }
        }

        // current selected notes for selected date
        public NotesByDate NotesForSelectedDate
        {
            get => _notesForSelectedDate;
            set
            {
                if (_notesForSelectedDate != value)
                {
                    _notesForSelectedDate = value;
                    OnPropertyChanged(nameof(NotesForSelectedDate));
                    SelectedDateForCarousel = value?.Date ?? DateTime.Today;
                }
            }
        }

        // current selected date for carousel 
        public DateTime SelectedDateForCarousel
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged(nameof(SelectedDateForCarousel));
                    NotesForSelectedDate = NotesByDate.FirstOrDefault(n => n.Date == value);
                }
            }
        }

        // command
        public ICommand AddNoteCommand { get; }
        public ICommand DeleteNoteCommand { get; }
        public ICommand OpenNoteCommand { get; }

        // uk
        public DiaryViewModel(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _notes = new ObservableCollection<Note>();
            _notes.CollectionChanged += Notes_CollectionChanged;
            _notesByDate = new ObservableCollection<NotesByDate>();
            AddNoteCommand = new Command(async () => await ShowNotePopup());
            DeleteNoteCommand = new Command<Note>(async (note) => await DeleteNote(note));
            OpenNoteCommand = new Command<Note>(async (note) => await ShowNoteFullPopup(note));
            _selectedDate = DateTime.Today;
            _needsRefresh = true;
            AppSettings.Instance.PropertyChanged += OnAppSettingsChanged;
        }

        // target upd for changing notes cases
        private void Notes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var newNote = (Note)e.NewItems[0];
                var date = newNote.CreationTime.Date;
                var notesByDate = NotesByDate.FirstOrDefault(n => n.Date == date);
                if (notesByDate != null)
                {
                    notesByDate.Notes.Add(newNote);
                    UpdateColumns(notesByDate);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var oldNote = (Note)e.OldItems[0];
                var date = oldNote.CreationTime.Date;
                var notesByDate = NotesByDate.FirstOrDefault(n => n.Date == date);
                if (notesByDate != null)
                {
                    notesByDate.Notes.Remove(oldNote);
                    UpdateColumns(notesByDate);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                var oldNote = (Note)e.OldItems[0];
                var newNote = (Note)e.NewItems[0];
                var newDate = DateTime.Now;
                var notesByDate = NotesByDate.FirstOrDefault(n => n.Date == oldNote.CreationTime.Date);
                if (notesByDate != null)
                {
                    var noteIndex = notesByDate.Notes.IndexOf(oldNote);
                    newNote.ModifyTime = newDate;
                    notesByDate.Notes[noteIndex] = newNote;
                    UpdateColumns(notesByDate);
                }
            }
        }

        // if settings changed updating columns // obsolete
        private void OnAppSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppSettings.NoteDisplayStyle))
            {
                // collectin not recreating rn
                foreach (var group in NotesByDate)
                {
                    UpdateColumns(group);
                }
            }
        }

        // 
        public void SelectClosestDate()
        {
            if (NotesByDate.Any())
            {
                NotesForSelectedDate = NotesByDate.OrderBy(d => Math.Abs((d.Date - DateTime.Today).TotalDays)).First();
                Debug.WriteLine($"SelectClosestDate: Selected {NotesForSelectedDate.Date:dd MMMM yyyy}");
            }
        }

        // refresh with condition
        public async Task RefreshNotes()
        {
            if (_needsRefresh)
            {
                LoadNotesFromDatabase();
                _needsRefresh = false;
            }
            SelectClosestDate();
        }

        // loading from db into main notes collection
        public void LoadNotesFromDatabase()
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            try
            {
                var userId = UserService.Instance.UserId;
                if (userId == 0)
                {
                    Debug.WriteLine("LoadNotesFromDatabase: No valid user_id found, loading no notes.");
                    Notes = new ObservableCollection<Note>();
                    return;
                }
                Notes = new ObservableCollection<Note>(dbContext.Notes.Where(n => n.UserID == userId).ToList());
                Debug.WriteLine($"LoadNotesFromDatabase: Loaded {Notes.Count} notes for UserID={userId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadNotesFromDatabase Error: {ex.Message}");
                Application.Current?.MainPage?.DisplayAlert("Viga", $"Ei saanud märkmeid laadida: {ex.Message}", "OK");
            }
        }

        // sorting by columns
        private void UpdateColumns(NotesByDate notesByDate)
        {
            try
            {
                var filteredNotes = notesByDate.Notes.ToList();
                notesByDate.LeftColumnNotes.Clear();
                notesByDate.RightColumnNotes.Clear();
                foreach (var note in filteredNotes)
                {
                    if (notesByDate.LeftColumnNotes.Count <= notesByDate.RightColumnNotes.Count)
                        notesByDate.LeftColumnNotes.Add(note);
                    else
                        notesByDate.RightColumnNotes.Add(note);
                }
                Debug.WriteLine($"UpdateColumnsFromNotes: Date={notesByDate.Date:dd MMMM yyyy}, Left={notesByDate.LeftColumnNotes.Count}, Right={notesByDate.RightColumnNotes.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateColumnsFromNotes Error: {ex.Message}");
                Application.Current?.MainPage?.DisplayAlert("Viga", $"Ei saanud märkmeid laadida: {ex.Message}", "OK");
            }
        }

        // sorting all notes
        public void UpdateNotesByDate()
        {
            try
            {
                Debug.WriteLine($"UpdateNotesByDate: Notes count={_notes.Count}");
                var groupedNotes = _notes
                    .GroupBy(n => n.CreationTime.Date)
                    .Select(g => new NotesByDate
                    {
                        Date = g.Key,
                        Notes = new ObservableCollection<Note>(g),
                        LeftColumnNotes = new ObservableCollection<Note>(),
                        RightColumnNotes = new ObservableCollection<Note>()
                    })
                    .OrderBy(g => g.Date)
                    .ToList();

                // adding current date if not exist in collection
                var today = DateTime.Today;
                if (!groupedNotes.Any(g => g.Date == today))
                {
                    groupedNotes.Add(new NotesByDate
                    {
                        Date = today,
                        Notes = new ObservableCollection<Note>(),
                        LeftColumnNotes = new ObservableCollection<Note>(),
                        RightColumnNotes = new ObservableCollection<Note>()
                    });
                }

                foreach (var group in groupedNotes)
                {
                    UpdateColumns(group);
                }

                NotesByDate = new ObservableCollection<NotesByDate>(groupedNotes);
                Debug.WriteLine($"Updated NotesByDate: NotesByDate count={NotesByDate.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateNotesByDate Error: {ex.Message}");
                Application.Current?.MainPage?.DisplayAlert("Viga", $"Ei saanud märkmeid grupeerida: {ex.Message}", "OK");
            }
        }

        // notes view / edit popup
        public async Task ShowNoteFullPopup(Note note)
        {
            var popup = new NoteFullPopup(note, this);
            await Application.Current.MainPage.ShowPopupAsync(popup);
        }

        // popup add
        public async Task ShowNotePopup()
        {
            try
            {
                var popup = new AddNotePopup();
                var result = await Application.Current.MainPage.ShowPopupAsync(popup);
                if (result is Note note)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    dbContext.Notes.Add(note);
                    await dbContext.SaveChangesAsync();
                    Notes.Add(note);
                    try
                    {
                        await SyncService.UploadNotesToServerAsync();
                    }
                    catch
                    {
                        await Application.Current.MainPage.DisplayAlert("Info", "Saved locally", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShowNotePopup Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Viga", $"Ei saanud märkmeid salvestada: {ex.Message}", "OK");
            }
        }

        // delete
        public async Task DeleteNote(Note note)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                Notes.Remove(note);
                dbContext.Notes.Remove(note);
                await dbContext.SaveChangesAsync();
                await SyncService.DeleteNoteAsync(note.SyncId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteNote Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Viga", $"Ei saanud märkmeid kustutada: {ex.Message}", "OK");
            }
        }

        // setting upd flag
        public void MarkForRefresh()
        {
            _needsRefresh = true;
        }

        // property changed handler
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class NotesByDate
    {
        public DateTime Date { get; set; }
        public ObservableCollection<Note> Notes { get; set; }
        public ObservableCollection<Note> LeftColumnNotes { get; set; }
        public ObservableCollection<Note> RightColumnNotes { get; set; }
    }
}