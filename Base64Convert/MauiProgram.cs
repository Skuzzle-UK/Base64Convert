using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace Base64Convert;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif

        RemoveEditorChrome();

        return builder.Build();
    }

    /// <summary>
    /// Strips the native border, underline and focus background from editors so they sit cleanly inside cards.
    /// </summary>
    private static void RemoveEditorChrome()
    {
        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("Borderless", (handler, _) =>
        {
#if WINDOWS
            Microsoft.UI.Xaml.Controls.TextBox textBox = handler.PlatformView;
            Microsoft.UI.Xaml.Media.SolidColorBrush transparent = new(Microsoft.UI.Colors.Transparent);
            string[] brushKeys =
            [
                "TextControlBackground", "TextControlBackgroundPointerOver", "TextControlBackgroundFocused",
                "TextControlBackgroundDisabled", "TextControlBorderBrush", "TextControlBorderBrushPointerOver",
                "TextControlBorderBrushFocused", "TextControlBorderBrushDisabled",
            ];
            foreach (string key in brushKeys)
            {
                textBox.Resources[key] = transparent;
            }
            textBox.Resources["TextControlBorderThemeThickness"] = new Microsoft.UI.Xaml.Thickness(0);
            textBox.Resources["TextControlBorderThemeThicknessFocused"] = new Microsoft.UI.Xaml.Thickness(0);
            textBox.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            textBox.Padding = new Microsoft.UI.Xaml.Thickness(0, 4, 0, 4);
#elif ANDROID
            handler.PlatformView.Background = null;
            handler.PlatformView.SetPadding(0, 8, 0, 8);
#endif
        });
    }
}
