using Microsoft.Maui.Controls;
using NutikasPaevik.Database;
using System;
using System.Diagnostics;

namespace NutikasPaevik
{
    public partial class CalendarPage : ContentPage
    {
        private readonly CalendarViewModel _viewModel;

        public CalendarPage()
        {
            InitializeComponent(); 
            _viewModel = App.Services.GetRequiredService<CalendarViewModel>();
            BindingContext = _viewModel;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadCalendarAsync();
        }

        private async void OnDayTapped(object sender, EventArgs e)
        {
            if (sender is View view && view.BindingContext is CalendarDay day)
            {
                Debug.WriteLine($"Navigating to PlannerPage for date: {day.Date}");
                await Navigation.PushAsync(new PlannerPage(day.Date));
            }
        }

        private async void OnMenuButtonClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = true;
        }
    }
}