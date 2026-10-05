#if ANDROID
using Microsoft.Maui.Platform;
#endif

namespace Base64Convert;

/// <summary>
/// Publishes the device's own colours (Windows accent / Fluent surfaces, Android Material You) as dynamic
/// resources, and swaps them when the device switches between light and dark.
/// </summary>
public static class SystemTheme
{
    public const string Accent = nameof(Accent);
    public const string OnAccent = nameof(OnAccent);
    public const string PageBackground = nameof(PageBackground);
    public const string CardBackground = nameof(CardBackground);
    public const string CardStroke = nameof(CardStroke);
    public const string TextPrimary = nameof(TextPrimary);
    public const string TextSecondary = nameof(TextSecondary);
    public const string Error = nameof(Error);

    public static bool IsDark => Application.Current?.RequestedTheme == AppTheme.Dark;

    public static void Apply(Application app)
    {
        Write(app);
        app.RequestedThemeChanged += (_, _) => Write(app);
    }

    private static void Write(Application app)
    {
        bool dark = app.RequestedTheme == AppTheme.Dark;
        foreach ((string key, (Color Light, Color Dark) value) in GetPalette())
        {
            app.Resources[key] = dark ? value.Dark : value.Light;
        }
    }

    private static Dictionary<string, (Color Light, Color Dark)> GetPalette()
    {
        Dictionary<string, (Color, Color)> palette = new()
        {
            [Error] = (Color.FromArgb("#C42B1C"), Color.FromArgb("#FF99A4")),
        };

#if WINDOWS
        // Fluent: accent fills use AccentDark1 on light and AccentLight2 on dark.
        Windows.UI.ViewManagement.UISettings settings = new();
        Color Ui(Windows.UI.ViewManagement.UIColorType type)
        {
            Windows.UI.Color c = settings.GetColorValue(type);
            return Color.FromRgba(c.R, c.G, c.B, c.A);
        }

        palette[Accent] = (Ui(Windows.UI.ViewManagement.UIColorType.AccentDark1), Ui(Windows.UI.ViewManagement.UIColorType.AccentLight2));
        palette[OnAccent] = (Colors.White, Colors.Black);
        palette[PageBackground] = (Color.FromArgb("#F3F3F3"), Color.FromArgb("#202020"));
        palette[CardBackground] = (Color.FromArgb("#FBFBFB"), Color.FromArgb("#2B2B2B"));
        palette[CardStroke] = (Color.FromArgb("#E5E5E5"), Color.FromArgb("#1D1D1D"));
        palette[TextPrimary] = (Color.FromArgb("#1B1B1B"), Colors.White);
        palette[TextSecondary] = (Color.FromArgb("#5F5F5F"), Color.FromArgb("#C5C5C5"));
#elif ANDROID
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            // Material You dynamic colours derived from the wallpaper.
            Android.Content.Context context = Platform.AppContext;
            Color Sys(int id) => new Android.Graphics.Color(context.GetColor(id)).ToColor();

            palette[Accent] = (Sys(Android.Resource.Color.SystemAccent1600), Sys(Android.Resource.Color.SystemAccent1200));
            palette[OnAccent] = (Colors.White, Sys(Android.Resource.Color.SystemAccent1800));
            palette[PageBackground] = (Sys(Android.Resource.Color.SystemNeutral150), Sys(Android.Resource.Color.SystemNeutral1900));
            palette[CardBackground] = (Sys(Android.Resource.Color.SystemNeutral110), Sys(Android.Resource.Color.SystemNeutral1800));
            palette[CardStroke] = (Sys(Android.Resource.Color.SystemNeutral110), Sys(Android.Resource.Color.SystemNeutral1800));
            palette[TextPrimary] = (Sys(Android.Resource.Color.SystemNeutral1900), Sys(Android.Resource.Color.SystemNeutral1100));
            palette[TextSecondary] = (Sys(Android.Resource.Color.SystemNeutral2700), Sys(Android.Resource.Color.SystemNeutral2200));
        }
        else
        {
            // Material 3 baseline for devices without dynamic colour.
            palette[Accent] = (Color.FromArgb("#6750A4"), Color.FromArgb("#D0BCFF"));
            palette[OnAccent] = (Colors.White, Color.FromArgb("#381E72"));
            palette[PageBackground] = (Color.FromArgb("#F3EDF7"), Color.FromArgb("#141218"));
            palette[CardBackground] = (Color.FromArgb("#FEF7FF"), Color.FromArgb("#211F26"));
            palette[CardStroke] = (Color.FromArgb("#FEF7FF"), Color.FromArgb("#211F26"));
            palette[TextPrimary] = (Color.FromArgb("#1D1B20"), Color.FromArgb("#E6E0E9"));
            palette[TextSecondary] = (Color.FromArgb("#49454F"), Color.FromArgb("#CAC4D0"));
        }
#endif

        return palette;
    }
}
