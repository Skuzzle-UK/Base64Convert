namespace Base64Convert.Imaging;

/// <summary>
/// Reads images dropped onto the app. Only Windows supports dropping from other apps.
/// </summary>
public static class ImageDrop
{
    public static bool CanAccept(DragEventArgs e)
    {
#if WINDOWS
        return e.PlatformArgs?.DragEventArgs is { } args && ImageClipboard.MayContainImage(args.DataView);
#else
        return false;
#endif
    }

    /// <summary>
    /// Sets the caption shown next to the cursor while dragging.
    /// </summary>
    public static void SetCaption(DragEventArgs e, string caption)
    {
#if WINDOWS
        if (e.PlatformArgs?.DragEventArgs is { } args)
        {
            args.DragUIOverride.Caption = caption;
        }
#endif
    }

    public static async Task<byte[]?> GetImageAsync(DropEventArgs e)
    {
#if WINDOWS
        if (e.PlatformArgs?.DragEventArgs is { } args)
        {
            return await ImageClipboard.ReadImageAsync(args.DataView);
        }
#endif
        await Task.CompletedTask;
        return null;
    }
}
