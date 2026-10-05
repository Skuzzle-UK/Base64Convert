using System.Text;

namespace Base64Convert;

public static class Base64Converter
{
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public static string Encode(string text, bool urlSafe)
    {
        string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

        return urlSafe
            ? encoded.TrimEnd('=').Replace('+', '-').Replace('/', '_')
            : encoded;
    }

    public static bool TryDecode(string base64, out string text, out string error)
    {
        text = string.Empty;

        if (!TryDecodeBytes(base64, out byte[] bytes, out error))
        {
            return false;
        }

        try
        {
            text = StrictUtf8.GetString(bytes);
            return true;
        }
        catch (DecoderFallbackException)
        {
            error = "Decoded data is binary, not UTF-8 text.";
            return false;
        }
    }

    /// <summary>
    /// Decodes standard or URL-safe Base64 (optionally a data URI), ignoring whitespace and tolerating missing padding.
    /// </summary>
    public static bool TryDecodeBytes(string base64, out byte[] bytes, out string error)
    {
        bytes = [];
        error = string.Empty;

        string trimmed = base64.Trim();
        int comma = trimmed.IndexOf(',');
        if (trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma >= 0)
        {
            trimmed = trimmed[(comma + 1)..];
        }

        string normalised = string.Concat(trimmed.Where(c => !char.IsWhiteSpace(c)))
            .Replace('-', '+')
            .Replace('_', '/');

        int remainder = normalised.Length % 4;
        if (remainder == 1)
        {
            error = "Invalid Base64 length.";
            return false;
        }
        if (remainder > 0)
        {
            normalised += new string('=', 4 - remainder);
        }

        try
        {
            bytes = Convert.FromBase64String(normalised);
            return true;
        }
        catch (FormatException)
        {
            error = "Input is not valid Base64.";
            return false;
        }
    }
}
