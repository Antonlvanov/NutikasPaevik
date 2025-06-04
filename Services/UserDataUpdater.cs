using NutikasPaevik.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NutikasPaevik
{
    public partial class UserService
    {
        public async Task<bool> UpdateUserDetailsAsync(string newUsername, string newEmail, string newPassword = null)
        {
            if (!IsUserLoggedIn() || CurrentUser == null)
            {
                System.Diagnostics.Debug.WriteLine("UpdateUserDetailsAsync: User not logged in.");
                return false;
            }

            try
            {
                App.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);

                var updateData = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(newUsername)) updateData["username"] = newUsername;
                if (!string.IsNullOrEmpty(newEmail)) updateData["email"] = newEmail;
                if (!string.IsNullOrEmpty(newPassword)) updateData["password"] = newPassword;

                var response = await App.HttpClient.PutAsJsonAsync("http://paevik.antonivanov23.thkit.ee/user", updateData);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadFromJsonAsync<UpdateUserResponse>();
                    if (responseData?.User != null)
                    {
                        CurrentUser.Username = responseData.User.Username;
                        CurrentUser.Email = responseData.User.Email;
                        Preferences.Set("current_user", JsonSerializer.Serialize(CurrentUser));
                        Preferences.Set("user_email", CurrentUser.Email);

                        if (!string.IsNullOrEmpty(responseData.Token))
                        {
                            AuthToken = responseData.Token;
                            await SecureStorage.SetAsync("auth_token", AuthToken);
                            App.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
                        }

                        System.Diagnostics.Debug.WriteLine($"User updated: ID={CurrentUser.Id}, Username={CurrentUser.Username}, Email={CurrentUser.Email}");
                        return true;
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Failed to update user. Status: {response.StatusCode}, Content: {errorContent}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateUserDetailsAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateEmailAsync(string newEmail)
        {
            if (!IsUserLoggedIn() || CurrentUser == null)
            {
                System.Diagnostics.Debug.WriteLine("UpdateEmailAsync: User not logged in.");
                return false;
            }

            try
            {
                App.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);

                var updateData = new { email = newEmail };
                var response = await App.HttpClient.PatchAsJsonAsync("http://paevik.antonivanov23.thkit.ee/user/email", updateData);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadFromJsonAsync<UpdateUserResponse>();
                    if (responseData?.User != null)
                    {
                        CurrentUser.Email = responseData.User.Email;
                        Preferences.Set("current_user", JsonSerializer.Serialize(CurrentUser));
                        Preferences.Set("user_email", CurrentUser.Email);

                        if (!string.IsNullOrEmpty(responseData.Token))
                        {
                            AuthToken = responseData.Token;
                            await SecureStorage.SetAsync("auth_token", AuthToken);
                            App.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
                        }

                        System.Diagnostics.Debug.WriteLine($"Email updated: {CurrentUser.Email}");
                        return true;
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Failed to update email. Status: {response.StatusCode}, Content: {errorContent}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateEmailAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUsernameAsync(string newUsername)
        {
            if (!IsUserLoggedIn() || CurrentUser == null)
            {
                System.Diagnostics.Debug.WriteLine("UpdateUsernameAsync: User not logged in.");
                return false;
            }

            try
            {
                App.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);

                var updateData = new { username = newUsername };
                var response = await App.HttpClient.PatchAsJsonAsync("http://paevik.antonivanov23.thkit.ee/user/username", updateData);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadFromJsonAsync<UpdateUserResponse>();
                    if (responseData?.User != null)
                    {
                        CurrentUser.Username = responseData.User.Username;
                        Preferences.Set("current_user", JsonSerializer.Serialize(CurrentUser));

                        System.Diagnostics.Debug.WriteLine($"Username updated: {CurrentUser.Username}");
                        return true;
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Failed to update username. Status: {response.StatusCode}, Content: {errorContent}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateUsernameAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdatePasswordAsync(string oldPassword, string newPassword)
        {
            if (!IsUserLoggedIn() || CurrentUser == null)
            {
                System.Diagnostics.Debug.WriteLine("UpdatePasswordAsync: User not logged in.");
                return false;
            }

            try
            {
                App.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);

                var updateData = new { oldPassword, newPassword };
                var response = await App.HttpClient.PatchAsJsonAsync("http://paevik.antonivanov23.thkit.ee/user/password", updateData);

                if (response.IsSuccessStatusCode)
                {
                    Preferences.Set("user_password", newPassword);
                    System.Diagnostics.Debug.WriteLine("Password updated successfully.");
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Failed to update password. Status: {response.StatusCode}, Content: {errorContent}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePasswordAsync error: {ex.Message}");
                return false;
            }
        }
    }

    public class UpdateUserResponse
    {
        public User User { get; set; }
        public string Token { get; set; }
    }
}
