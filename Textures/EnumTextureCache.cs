using Dalamud.Interface.Textures.TextureWraps;

namespace CkCommons.Textures;
/// <summary>
///     Stores a Concurrent Dictionary <see cref="IDalamudTextureWrap"/> cache that retains
///     its texture throughout plugin lifetime. Thread-Safe, and Uses Enums as keys.
/// </summary>
/// <remarks> 
///     Automatically disposed of if created from <see cref="TextureManager>"/>.
/// </remarks>
public class EnumTextureCache<TEnum> : IDisposable where TEnum : Enum
{
    private readonly ConcurrentDictionary<TEnum, IDalamudTextureWrap> _cache = new();

    public IReadOnlyDictionary<TEnum, IDalamudTextureWrap> Cache => _cache;

    public EnumTextureCache(Dictionary<TEnum, string> lookup)
    {
        foreach (var (key, path) in lookup)
        {
            if (string.IsNullOrEmpty(path))
                continue;

            if (TextureManager.TryRentAssetDirectoryImage(path, out var t) && t != null)
                _cache.TryAdd(key, t);
        }
    }

    // attempt to refetch a failed key if it is not present.
    public bool TryRefetchForKey(TEnum key)
    {
        if(_cache.TryGetValue(key, out var existingTexture) && existingTexture != null)
            return true; // already cached.

        if (TextureManager.TryRentAssetDirectoryImage(key.ToString(), out var t) && t != null)
        {
            _cache[key] = t;
            return true;
        }
        return false;
    }

    public void Dispose()
    {
        foreach (var texture in _cache.Values)
            texture?.Dispose();
        _cache.Clear();
    }
}
