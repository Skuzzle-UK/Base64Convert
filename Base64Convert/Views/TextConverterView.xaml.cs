namespace Base64Convert.Views;

public partial class TextConverterView : ContentView
{
    public TextConverterView()
    {
        InitializeComponent();
        ApplyMode();
    }

    private bool IsEncoding => ModeSwitch.IsFirstSelected;

    private void OnModeChanged(object? sender, EventArgs e) => ApplyMode();

    private void OnUrlSafeToggled(object? sender, EventArgs e) => UpdateOutput();

    private void OnInputChanged(object? sender, TextChangedEventArgs e) => UpdateOutput();

    private async void OnPasteClicked(object? sender, EventArgs e)
    {
        string? text = await Clipboard.Default.GetTextAsync();
        if (text is not null)
        {
            InputEditor.Text = text;
        }
    }

    private void OnClearClicked(object? sender, EventArgs e) => InputEditor.Text = string.Empty;

    private async void OnCopyClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(OutputText.Text))
        {
            return;
        }

        await Clipboard.Default.SetTextAsync(OutputText.Text);
        await CopyButton.FlashAsync("Copied");
    }

    private void ApplyMode()
    {
        InputCaption.Text = IsEncoding ? "Text" : "Base64";
        OutputCaption.Text = IsEncoding ? "Base64" : "Text";
        InputEditor.Placeholder = IsEncoding ? "Type or paste text to encode" : "Paste Base64 to decode";
        UpdateOutput();
    }

    private void UpdateOutput()
    {
        string input = InputEditor.Text ?? string.Empty;
        string error = string.Empty;

        if (input.Length == 0)
        {
            OutputText.Text = string.Empty;
        }
        else if (IsEncoding)
        {
            OutputText.Text = Base64Converter.Encode(input, UrlSafeChip.IsChecked);
        }
        else if (Base64Converter.TryDecode(input, out string decoded, out error))
        {
            OutputText.Text = decoded;
        }
        else
        {
            OutputText.Text = string.Empty;
        }

        InputCount.Text = Ui.FormatChars(input.Length);
        OutputCount.Text = Ui.FormatChars(OutputText.Text?.Length ?? 0);
        StatusLabel.Text = error;
        StatusLabel.IsVisible = error.Length > 0;
    }
}
