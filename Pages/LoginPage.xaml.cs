using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using NutikasPaevik.Database.Models;
using NutikasPaevik.Services;

namespace NutikasPaevik
{
    public partial class LoginPage : ContentPage
    {
        private ActivityIndicator _activityIndicator;

        public LoginPage()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            _activityIndicator = new ActivityIndicator
            {
                IsRunning = false,
                Color = Colors.Blue,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            var layout = this.Content as StackLayout;
            if (layout != null)
            {
                layout.Children.Add(_activityIndicator);
            }
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnLoginClicked started.");
            if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Viga", "Täitke kõik väljad", "OK");
                return;
            }

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("Viga", "Internetiühendus puudub", "OK");
                return;
            }

            var model = new
            {
                email = EmailEntry.Text,
                password = PasswordEntry.Text
            };

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            try
            {
                _activityIndicator.IsRunning = true;
                await MainThread.InvokeOnMainThreadAsync(() => _activityIndicator.IsVisible = true);

                await UserService.Instance.ClearUserAsync();

                var response = await App.HttpClient.PostAsync("http://paevik.antonivanov23.thkit.ee/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, options);

                    await UserService.Instance.SetUserAsync(loginResponse.User, loginResponse.Token, PasswordEntry.Text);
                    App.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);

                    await SyncService.DownloadEventsFromServerAsync();
                    await SyncService.DownloadNotesFromServerAsync();
                    await Task.Delay(1000);
;
                    App.SwitchToMainApp();
                }
                else
                {
                    await DisplayAlert("Viga", $"Sisselogimine ebaõnnestus", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnLoginClicked error: {ex.Message}");
                await DisplayAlert("Viga", $"Sisselogimisel ilmnes viga: {ex.Message}", "OK");
            }
            finally
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _activityIndicator.IsRunning = false;
                    _activityIndicator.IsVisible = false;
                });
                System.Diagnostics.Debug.WriteLine("OnLoginClicked finished.");
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnRegisterClicked started.");
            await Navigation.PushAsync(new RegisterPage());
            System.Diagnostics.Debug.WriteLine("OnRegisterClicked finished.");
        }

        public HttpClient GetHttpClient()
        {
            return App.HttpClient;
        }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public User User { get; set; }
    }
}