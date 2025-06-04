using Microsoft.Maui.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NutikasPaevik
{
    public partial class AccountPage : ContentPage, INotifyPropertyChanged
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public AccountPage()
        {
            InitializeComponent();
            BindingContext = UserService.Instance.CurrentUser;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (UserService.Instance.CurrentUser == null)
            {
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }

        private async void OnSaveChangesClicked(object sender, EventArgs e)
        {
            if (UserService.Instance.CurrentUser == null)
            {
                StatusMessage = "Volitatud kasutajat pole.";
                return;
            }

            if (string.IsNullOrEmpty(UsernameEntry.Text) || string.IsNullOrEmpty(EmailEntry.Text))
            {
                StatusMessage = "Kasutajanime ja e-posti aadressi väljad ei tohi olla tühjad.";
                return;
            }

            string newPassword = null;
            if (!string.IsNullOrEmpty(NewPasswordEntry.Text))
            {
                if (string.IsNullOrEmpty(OldPasswordEntry.Text))
                {
                    StatusMessage = "Parooli muutmiseks sisestage oma kehtiv parool.";
                    return;
                }
                if (NewPasswordEntry.Text.Length < 8)
                {
                    StatusMessage = "Uus parool peab olema vähemalt kaheksa tähemärki pikk.";
                    return;
                }
                newPassword = NewPasswordEntry.Text;
            }

            IsBusy = true;
            StatusMessage = "Muudatuste salvestamine...";

            try
            {
                var success = await UserService.Instance.UpdateUserDetailsAsync(
                    UsernameEntry.Text,
                    EmailEntry.Text,
                    newPassword);

                if (success)
                {
                    StatusMessage = "Andmed on edukalt uuendatud.";
                    if (newPassword != null)
                    {
                        Preferences.Set("user_password", newPassword);
                        OldPasswordEntry.Text = "";
                        NewPasswordEntry.Text = "";
                    }
                }
                else
                {
                    StatusMessage = "Andmete värskendamine ebaõnnestus.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Viga: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            await UserService.Instance.ClearUserAsync();
            await Shell.Current.GoToAsync("//LoginPage");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}