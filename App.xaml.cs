using NutikasPaevik.Services;

namespace NutikasPaevik
{
    public partial class App : Application
    {
        public static HttpClient HttpClient { get; private set; }
        public static DiaryViewModel DiaryViewModel { get; private set; }
        public static HomePageViewModel HomePageViewModel { get; private set; }
        public static CalendarViewModel CalendarViewModel { get; private set; }
        public static IServiceProvider Services { get; set; }
        public App()
        {
            InitializeComponent();
            //UserAppTheme = AppTheme.Dark;

            var handler = new HttpClientHandler { UseProxy = false, AllowAutoRedirect = true };
            HttpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(60) };

            DiaryViewModel = Services.GetService<DiaryViewModel>();
            HomePageViewModel = Services.GetService<HomePageViewModel>();
            CalendarViewModel = Services.GetService<CalendarViewModel>();

            AppSettings.Instance.UpdateApplicationTheme();
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("starting UpdateApplicationTheme");
                AppSettings.Instance.UpdateApplicationTheme();
                System.Diagnostics.Debug.WriteLine("Updated Application Theme");
                await UserService.Instance.AutoLogin();
                if (UserService.Instance.CurrentUser != null)
                {
                    await SyncService.DownloadEventsFromServerAsync();
                    await SyncService.DownloadNotesFromServerAsync();
                    SwitchToMainApp();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("User can't login");
                    await Shell.Current.GoToAsync(nameof(LoginPage), false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnStart error: {ex.Message}");
            }
        }

        public static async void SwitchToMainApp()
        {
            try
            {
                await Task.Run(async () =>
                {
                    while (!Application.Current.Resources.ContainsKey("BackgroundColor"))
                    {
                        await Task.Delay(100);
                    }
                });
                System.Diagnostics.Debug.WriteLine("Switching to AppShell.");
                Current.MainPage = new AppShell();
                System.Diagnostics.Debug.WriteLine("Switched to AppShell successfully.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SwitchToMainApp Error: {ex.Message}");
                Current.MainPage.DisplayAlert("Viga", $"Põhirakenduse laadimine ebaõnnestus: " +
                    $"{ex.Message}", "OK");
            }
        }
    }
}