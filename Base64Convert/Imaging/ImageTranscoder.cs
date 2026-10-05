#if WINDOWS
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
#elif ANDROID
using Android.Graphics;
using AColor = Android.Graphics.Color;
#endif

namespace Base64Convert.Imaging;

/// <summary>
/// Re-encodes images using the platform's built-in codecs.
/// </summary>
public static class ImageTranscoder
{
    private const int JpegQuality = 92;

    /// <summary>
    /// Formats this platform can encode to.
    /// </summary>
    public static IReadOnlyList<ImageType> TargetTypes { get; } =
#if WINDOWS
        [ImageType.Png, ImageType.Jpeg, ImageType.Bmp, ImageType.Gif, ImageType.Tiff];
#elif ANDROID
        [ImageType.Png, ImageType.Jpeg, ImageType.Webp];
#else
        [];
#endif

#if WINDOWS
    public static async Task<byte[]> ConvertAsync(byte[] source, ImageType target)
    {
        using InMemoryRandomAccessStream input = new();
        using (Stream writer = input.AsStreamForWrite())
        {
            await writer.WriteAsync(source);
            await writer.FlushAsync();
        }
        input.Seek(0);

        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(input);
        PixelDataProvider provider = await decoder.GetPixelDataAsync(
            BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, new BitmapTransform(),
            ExifOrientationMode.RespectExifOrientation, ColorManagementMode.ColorManageToSRgb);
        byte[] pixels = provider.DetachPixelData();

        bool opaque = !SupportsTransparency(target);
        if (opaque)
        {
            FlattenOntoWhite(pixels);
        }

        Guid encoderId = target.Extension switch
        {
            "png" => BitmapEncoder.PngEncoderId,
            "jpg" => BitmapEncoder.JpegEncoderId,
            "bmp" => BitmapEncoder.BmpEncoderId,
            "gif" => BitmapEncoder.GifEncoderId,
            "tif" => BitmapEncoder.TiffEncoderId,
            _ => throw new NotSupportedException($"Can't save as {target.Name}."),
        };

        using InMemoryRandomAccessStream output = new();
        BitmapEncoder encoder = target == ImageType.Jpeg
            ? await BitmapEncoder.CreateAsync(encoderId, output, new BitmapPropertySet
            {
                ["ImageQuality"] = new BitmapTypedValue(JpegQuality / 100f, Windows.Foundation.PropertyType.Single),
            })
            : await BitmapEncoder.CreateAsync(encoderId, output);

        encoder.SetPixelData(BitmapPixelFormat.Bgra8, opaque ? BitmapAlphaMode.Ignore : BitmapAlphaMode.Straight,
            decoder.OrientedPixelWidth, decoder.OrientedPixelHeight, decoder.DpiX, decoder.DpiY, pixels);
        await encoder.FlushAsync();

        output.Seek(0);
        using Stream stream = output.AsStreamForRead();
        using MemoryStream memory = new();
        await stream.CopyToAsync(memory);
        return memory.ToArray();
    }

    private static void FlattenOntoWhite(byte[] bgra)
    {
        for (int i = 0; i < bgra.Length; i += 4)
        {
            int alpha = bgra[i + 3];
            for (int c = 0; c < 3; c++)
            {
                bgra[i + c] = (byte)((bgra[i + c] * alpha + 255 * (255 - alpha)) / 255);
            }
            bgra[i + 3] = 255;
        }
    }
#elif ANDROID
    public static async Task<byte[]> ConvertAsync(byte[] source, ImageType target)
    {
        using Bitmap bitmap = await BitmapFactory.DecodeByteArrayAsync(source, 0, source.Length)
            ?? throw new InvalidDataException("The image couldn't be decoded.");

        Bitmap.CompressFormat format = target.Extension switch
        {
            "png" => Bitmap.CompressFormat.Png!,
            "jpg" => Bitmap.CompressFormat.Jpeg!,
#pragma warning disable CA1422 // Lossy WEBP is the only option before Android 11.
            "webp" => OperatingSystem.IsAndroidVersionAtLeast(30) ? Bitmap.CompressFormat.WebpLossless! : Bitmap.CompressFormat.Webp!,
#pragma warning restore CA1422
            _ => throw new NotSupportedException($"Can't save as {target.Name}."),
        };

        Bitmap output = bitmap;
        if (!SupportsTransparency(target))
        {
            output = Bitmap.CreateBitmap(bitmap.Width, bitmap.Height, Bitmap.Config.Argb8888!);
            using Canvas canvas = new(output);
            canvas.DrawColor(AColor.White);
            canvas.DrawBitmap(bitmap, 0, 0, null);
        }

        try
        {
            using MemoryStream memory = new();
            await output.CompressAsync(format, JpegQuality, memory);
            return memory.ToArray();
        }
        finally
        {
            if (output != bitmap)
            {
                output.Dispose();
            }
        }
    }
#else
    public static Task<byte[]> ConvertAsync(byte[] source, ImageType target) => throw new PlatformNotSupportedException();
#endif

    private static bool SupportsTransparency(ImageType type) => type != ImageType.Jpeg && type != ImageType.Bmp;
}
