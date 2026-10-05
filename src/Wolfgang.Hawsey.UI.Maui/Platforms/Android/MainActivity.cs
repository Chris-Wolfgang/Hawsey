using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;

namespace Wolfgang.Hawsey.UI.Maui;

// Landscape only: the table is laid out for a wide screen and scaled to fit
// (Views/TableScale); in portrait it would be about a third of its size.
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    ScreenOrientation = ScreenOrientation.SensorLandscape,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
                           ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
                           ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Full screen, as card games usually are: the status and navigation bars are
        // hidden (a swipe from the edge shows them for a moment), so the table is scaled
        // to the whole screen. With the bars showing, the page reported the full screen
        // height while drawing below the status bar, and the table's bottom was cut off.
        if (Window is not { } window)
        {
            return;
        }

        WindowCompat.SetDecorFitsSystemWindows(window, decorFitsSystemWindows: false);

        if (WindowCompat.GetInsetsController(window, window.DecorView) is { } controller)
        {
            controller.Hide(WindowInsetsCompat.Type.SystemBars());
            controller.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
        }
    }
}
