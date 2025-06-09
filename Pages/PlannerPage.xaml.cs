using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;

namespace NutikasPaevik
{
    public partial class PlannerPage : ContentPage
    {
        private readonly PlannerViewModel _viewModel;

        public PlannerPage(DateTime? selectedDate = null)
        {
            if (selectedDate != null) { Debug.WriteLine("DATE AQUIRED"); }
            try
            {
                InitializeComponent();
                _viewModel = new PlannerViewModel(App.Services.GetRequiredService<IServiceScopeFactory>(), selectedDate);
                BindingContext = _viewModel;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PlannerPage Initialization Error: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext != _viewModel)
            {
                BindingContext = _viewModel;
                Debug.WriteLine("PlannerPage: BindingContext reset to _viewModel.");
            }
            await _viewModel.RefreshEvents();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_viewModel.EventsForSelectedDate != null)
                {
                    var index = _viewModel.EventsByDate.IndexOf(_viewModel.EventsForSelectedDate);
                    if (index >= 0 && EventsCarouselView != null)
                    {
                        EventsCarouselView.ScrollTo(index, animate: false);
                    }
                    else
                    {
                        Debug.WriteLine("PlannerPage: invalid index or CarouselView is null.");
                    }
                }
                else
                {
                    Debug.WriteLine("PlannerPage: EventsForSelectedDate is null, cannot synchronize CarouselView.");
                }
            });
        }
    }
}