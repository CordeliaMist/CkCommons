using Dalamud.Interface.Textures.TextureWraps;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace CkCommons.Textures;

public static class TextureManager
{
    // maybe add customization for this later?
    public static string AssetFolderPath => Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName!, "Assets");
    public static string EmoteFolderPath => Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName!, "Assets", "Emotes");


    private static readonly object _cacheLock = new();
    internal static readonly HashSet<IDisposable> _monitoredTextureEnumCaches = new();

    /// <summary> Safely allocate a new TextureCache for an Enum type. Then, store it internally for cleanup </summary>
    /// <returns> A new TextureCache instance for the specified Enum type.</returns>
    public static EnumTextureCache<TEnum> CreateEnumTextureCache<TEnum>(Dictionary<TEnum, string> texturePathMap) where TEnum : Enum
    {
        var cache = new EnumTextureCache<TEnum>(texturePathMap);
        lock (_cacheLock)
            _monitoredTextureEnumCaches.Add(cache);
        return cache;
    }

    /// <summary> Rents image from Assets folder, Remains cached until plugin disposal. Care for this wisely. </summary>
    public static bool TryRentAssetDirectoryImage(string path, [NotNullWhen(true)] out IDalamudTextureWrap? fileTexture)
    {
        try
        {
            fileTexture = Svc.Texture.GetFromFile(Path.Combine(AssetFolderPath, path)).RentAsync().Result;
            return true;
        }
        catch (Exception)
        {
            fileTexture = null;
            return false;
        }
    }

    /// <summary> Rents image from Assets folder, Remains cached until plugin disposal. Care for this wisely. </summary>
    public static bool TryRentTexture(string fullFilePath, [NotNullWhen(true)] out IDalamudTextureWrap? fileTexture)
    {
        try
        {
            fileTexture = Svc.Texture.GetFromFile(fullFilePath).RentAsync().Result;
            return true;
        }
        catch (Exception)
        {
            fileTexture = null;
            return false;
        }
    }

    public static async Task<IDalamudTextureWrap?> RentTextureAsync(string fullFilePath)
        => await Generic.Safe(() => Svc.Texture.GetFromFile(fullFilePath).RentAsync());

    public static IDalamudTextureWrap? GetImageFromBytes(byte[] imageData)
        => Generic.Safe(() => Svc.Texture.CreateFromImageAsync(imageData).Result);

    public static async Task<IDalamudTextureWrap?> GetImageFromBytesAsync(byte[] imageData)
        => await Generic.Safe(() => Svc.Texture.CreateFromImageAsync(imageData));

    public static IDalamudTextureWrap? AssetImageOrDefault(string path)
        => Svc.Texture.GetFromFile(Path.Combine(AssetFolderPath, path)).GetWrapOrDefault();

    public static IDalamudTextureWrap AssetImageOrEmpty(string path)
        => Svc.Texture.GetFromFile(Path.Combine(AssetFolderPath, path)).GetWrapOrEmpty();
    
    public static string GetFullAssetPath(string imageName)
    {
        var fullPath = Path.Combine(AssetFolderPath, $"{imageName}.png");
        return File.Exists(fullPath) ? fullPath : string.Empty;
    }

    public static string GetFullEmotePath(string imageName)
    {
        var fullPath = Path.Combine(EmoteFolderPath, $"{imageName}.png");
        return File.Exists(fullPath) ? fullPath : string.Empty;
    }

    public static void Dispose()
    {
        Svc.Log.Information("[CoreTextureManager] Disposing of Cache.");
        IDisposable[] cachesToDispose;
        // Lock only to extract and clear the items
        lock (_cacheLock)
        {
            cachesToDispose = _monitoredTextureEnumCaches.ToArray();
            _monitoredTextureEnumCaches.Clear();
        }
        // Dispose outside the lock to prevent deadlocks
        foreach (var cache in cachesToDispose)
            cache.Dispose();
    }
}
