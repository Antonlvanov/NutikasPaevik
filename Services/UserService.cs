using NutikasPaevik.Database.Models;
using System.Text.Json;
using Microsoft.Maui.Storage;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http.Json;

namespace NutikasPaevik
{
    public partial class UserService
    {
        private static UserService _instance;
        public static UserService Instance => _instance ??= new UserService();

        public User? CurrentUser { get; private set; }
        public string? AuthToken { get; private set; }
        public int UserId => CurrentUser?.Id ?? 0;
        public DateTime? LastSyncTime { get; private set; }

        private UserService() { }
        public class UpdateUserResponse
        {
            public User User { get; set; }
            public string Token { get; set; }
        }
        public async Task<bool> IsTokenValidAsync()
        {
            if (string.IsNullOrEmpty(AuthToken)) return false;
            try
            {
                App.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
                var response = await App.HttpClient.GetAsync("http://paevik.antonivanov23.thkit.ee/notes");
                System.Diagnostics.Debug.WriteLine($"Token is valid");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IsTokenValidAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task SetUserAsync(User user, string token, string password)
        {
            try
            {
                CurrentUser = user ?? throw new ArgumentNullException(nameof(user));
                AuthToken = token;
                await SecureStorage.SetAsync("auth_token", token);
                Preferences.Set("current_user", JsonSerializer.Serialize(user));
                Preferences.Set("user_id", user.Id.ToString());
                Preferences.Set("user_password", password);
                Preferences.Set("user_email", user.Email);
                System.Diagnostics.Debug.WriteLine($"User set: ID={user.Id}, Username={user.Email}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetUserAsync error: {ex.Message}");
                await ClearUserAsync();
            }
        }

        public async Task AutoLogin()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("AutoLogin started.");

                var userJson = Preferences.Get("current_user", null);
                var password = Preferences.Get("user_password", null);
                if (string.IsNullOrEmpty(userJson) || string.IsNullOrEmpty(password))
                {
                    System.Diagnostics.Debug.WriteLine("AutoLogin failed: No user credentials found.");
                    return;
                }

                var user = JsonSerializer.Deserialize<User>(userJson);
                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    System.Diagnostics.Debug.WriteLine("AutoLogin failed: Invalid user data.");
                    return;
                }

                if (Microsoft.Maui.Networking.Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    System.Diagnostics.Debug.WriteLine("AutoLogin failed: No internet connection.");
                    return;
                }

                var model = new
                {
                    email = user.Email,
                    password
                };
                var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

                var response = await App.HttpClient.PostAsync("http://paevik.antonivanov23.thkit.ee/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, options);

                    await SetUserAsync(loginResponse.User, loginResponse.Token, password);
                    App.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
                    System.Diagnostics.Debug.WriteLine("AutoLogin successful.");
                }
                else
                {
                    await ClearUserAsync();
                    System.Diagnostics.Debug.WriteLine($"AutoLogin failed: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AutoLogin error: {ex.Message}");
            }
        }

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

        public async Task ClearUserAsync()
        {
            try
            {
                CurrentUser = null;
                AuthToken = null;
                SecureStorage.Remove("auth_token");
                Preferences.Remove("current_user");
                Preferences.Remove("user_password");
                Preferences.Remove("user_id");
                Preferences.Remove("user_email");
                System.Diagnostics.Debug.WriteLine("User cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ClearUserAsync error: {ex.Message}");
            }
        }

        public async Task ClearTokenAsync()
        {
            try
            {
                AuthToken = null;
                SecureStorage.Remove("auth_token");
                System.Diagnostics.Debug.WriteLine("Token cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ClearUserAsync error: {ex.Message}");
            }
        }

        public void UpdateLastSyncTime(DateTime syncTime)
        {
            LastSyncTime = syncTime;
            if (UserId != 0)
            {
                Preferences.Set($"last_sync_time_{UserId}", syncTime.ToString("yyyy-MM-dd HH:mm:ss"));
                System.Diagnostics.Debug.WriteLine($"LastSyncTime updated: {syncTime}");
            }
        }

        public bool IsUserLoggedIn()
        {
            return CurrentUser != null && !string.IsNullOrEmpty(AuthToken);
        }
    }
}