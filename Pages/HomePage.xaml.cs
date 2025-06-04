using Microsoft.Maui.Controls;
using NutikasPaevik.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NutikasPaevik
{
    public partial class HomePage : ContentPage
    {
        private readonly HomePageViewModel _viewModel;

        public HomePage()
        {
            InitializeComponent();
            _viewModel = App.Services.GetRequiredService<HomePageViewModel>();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadData();
        }
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = true;
        }
    }
}