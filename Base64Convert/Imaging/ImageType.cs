using System.Text;

namespace Base64Convert.Imaging;

public sealed record ImageType(string Name, string MimeType, string Extension)
{
    public static readonly ImageType Png = new("PNG", "image/png", "png");
    public static readonly ImageType Jpeg = new("JPEG", "image/jpeg", "jpg");
    public static readonly ImageType Gif = new("GIF", "image/gif", "gif");
    public static readonly ImageType Webp = new("WebP", "image/webp", "webp");
    public static readonly ImageType Bmp = new("BMP", "image/bmp", "bmp");
    public static readonly ImageType Ico = new("ICO", "image/x-icon", "ico");
    public static readonly ImageType Tiff = new("TIFF", "image/tiff", "tif");
    public static readonly ImageType Svg = new("SVG", "image/svg+xml", "svg");

    /// <summary>
    /// Identifies an image from its leading bytes, or returns null if it isn't a recognised format.
    /// </summary>
    public static ImageType? Detect(ReadOnlySpan<byte> data)
    {
        if (data.StartsWith((byte[])[0x89, 0x50, 0x4E, 0x47])) return Png;
        if (data.StartsWith((byte[])[0xFF, 0xD8, 0xFF])) return Jpeg;
        if (data.StartsWith("GIF8"u8)) return Gif;
        if (data.Length >= 12 && data.StartsWith("RIFF"u8) && data[8..12].SequenceEqual("WEBP"u8)) return Webp;
        if (data.StartsWith("BM"u8)) return Bmp;
        if (data.StartsWith((byte[])[0x00, 0x00, 0x01, 0x00])) return Ico;
        if (data.StartsWith("II*\0"u8) || data.StartsWith("MM\0*"u8)) return Tiff;

        string head = Encoding.UTF8.GetString(data[..Math.Min(data.Length, 1024)]).TrimStart('﻿', ' ', '\t', '\r', '\n');
        if (head.StartsWith("<svg", StringComparison.OrdinalIgnoreCase) ||
            (head.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) && head.Contains("<svg", StringComparison.OrdinalIgnoreCase)))
        {
            return Svg;
        }

        return null;
    }
}
