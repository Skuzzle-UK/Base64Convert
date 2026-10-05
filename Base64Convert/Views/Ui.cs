namespace Base64Convert.Views;

/// <summary>
/// Small UI helpers shared by the converter views.
/// </summary>
internal static class Ui
{
    public static void ShowStatus(Label label, string text, bool isError)
    {
        label.SetDynamicResource(Label.TextColorProperty, isError ? SystemTheme.Error : SystemTheme.TextSecondary);
        label.Text = text;
        label.IsVisible = text.Length > 0;
    }

    public static string FormatChars(int length) => length == 0 ? string.Empty : $"{length:N0} chars";

    public static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:0.#} KB",
        _ => $"{bytes / (1024.0 * 1024.0):0.##} MB",
    };
}
