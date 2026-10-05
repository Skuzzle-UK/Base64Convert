using Base64Convert.Imaging;
using CommunityToolkit.Maui.Storage;

namespace Base64Convert.Views;

public partial class ImageConverterView : ContentView
{
    // Rendering megabytes of text in a label is slow; the full string is still copied.
    private const int PreviewLength = 4000;

    private byte[]? _sourceImage;
    private ImageType? _sourceFormat;
    private string _encoded = string.Empty;

    private byte[]? _decodedImage;
    private ImageType? _decodedFormat;

    public ImageConverterView()
    {
        InitializeComponent();
    }

    private void OnModeChanged(object? sender, EventArgs e)
    {
        bool encoding = ModeSwitch.IsFirstSelected;
        EncodePanel.IsVisible = encoding;
        DecodePanel.IsVisible = !encoding;
        DataUriChip.IsVisible = encoding;
    }

    // ---- Encode: image -> Base64 ----

    private async void OnOpenClicked(object? sender, EventArgs e)
    {
        try
        {
            FileResult? file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choose an image",
                FileTypes = FilePickerFileType.Images,
            });
            if (file is null)
            {
                return;
            }

            using Stream stream = await file.OpenReadAsync();
            using MemoryStream memory = new();
            await stream.CopyToAsync(memory);
            SetSourceImage(memory.ToArray());
        }
        catch (Exception ex)
        {
            Ui.ShowStatus(EncodeStatus, $"Couldn't open the file: {ex.Message}", isError: true);
        }
    }

    private async void OnPasteImageClicked(object? sender, EventArgs e)
    {
        try
        {
            byte[]? bytes = await ImageClipboard.GetImageAsync();
            if (bytes is null)
            {
                Ui.ShowStatus(EncodeStatus, "There's no image on the clipboard.", isError: true);
                return;
            }

            SetSourceImage(bytes);
        }
        catch (Exception ex)
        {
            Ui.ShowStatus(EncodeStatus, $"Couldn't read the clipboard: {ex.Message}", isError: true);
        }
    }

    private void OnClearImageClicked(object? sender, EventArgs e) => SetSourceImage(null);

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (!ImageDrop.CanAccept(e))
        {
            e.AcceptedOperation = DataPackageOperation.None;
            return;
        }

        e.AcceptedOperation = DataPackageOperation.Copy;
        ImageDrop.SetCaption(e, "Encode image");
        HighlightDropTarget(true);
    }

    private void OnDragLeave(object? sender, DragEventArgs e) => HighlightDropTarget(false);

    private async void OnDrop(object? sender, DropEventArgs e)
    {
        e.Handled = true;
        HighlightDropTarget(false);

        try
        {
            byte[]? bytes = await ImageDrop.GetImageAsync(e);
            ModeSwitch.IsFirstSelected = true;

            if (bytes is null)
            {
                Ui.ShowStatus(EncodeStatus, "That doesn't look like an image.", isError: true);
                return;
            }

            SetSourceImage(bytes);
        }
        catch (Exception ex)
        {
            Ui.ShowStatus(EncodeStatus, $"Couldn't read the dropped image: {ex.Message}", isError: true);
        }
    }

    private void HighlightDropTarget(bool on)
    {
        // Switch to encode so the highlighted card is visible while dragging.
        if (on)
        {
            ModeSwitch.IsFirstSelected = true;
        }

        SourceCard.SetDynamicResource(Border.StrokeProperty, on ? SystemTheme.Accent : SystemTheme.CardStroke);
        SourceCard.StrokeThickness = on ? 2 : 1;
    }

    private void OnDataUriToggled(object? sender, EventArgs e) => UpdateEncoded();

    private async void OnCopyBase64Clicked(object? sender, EventArgs e)
    {
        if (_encoded.Length == 0)
        {
            return;
        }

        await Clipboard.Default.SetTextAsync(_encoded);
        await CopyBase64Button.FlashAsync("Copied");
    }

    private void SetSourceImage(byte[]? bytes)
    {
        Ui.ShowStatus(EncodeStatus, string.Empty, isError: false);

        ImageType? format = bytes is null ? null : ImageType.Detect(bytes);
        if (bytes is not null && format is null)
        {
            Ui.ShowStatus(EncodeStatus, "That isn't a recognised image format.", isError: true);
            bytes = null;
        }

        _sourceImage = bytes;
        _sourceFormat = format;
        SourcePreview.Source = bytes is null ? null : ImageSource.FromStream(() => new MemoryStream(bytes));
        SourcePlaceholder.IsVisible = bytes is null;
        SourceInfo.Text = bytes is null || format is null ? string.Empty : $"{format.Name} · {Ui.FormatSize(bytes.Length)}";
        UpdateEncoded();
    }

    private void UpdateEncoded()
    {
        if (_sourceImage is null || _sourceFormat is null)
        {
            _encoded = string.Empty;
        }
        else
        {
            string base64 = Convert.ToBase64String(_sourceImage);
            _encoded = DataUriChip.IsChecked ? $"data:{_sourceFormat.MimeType};base64,{base64}" : base64;
        }

        EncodedText.Text = _encoded.Length > PreviewLength ? _encoded[..PreviewLength] + "…" : _encoded;
        EncodedCount.Text = Ui.FormatChars(_encoded.Length);
    }

    // ---- Decode: Base64 -> image ----

    private async void OnPasteBase64Clicked(object? sender, EventArgs e)
    {
        string? text = await Clipboard.Default.GetTextAsync();
        if (text is not null)
        {
            DecodeInput.Text = text;
        }
    }

    private void OnClearBase64Clicked(object? sender, EventArgs e) => DecodeInput.Text = string.Empty;

    private void OnBase64Changed(object? sender, TextChangedEventArgs e)
    {
        string input = DecodeInput.Text ?? string.Empty;
        DecodeInputCount.Text = Ui.FormatChars(input.Length);

        if (string.IsNullOrWhiteSpace(input))
        {
            SetDecodedImage(null, null, string.Empty);
        }
        else if (!Base64Converter.TryDecodeBytes(input, out byte[] bytes, out string error))
        {
            SetDecodedImage(null, null, error);
        }
        else if (ImageType.Detect(bytes) is not { } format)
        {
            SetDecodedImage(null, null, "Decoded data isn't a recognised image format.");
        }
        else
        {
            SetDecodedImage(bytes, format, string.Empty);
        }
    }

    private async void OnCopyImageClicked(object? sender, EventArgs e)
    {
        if (_decodedImage is null || _decodedFormat is null)
        {
            return;
        }

        try
        {
            await ImageClipboard.SetImageAsync(_decodedImage, _decodedFormat);
            await CopyImageButton.FlashAsync("Copied");
        }
        catch (Exception ex)
        {
            Ui.ShowStatus(DecodeStatus, $"Couldn't copy the image: {ex.Message}", isError: true);
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_decodedImage is null || _decodedFormat is null)
        {
            return;
        }

        ImageType? target = await ChooseSaveFormatAsync(_decodedFormat);
        if (target is null)
        {
            return;
        }

        byte[] bytes;
        try
        {
            bytes = target == _decodedFormat
                ? _decodedImage
                : await ImageTranscoder.ConvertAsync(_decodedImage, target);
        }
        catch (Exception ex)
        {
            Ui.ShowStatus(DecodeStatus, $"Couldn't convert to {target.Name}: {ex.Message}", isError: true);
            return;
        }

        using MemoryStream stream = new(bytes);
        FileSaverResult result = await FileSaver.Default.SaveAsync($"image.{target.Extension}", stream, CancellationToken.None);

        if (result.IsSuccessful)
        {
            Ui.ShowStatus(DecodeStatus, $"Saved to {result.FilePath}", isError: false);
        }
        else if (result.Exception is { } ex && !ex.Message.Contains("cancel", StringComparison.OrdinalIgnoreCase))
        {
            Ui.ShowStatus(DecodeStatus, $"Couldn't save the image: {ex.Message}", isError: true);
        }
    }

    /// <summary>
    /// Asks which format to save in; the original format is always offered first.
    /// </summary>
    private async Task<ImageType?> ChooseSaveFormatAsync(ImageType original)
    {
        // SVG is vector, so it can't be re-encoded by the bitmap codecs.
        List<ImageType> choices = [original, .. original == ImageType.Svg
            ? []
            : ImageTranscoder.TargetTypes.Where(t => t != original)];

        if (choices.Count == 1 || Window?.Page is not { } page)
        {
            return original;
        }

        string originalLabel = $"{original.Name} (original)";
        string[] labels = [originalLabel, .. choices.Skip(1).Select(t => t.Name)];

        string? choice = await page.DisplayActionSheetAsync("Save as", "Cancel", null, labels);
        return choice == originalLabel
            ? original
            : choices.Skip(1).FirstOrDefault(t => t.Name == choice);
    }

    private void SetDecodedImage(byte[]? bytes, ImageType? format, string error)
    {
        _decodedImage = bytes;
        _decodedFormat = format;
        DecodedPreview.Source = bytes is null ? null : ImageSource.FromStream(() => new MemoryStream(bytes));
        DecodedPlaceholder.IsVisible = bytes is null;
        DecodedInfo.Text = bytes is null || format is null ? string.Empty : $"{format.Name} · {Ui.FormatSize(bytes.Length)}";
        Ui.ShowStatus(DecodeStatus, error, isError: true);
    }
}
