using CkCommons.Services;
using SixLabors.ImageSharp;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CkCommons;
public static class Generic
{
    // #nullable disable
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Safe(Action a, bool suppressErrors = false)
    {
        try
        {
            a();
        }
        catch (Exception e)
        {
            if (!suppressErrors) Svc.Log.Error($"{e.Message}\n{e.StackTrace ?? ""}");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? Safe<T>(Func<T> a, bool suppressErrors = false)
    {
        try
        {
            return a();
        }
        catch (Exception e)
        {
            if (!suppressErrors) Svc.Log.Error($"{e.Message}\n{e.StackTrace ?? ""}");
            return default;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T?> Safe<T>(Func<Task<T>> a, bool suppressErrors = false)
    {
        try
        {
            return await a();
        }
        catch (Exception e)
        {
            if (!suppressErrors) Svc.Log.Error($"{e.Message}\n{e.StackTrace ?? ""}");
            return default;
        }
    }

    /// <summary> Returns an image currently on your clipboard to raw image byte[] </summary>
    /// <returns> The image data in a byte array. </returns>
    /// <remarks> This method is not operatable on linux to my knowledge. </remarks>
    public static bool TryGetClipboardImage(out byte[] byteArr, [NotNullWhen(true)] out ImageInfo? context)
    {
        if (!Clipboard.ContainsImage())
        {
            byteArr = Array.Empty<byte>();
            context = null;
            return false;
        }

        try
        {
            // Attempt to retrieve an image from the clipboard.
            if (Clipboard.GetImage() is not { } bitmapFormattedImage)
                throw new ExternalException("No image found in clipboard.");

            using var memoryStream = new MemoryStream();
            bitmapFormattedImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

            // Reset the stream's position to the beginning before detecting the format
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Ensure that it properly went to PNG.
            var format = Image.DetectFormat(memoryStream);
            if (!format.FileExtensions.Contains("png", StringComparer.OrdinalIgnoreCase))
                throw new Exception("Error: Image is not in PNG format.");

            byteArr = memoryStream.ToArray();
            context = Image.Identify(memoryStream);
            return true;
        }
        catch (ExternalException ex)
        {
            Svc.Log.Error("Error: " + ex);
            byteArr = Array.Empty<byte>();
            context = null;
            return false;
        }
    }
}
