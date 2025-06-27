using CkCommons.Gui;
using CkCommons.Helpers;
using CkCommons.Services;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures.TextureWraps;
using ImGuiNET;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace CkCommons.Textures;

public static class CoreTextureManager
{
    // The keys are the .ToString()'s of the defined Enums
    internal static ConcurrentDictionary<string, IDalamudTextureWrap> _nessisary = new();
    internal static ConcurrentDictionary<string, IDalamudTextureWrap> _emoteTextures = new();
    public static IReadOnlyDictionary<string, IDalamudTextureWrap> NessisaryTextures => _nessisary;
    public static IReadOnlyDictionary<string, IDalamudTextureWrap> EmoteTextures => _emoteTextures; 

    public static void RegisterNessisary<TEnum>(Dictionary<TEnum, string> lookup) where TEnum : Enum
    {
        // do the lookup.
        foreach ((TEnum enumKey, string path) in lookup)
        {
            if (string.IsNullOrEmpty(path))
            {
                Svc.Log.Error($"[CoreTextures] The texture path is Empty: {path}.");
                return;
            }

            if (TryRentAssetDirectoryImage(path, out IDalamudTextureWrap? texture))
                _nessisary.TryAdd(enumKey.ToString(), texture);
        }
    }

    public static void RegisterEmote<TEnum>(Dictionary<TEnum, string> lookup) where TEnum : Enum
    {
        // do the lookup.
        foreach ((TEnum enumKey, string path) in lookup)
        {
            if (string.IsNullOrEmpty(path))
            {
                Svc.Log.Error($"[CoreTextures] The texture path is Empty: {path}.");
                return;
            }

            if (TryRentAssetDirectoryImage(path, out IDalamudTextureWrap? texture))
                _emoteTextures.TryAdd(enumKey.ToString(), texture);
        }
    }

    /// <summary> 
    ///     Rents an image from the Assets folder, using the provided path.
    ///     Remains cached until the plugin is disposed of. Care for this wisely.
    /// </summary>
    private static bool TryRentAssetDirectoryImage(string path, [NotNullWhen(true)] out IDalamudTextureWrap? fileTexture)
    {
        try
        {
            fileTexture = Svc.Texture.GetFromFile(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName!, "Assets", path)).RentAsync().Result;
            return true;
        }
        catch (Exception)
        {
            fileTexture = null;
            return false;
        }
    }

    public static void Dispose()
    {
        Svc.Log.Information("[CoreTextureManager] Disposing of Cache.");
        foreach (IDalamudTextureWrap texture in _nessisary.Values)
            texture?.Dispose();
        foreach (IDalamudTextureWrap texture in _emoteTextures.Values)
            texture?.Dispose();

        // clear the dictionary, erasing all disposed textures.
        _nessisary.Clear();
        _emoteTextures.Clear();
    }
}