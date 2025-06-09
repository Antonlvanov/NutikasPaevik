using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using NutikasPaevik.Database;
using NutikasPaevik.Services;
using System;
using System.Threading.Tasks;

namespace NutikasPaevik
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private async void OnSyncEventsClientToServerClicked(object sender, EventArgs e)
        {
            try
            {
                await SyncService.UploadEventsToServerAsync();
                await DisplayAlert("Info", "Märkmed saadetud serverile", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Viga", $"Sünkroniseerimine ebaõnnestus: {ex.Message}", "OK");
            }
        }

        private async void OnSyncEventsServerToClientClicked(object sender, EventArgs e)
        {
            try
            {
                await SyncService.DownloadEventsFromServerAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Viga", $"Sünkroniseerimine ebaõnnestus: {ex.Message}", "OK");
            }
        }

        private async void OnSyncNotesClientToServerClicked(object sender, EventArgs e)
        {
            try
            {
                await SyncService.UploadNotesToServerAsync();
                await DisplayAlert("Info", "Märkmed on saddetud serverile", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Viga", $"Sünkroniseerimine ebaõnnestus: {ex.Message}", "OK");
            }
        }

        private async void OnSyncNotesServerToClientClicked(object sender, EventArgs e)
        {
            try
            {
                await SyncService.DownloadNotesFromServerAsync();
                await DisplayAlert("Info", "Märkmed alla laaditud serverist", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Viga", $"Sünkroniseerimine ebaõnnestus: {ex.Message}", "OK");
            }
        }

        private async void OnClearLocalDatabaseClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Kinnitus", "Kas soovid kindlasti puhastada kohaliku andmebaasi?", "Jah", "Ei");
            if (confirm)
            {
                try
                {
                    await SyncService.ClearLocalDatabaseAsync();
                    await DisplayAlert("Info", "Kohalik andmebaas on puhastatud", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Viga", $"Puhastamine ebaõnnestus: {ex.Message}", "OK");
                }
            }
        }

        private async void OnClearLocalUserEventsClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Kinnitus", "Kas soovid kindlasti puhastada oma kohalikud märkmed?", "Jah", "Ei");
            if (confirm)
            {
                try
                {
                    await SyncService.ClearLocalEventsForCurrentUserAsync();
                    await DisplayAlert("Info", "Sinu kohalikud märkmed on puhastatud", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Viga", $"Puhastamine ebaõnnestus: {ex.Message}", "OK");
                }
            }
        }

        private async void OnClearLocalUserNotesClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Kinnitus", "Kas soovid kindlasti puhastada oma kohalikud märkmed?", "Jah", "Ei");
            if (confirm)
            {
                try
                {
                    await SyncService.ClearLocalNotesForCurrentUserAsync();
                    await DisplayAlert("Info", "Sinu kohalikud märkmed on puhastatud", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Viga", $"Puhastamine ebaõnnestus: {ex.Message}", "OK");
                }
            }
        }
    }
}