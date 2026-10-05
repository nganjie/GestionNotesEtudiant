using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;

namespace GestionNotesEtudiant
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (Window is null)
                return;

            // Barre d'état claire, accordée au fond des écrans.
            if (!OperatingSystem.IsAndroidVersionAtLeast(35))
                Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#F5F6F8"));
            WindowCompat.GetInsetsController(Window, Window.DecorView).AppearanceLightStatusBars = true;
        }
    }
}
