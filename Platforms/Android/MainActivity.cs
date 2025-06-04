using Android.App;
using Android.Content.PM;
using Android.OS;

namespace NutikasPaevik
{
    [Activity(Theme = "@style/AppTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (SupportActionBar != null)
            {
                SupportActionBar.Hide();
            }
        }
    }
}