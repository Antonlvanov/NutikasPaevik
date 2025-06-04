using NutikasPaevik.Database;
using System;
using System.Collections.ObjectModel;

namespace NutikasPaevik
{
    /// <summary>
    /// creating month collection with calendardies each calendarday contains its own collection with events and their dates obj
    /// </summary>
    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public ObservableCollection<Event> Events { get; set; } = new ObservableCollection<Event>();
    }
    public class MonthViewModel
    {
        public string MonthYear { get; set; }
        public ObservableCollection<CalendarDay> CalendarDays { get; set; } = new ObservableCollection<CalendarDay>();

        public MonthViewModel(DateTime month)
        {
            MonthYear = month.ToString("MMMM yyyy", System.Globalization.CultureInfo.CurrentCulture);
            GenerateCalendarDaysForCurrentMonth(month);
        }

        private void GenerateCalendarDaysForCurrentMonth(DateTime month)
        {
            DateTime firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            int daysToFirst = ((int)firstDayOfMonth.DayOfWeek + 6) % 7; // monday = 0

            DateTime lastDayofprevMonth = firstDayOfMonth.AddDays(-1); // last day of prev month
            for (int i = 0; i < daysToFirst; i++)
            {
                // starting from 1stday
                DateTime date = lastDayofprevMonth.AddDays(- (daysToFirst - 1 - i));
                CalendarDays.Add(new CalendarDay { Date = date, IsCurrentMonth = false });
            }

            // current month
            for (int day = 1; day <= lastDayOfMonth.Day; day++)
            {
                DateTime date = new DateTime(month.Year, month.Month, day);
                CalendarDays.Add(new CalendarDay { Date = date, IsCurrentMonth = true, IsToday = date.Date == DateTime.Today });
            }

            // next month
            DateTime firstDayOfNextMonth = lastDayOfMonth.AddDays(1); // 1st day of prev month
            int totalDays = CalendarDays.Count;
            int nextMonthDay = 1;
            while (totalDays < 42) // 7x6
            {
                DateTime date = firstDayOfNextMonth.AddDays(nextMonthDay - 1);
                CalendarDays.Add(new CalendarDay { Date = date, IsCurrentMonth = false });
                nextMonthDay++;
                totalDays++;
            }
        }
    }
}