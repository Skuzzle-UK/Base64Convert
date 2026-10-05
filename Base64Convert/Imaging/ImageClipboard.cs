#if WINDOWS
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Clipboard = Windows.ApplicationModel.DataTransfer.Clipboard;
using DataPackage = Windows.ApplicationModel.DataTransfer.DataPackage;
using DataPackageView = Windows.ApplicationModel.DataTransfer.DataPackageView;
#elif ANDROID
using Android.Content;
#endif

namespace Base64Convert.Imaging;

/// <summary>
/// Reads and writes images on the system clipboard. MAUI's built-in clipboard only supports text.
/// </summary>
public static class ImageClipboard
{
#if WINDOWS
    public static Task<byte[]?> GetImageAsync() => ReadImageAsync(Clipboard.GetContent());

    /// <summary>
    /// True if the data might hold an image that <see cref="ReadImageAsync"/> can read.
    /// </summary>
    internal static bool MayContainImage(DataPackageView content) =>
        content.Contains(StandardDataFormats.StorageItems) ||
        content.Contains(StandardDataFormats.Bitmap) ||
        content.Contains("PNG");

    /// <summary>
    /// Reads an image from clipboard or drag-and-drop data.
    /// </summary>
    internal static async Task<byte[]?> ReadImageAsync(DataPackageView content)
    {
        // A file copied or dragged from Explorer.
        if (content.Contains(StandardDataFormats.StorageItems))
        {
            foreach (IStorageItem item in await content.GetStorageItemsAsync())
            {
                if (item is StorageFile file)
                {
                    using Stream stream = await file.OpenStreamForReadAsync();
                    byte[] bytes = await ReadAllAsync(stream);
                    if (ImageType.Detect(bytes) is not null)
                    {
                        return bytes;
                    }
                }
            }
        }

        // Browsers and Office put an exact PNG alongside the bitmap, preserving transparency.
        if (content.Contains("PNG") && await content.GetDataAsync("PNG") is IRandomAccessStream png)
        {
            using Stream stream = png.AsStreamForRead();
            return await ReadAllAsync(stream);
        }

        if (content.Contains(StandardDataFormats.Bitmap))
        {
            RandomAccessStreamReference reference = await content.GetBitmapAsync();
            using IRandomAccessStreamWithContentType bitmap = await reference.OpenReadAsync();
            return await ToPngAsync(bitmap);
        }

        return null;
    }

    public static async Task SetImageAsync(byte[] bytes, ImageType format)
    {
        InMemoryRandomAccessStream stream = new();
        using (Stream writer = stream.AsStreamForWrite())
        {
            await writer.WriteAsync(bytes);
            await writer.FlushAsync();
        }
        stream.Seek(0);

        DataPackage package = new();
        package.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));
        Clipboard.SetContent(package);
        Clipboard.Flush();
    }

    private static async Task<byte[]> ToPngAsync(IRandomAccessStream source)
    {
        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(source);
        // Clipboard bitmaps often carry a meaningless alpha channel, so drop it.
        using SoftwareBitmap bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore);

        using InMemoryRandomAccessStream output = new();
        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, output);
        encoder.SetSoftwareBitmap(bitmap);
        await encoder.FlushAsync();

        output.Seek(0);
        using Stream stream = output.AsStreamForRead();
        return await ReadAllAsync(stream);
    }
#elif ANDROID
    private const string CacheFolder = "clipboard";

    public static async Task<byte[]?> GetImageAsync()
    {
        Context context = Platform.AppContext;
        ClipboardManager clipboard = (ClipboardManager)context.GetSystemService(Context.ClipboardService)!;

        ClipData? clip = clipboard.PrimaryClip;
        Android.Net.Uri? uri = clip is { ItemCount: > 0 } ? clip.GetItemAt(0)?.Uri : null;
        if (uri is null)
        {
            return null;
        }

        using Stream? stream = context.ContentResolver?.OpenInputStream(uri);
        return stream is null ? null : await ReadAllAsync(stream);
    }

    public static async Task SetImageAsync(byte[] bytes, ImageType format)
    {
        Context context = Platform.AppContext;

        string folder = Path.Combine(FileSystem.CacheDirectory, CacheFolder);
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, recursive: true);
        }
        Directory.CreateDirectory(folder);

        string path = Path.Combine(folder, $"image.{format.Extension}");
        await File.WriteAllBytesAsync(path, bytes);

        Android.Net.Uri uri = AndroidX.Core.Content.FileProvider.GetUriForFile(
            context, $"{context.PackageName}.imageprovider", new Java.IO.File(path))!;

        ClipboardManager clipboard = (ClipboardManager)context.GetSystemService(Context.ClipboardService)!;
        clipboard.PrimaryClip = ClipData.NewUri(context.ContentResolver, "Image", uri);
    }
#else
    public static Task<byte[]?> GetImageAsync() => throw new PlatformNotSupportedException();

    public static Task SetImageAsync(byte[] bytes, ImageType format) => throw new PlatformNotSupportedException();
#endif

    private static async Task<byte[]> ReadAllAsync(Stream stream)
    {
        using MemoryStream memory = new();
        await stream.CopyToAsync(memory);
        return memory.ToArray();
    }
}
