using Android.App;
using Android.Content;

namespace Base64Convert;

/// <summary>
/// Shares images copied to the clipboard with other apps.
/// </summary>
[ContentProvider(["${applicationId}.imageprovider"], Exported = false, GrantUriPermissions = true)]
[MetaData("android.support.FILE_PROVIDER_PATHS", Resource = "@xml/image_provider_paths")]
public class ImageFileProvider : AndroidX.Core.Content.FileProvider
{
}
