using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NutikasPaevik.Database;

namespace NutikasPaevik
{
    public class CalendarViewModel : INotifyPropertyChanged
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private DateTime _currentDate;
        private ObservableCollection<Event> _allEvents;
        private ObservableCollection<MonthViewModel> _months;
        private MonthViewModel _currentMonth;

        private bool _needsRefresh = true;
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public ObservableCollection<MonthViewModel> Months
        {
            get => _months;
            set
            {
                _months = value;
                OnPropertyChanged(nameof(Months));
            }
        }

        public MonthViewModel CurrentMonth
        {
            get => _currentMonth;
            set
            {
                if (_currentMonth != value)
                {
                    _currentMonth = value;
                    OnPropertyChanged(nameof(CurrentMonth));
                    LoadEventsForMonth(_currentMonth);
                }
            }
        }

        public CalendarViewModel(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _currentDate = DateTime.Now;
            _allEvents = new ObservableCollection<Event>();
            Months = new ObservableCollection<MonthViewModel>();
            IsLoading = true;
        }

        public async Task LoadCalendarAsync()
        {
            IsLoading = true;
            await LoadAllEventsAsync();

            if (_needsRefresh || !Months.Any()) // month collection create
            {
                var months = new ObservableCollection<MonthViewModel>();
                var startMonth = _currentDate.AddMonths(-2);
                for (int i = 0; i < 5; i++)
                {
                    var month = new MonthViewModel(startMonth.AddMonths(i));
                    LoadEventsForMonth(month);
                    months.Add(month);
                }
                Months = months;
            }
            else
            {
                // reloading only events in months
                foreach (var month in Months)
                {
                    LoadEventsForMonth(month);
                }
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                CurrentMonth = Months.FirstOrDefault(m => m.MonthYear == _currentDate.ToString("MMMM yyyy", CultureInfo.CurrentCulture));
                IsLoading = false;
            });

            _needsRefresh = false;
        }

        public void MarkForRefresh()
        {
            _needsRefresh = true;
        }

        private async Task LoadAllEventsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            try
            {
                var userId = UserService.Instance.UserId;
                var startDate = DateTime.Now.AddMonths(-2);
                var events = await dbContext.Events
                    .Where(e => e.UserID == userId && e.Status != "deleted" && e.Date >= startDate)
                    .ToListAsync();
                _allEvents = new ObservableCollection<Event>(events);
                System.Diagnostics.Debug.WriteLine($"LoadAllEventsAsync: Loaded {_allEvents.Count} events for UserID={userId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadAllEventsAsync Error: {ex.Message}");
                await Application.Current?.MainPage?.DisplayAlert("Viga", $"Ei saanud sündmusi laadida: {ex.Message}", "OK");
            }
        }

        private void LoadEventsForMonth(MonthViewModel current_month)
        {
            if (current_month == null) return;

            foreach (var day in current_month.CalendarDays)
            {
                day.Events = new ObservableCollection<Event>(
                    _allEvents.Where(e => e.Date.Month == day.Date.Month && e.Date.Year == day.Date.Year && e.Date.Day == day.Date.Day)
                );
            }
        }

        public void RefreshEventsForCurrentMonth()
        {
            LoadEventsForMonth(CurrentMonth);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}