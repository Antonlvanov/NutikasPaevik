using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace NutikasPaevik
{
    public partial class RegisterPage : ContentPage
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public RegisterPage()
        {
            InitializeComponent();
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameEntry.Text) || string.IsNullOrWhiteSpace(EmailEntry.Text) 
                || string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Viga", "Täitke kõik väljad", "OK");
                return;
            }

            var model = new
            {
                username = UsernameEntry.Text,
                email = EmailEntry.Text,
                password = PasswordEntry.Text
            };

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.PostAsync("http://paevik.antonivanov23.thkit.ee/register", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Edu", "Registreerimine oli edukas!", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Viga", $"Serveri viga regisreerimisel: {responseContent}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Viga", $"Viga: {ex.Message}", "OK");
            }
        }
    }
}