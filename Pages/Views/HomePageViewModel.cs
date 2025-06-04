using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NutikasPaevik.Database;
using NutikasPaevik.Enums;
using NutikasPaevik.Services;

namespace NutikasPaevik
{
    public class HomePageViewModel : INotifyPropertyChanged
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private ObservableCollection<Note> _todayNotes;
        private ObservableCollection<Event> _todayTasks;
        private ObservableCollection<Event> _todayEvents;
        private string _statisticsText;
        private bool _needsRefresh = true;
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public ObservableCollection<Note> TodayNotes
        {
            get => _todayNotes;
            set
            {
                _todayNotes = value;
                OnPropertyChanged(nameof(TodayNotes));
            }
        }

        public ObservableCollection<Event> TodayTasks
        {
            get => _todayTasks;
            set
            {
                _todayTasks = value;
                OnPropertyChanged(nameof(TodayTasks));
            }
        }

        public ObservableCollection<Event> TodayEvents
        {
            get => _todayEvents;
            set
            {
                _todayEvents = value;
                OnPropertyChanged(nameof(TodayEvents));
            }
        }

        public string StatisticsText
        {
            get => _statisticsText;
            set
            {
                _statisticsText = value;
                OnPropertyChanged(nameof(StatisticsText));
            }
        }

        public HomePageViewModel(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _todayNotes = new ObservableCollection<Note>();
            _todayTasks = new ObservableCollection<Event>();
            _todayEvents = new ObservableCollection<Event>();
            IsLoading = true;
        }

        public void MarkForRefresh()
        {
            _needsRefresh = true;
        }

        public void LoadData()
        {
            if (!_needsRefresh)
            {
                return; 
            }

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            try
            {
                var userId = UserService.Instance.UserId;
                System.Diagnostics.Debug.WriteLine($"LoadDataAsync: UserId = {userId}");

                if (userId <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("LoadDataAsync: Invalid UserId, loading no data.");
                    TodayNotes = new ObservableCollection<Note>();
                    TodayTasks = new ObservableCollection<Event>();
                    TodayEvents = new ObservableCollection<Event>();
                    StatisticsText = "Пользователь не авторизован";
                    return;
                }

                var today = DateTime.Today; 
                System.Diagnostics.Debug.WriteLine($"LoadDataAsync: Loading data for today = {today}");

                TodayNotes = new ObservableCollection<Note>(
                    dbContext.Notes
                        .Where(n => n.UserID == userId && n.CreationTime.Date == today)
                        .ToList()
                );

                TodayTasks = new ObservableCollection<Event>(
                    dbContext.Events
                        .Where(e => e.UserID == userId && e.Status != "deleted" &&
                                    e.Type == EventType.Task && e.Date.Date == today)
                        .OrderBy(e => e.StartTime)
                        .ToList()
                );
                TodayEvents = new ObservableCollection<Event>(
                    dbContext.Events
                        .Where(e => e.UserID == userId && e.Status != "deleted" &&
                                    e.Type == EventType.Event && e.Date.Date == today)
                        .OrderBy(e => e.StartTime)
                        .ToList()
                );

                System.Diagnostics.Debug.WriteLine($"LoadDataAsync: Loaded {TodayNotes.Count} notes, {TodayTasks.Count} tasks, {TodayEvents.Count} events");

                // stats
                var totalNotes = dbContext.Notes.Count(n => n.UserID == userId);
                var totalTasks = dbContext.Events.Count(e => e.UserID == userId &&
                                                                       e.Status != "deleted" &&
                                                                       e.Type == EventType.Task);
                var totalEvents = dbContext.Events.Count(e => e.UserID == userId &&
                                                                        e.Status != "deleted" &&
                                                                        e.Type == EventType.Event);
                StatisticsText = $"Märkmeid kokku: {totalNotes}, ülesandeid: {totalTasks}, sündmusi: {totalEvents}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadDataAsync Error: {ex.Message}");
                Application.Current?.MainPage?.DisplayAlert("Viga", $"Ei saanud andmeid laadida: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}