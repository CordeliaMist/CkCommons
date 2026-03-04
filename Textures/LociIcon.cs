using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using OtterGui.Text;
using System.Diagnostics.CodeAnalysis;

namespace CkCommons.Textures;

public static class LociIcon
{
    public static Vector2 Size => new Vector2(24, 32);
    public static Vector2 SizeFramed => new(ImGui.GetFrameHeight() * .75f, ImGui.GetFrameHeight());

    public static float GetFramedRowHeight(int rows = 1)
        => SizeFramed.Y * rows + ImUtf8.ItemSpacing.Y * (rows - 1);
    
    public static float GetRowHeight(float? h = null, int rows = 1)
        => (h ?? Size.Y) * rows + ImUtf8.ItemSpacing.Y * (rows - 1);

    public static bool TryGetGameIcon(uint iconId, bool hiRes, [NotNullWhen(true)] out IDalamudTextureWrap? wrap)
    {
        if (Svc.Texture.TryGetFromGameIcon(new(iconId, hiRes: hiRes), out var shared) && shared.GetWrapOrDefault() is { } w)
        {
            wrap = w;
            return true;
        }
        wrap = null;
        return false;
    }

    public static IDalamudTextureWrap? GetGameIconOrDefault(uint iconId, bool hiRes = true)
        => Svc.Texture.GetFromGameIcon(new(iconId, hiRes: hiRes)).GetWrapOrDefault();

    public static IDalamudTextureWrap GetGameIconOrEmpty(uint iconId, bool hiRes = true)
        => Svc.Texture.GetFromGameIcon(new(iconId, hiRes: hiRes)).GetWrapOrEmpty();

    public static IDalamudTextureWrap? GetGameIconOrDefault(int iconId, int stacks, bool hiRes = true)
        => Svc.Texture.GetFromGameIcon(new((uint)(iconId + stacks - 1), hiRes: hiRes)).GetWrapOrDefault();

    /// <summary>
    ///     Draws the Loci icon. This only draw a single image so you can use IsItemHovered() outside.
    /// </summary>
    public static void Draw(uint iconId, int stacks, Vector2 size)
    {
        if (Svc.Texture.TryGetFromGameIcon(new((uint)(iconId + stacks - 1)), out var wrap) && wrap.GetWrapOrDefault() is { } texture)
            ImGui.Image(texture.Handle, size);
        else
            ImGui.Dummy(size);
    }
}
