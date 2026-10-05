namespace Base64Convert.Controls;

/// <summary>
/// A native platform button with an icon from Resources/Images (ic_{name}_{ink|paper}.svg).
/// </summary>
public class IconButton : Button
{
    private string _icon = string.Empty;

    public IconButton()
    {
        ContentLayout = new ButtonContentLayout(ButtonContentLayout.ImagePosition.Left, 6);
        VerticalOptions = LayoutOptions.Center;

        if (Application.Current is { } app)
        {
            app.RequestedThemeChanged += (_, _) => UpdateImage();
        }
    }

    /// <summary>
    /// Icon name, e.g. "copy".
    /// </summary>
    public string Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            UpdateImage();
        }
    }

    /// <summary>
    /// Briefly shows a tick and the given text as confirmation, e.g. "Copied".
    /// </summary>
    public async Task FlashAsync(string text)
    {
        string originalText = Text;
        string originalIcon = Icon;
        Text = text;
        Icon = "check";
        await Task.Delay(1200);
        Text = originalText;
        Icon = originalIcon;
    }

    private void UpdateImage()
    {
        if (string.IsNullOrEmpty(_icon))
        {
            ImageSource = null;
            return;
        }

        // Match the native button's text colour: Windows buttons are neutral (dark text on light),
        // Android buttons are filled with the accent (light text on light theme).
        bool lightGlyph = DeviceInfo.Platform == DevicePlatform.Android ? !SystemTheme.IsDark : SystemTheme.IsDark;
        ImageSource = $"ic_{_icon}_{(lightGlyph ? "paper" : "ink")}.png";
    }
}
