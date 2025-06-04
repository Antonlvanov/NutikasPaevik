using Microsoft.Maui.Controls;
using NutikasPaevik.Database;

namespace NutikasPaevik
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            System.Diagnostics.Debug.WriteLine("AppShell constructor started.");
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("InitializeComponent completed.");

            Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
            Routing.RegisterRoute(nameof(DiaryPage), typeof(DiaryPage));
            Routing.RegisterRoute(nameof(PlannerPage), typeof(PlannerPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(AccountPage), typeof(AccountPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(CalendarPage), typeof(CalendarPage));
        }

        private async void OnProfileClicked(object sender, EventArgs e)
        {
            await GoToAsync(nameof(AccountPage));
            FlyoutIsPresented = false;
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await GoToAsync(nameof(SettingsPage));
            FlyoutIsPresented = false;
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Kinnitus", "Kas soovid välja logida?", "Jah", "Ei");
            if (confirm)
            {
                await UserService.Instance.ClearUserAsync(); 
                App.HttpClient.DefaultRequestHeaders.Authorization = null;
                await GoToAsync(nameof(LoginPage));
                FlyoutIsPresented = false;
            }
        }
    }
}