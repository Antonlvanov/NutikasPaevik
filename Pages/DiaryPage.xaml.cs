using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using NutikasPaevik.Database;
using NutikasPaevik.Database.Models;
using NutikasPaevik.Enums;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NutikasPaevik
{
    public partial class DiaryPage : ContentPage
    {
        private readonly DiaryViewModel _viewModel;

        public DiaryPage()
        {
            try
            {
                InitializeComponent();
                _viewModel = App.Services.GetRequiredService<DiaryViewModel>();
                BindingContext = _viewModel;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DiaryPage Initialization Error: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.RefreshNotes();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_viewModel.NotesForSelectedDate != null)
                {
                    var index = _viewModel.NotesByDate.IndexOf(_viewModel.NotesForSelectedDate);
                    if (index >= 0 && NotesCarouselView != null)
                    {
                        NotesCarouselView.ScrollTo(index, animate: false);
                        Debug.WriteLine($"CarouselView синхронизирован с индексом {index} для даты {_viewModel.NotesForSelectedDate.Date:dd MMMM yyyy}");
                    }
                }
            });
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = true;
        }
    }
}