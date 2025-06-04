using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Maui.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using NutikasPaevik.Database;
using NutikasPaevik.Database.Models;
using NutikasPaevik.Services;
using Event = NutikasPaevik.Database.Event;

namespace NutikasPaevik
{
    public class PlannerViewModel : INotifyPropertyChanged
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private ObservableCollection<Event> _allEvents;
        private ObservableCollection<EventsByDate> _eventsByDate;
        private EventsByDate _eventsForSelectedDate;
        private DateTime _selectedDate;

        public ObservableCollection<Event> Events
        {
            get => _allEvents;
            set
            {
                _allEvents = value;
                OnPropertyChanged(nameof(Events));
                UpdateEventsByDate();
            }
        }

        public ObservableCollection<EventsByDate> EventsByDate
        {
            get => _eventsByDate;
            set
            {
                _eventsByDate = value;
                OnPropertyChanged();
            }
        }

        public EventsByDate EventsForSelectedDate
        {
            get => _eventsForSelectedDate;
            set
            {
                if (_eventsForSelectedDate != value)
                {
                    _eventsForSelectedDate = value;
                    OnPropertyChanged(nameof(EventsForSelectedDate));
                    _selectedDate = value.Date;
                    OnPropertyChanged(nameof(SelectedDate));
                }
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged();
                    EventsForSelectedDate = EventsByDate.FirstOrDefault(d => d.Date.Date == value.Date);
                }
            }
        }

        public ICommand AddEventCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand OpenCalendarCommand { get; }
        public ICommand OpenEventCommand { get; }
        public ICommand DeleteEventCommand { get; }

        public PlannerViewModel(IServiceScopeFactory scopeFactory, DateTime? selectedDate = null)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _eventsByDate = new ObservableCollection<EventsByDate>();
            _allEvents = new ObservableCollection<Event>();
            _selectedDate = selectedDate ?? DateTime.Today;

            AddEventCommand = new Command(async () => await ShowAddEventPopup());
            OpenMenuCommand = new Command(() => Shell.Current.FlyoutIsPresented = true);

            OpenCalendarCommand = new Command(() => Shell.Current.Navigation.PopAsync());
            OpenEventCommand = new Command<Event>(async (ev) => await ShowEventFullPopup(ev));
            DeleteEventCommand = new Command<Event>(async (ev) => await DeleteEvent(ev));

            LoadAllEvents();
            UpdateEventsByDate();
        }

        private void LoadAllEvents()
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            try
            {
                var userId = UserService.Instance.UserId;
                if (userId == 0)
                {
                    Debug.WriteLine("LoadAllEvents: No valid user_id found");
                    _allEvents = new ObservableCollection<Event>();
                    return;
                }

                _allEvents = new ObservableCollection<Event>(
                    dbContext.Events
                        .Where(e => e.UserID == userId && e.Status != "deleted")
                        .ToList()
                );
                Debug.WriteLine($"LoadAllEvents: Loaded {_allEvents.Count} events for UserID={userId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadAllEvents Error: {ex.Message}");
                Application.Current?.MainPage?.DisplayAlert("Viga", $"Ei saanud sündmusi laadida: {ex.Message}", "OK");
            }
        }

        public void UpdateEventsByDate()
        {
            EventsByDate.Clear();

            int range = 30; // later..
            DateTime baseDate = DateTime.Today;

            for (int i = -range; i <= range; i++)
            {
                var date = baseDate.AddDays(i);
                var eventsForDate = _allEvents
                    .Where(e => e.Date.Date == date.Date)
                    .OrderBy(e => e.StartTime)
                    .ToList();
                EventsByDate.Add(new EventsByDate
                {
                    Date = date,
                    Events = new ObservableCollection<Event>(eventsForDate)
                });
            }
            EventsForSelectedDate = EventsByDate.FirstOrDefault(d => d.Date.Date == _selectedDate.Date)
                    ?? new EventsByDate { Date = _selectedDate, Events = new ObservableCollection<Event>() };
            if (!EventsByDate.Any(d => d.Date.Date == _selectedDate.Date))
            {
                EventsByDate.Add(EventsForSelectedDate);
                EventsByDate = new ObservableCollection<EventsByDate>(EventsByDate.OrderBy(n => n.Date));
            }
            Debug.WriteLine($"UpdateEventsByDate: Updated Events by date cound: {EventsByDate.Count} ");
        }

        public async Task ShowAddEventPopup()
        {
            try
            {
                var popup = new AddEventPopup(SelectedDate);
                var result = await Application.Current.MainPage.ShowPopupAsync(popup);
                if (result is Event newEvent)
                {

                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    dbContext.Events.Add(newEvent);
                    await dbContext.SaveChangesAsync();
                    _allEvents.Add(newEvent);
                    UpdateEventsByDate();
                    OnPropertyChanged(nameof(EventsByDate)); //upd ui

                    await SyncService.UploadEventsToServerAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShowAddEventPopup Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Viga", $"Ei saanud sündmust salvestada: {ex.Message}", "OK");
            }
        }

        public async Task ShowEventFullPopup(Event ev)
        {
            var popup = new EventFullPopup(ev, this);
            await Application.Current.MainPage.ShowPopupAsync(popup);
        }

        public async Task DeleteEvent(Event ev)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var eventToDelete = await dbContext.Events.FirstOrDefaultAsync(e => e.SyncId == ev.SyncId);
                if (eventToDelete != null)
                {
                    eventToDelete.Status = "deleted";
                    await dbContext.SaveChangesAsync();
                    _allEvents.Remove(_allEvents.FirstOrDefault(e => e.SyncId == ev.SyncId));
                    UpdateEventsByDate();
                    OnPropertyChanged(nameof(EventsByDate));

                    await SyncService.DeleteEventAsync(ev.SyncId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteEvent Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Viga", $"Ei saanud sündmust kustutada: {ex.Message}", "OK");
            }
        }

        public async Task RefreshEvents()
        {
            LoadAllEvents();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class EventsByDate
    {
        public DateTime Date { get; set; }
        public ObservableCollection<Event> Events { get; set; }
    }
}