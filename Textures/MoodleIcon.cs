using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using OtterGui.Text;

namespace CkCommons.Textures;

public static class MoodleIcon
{
    public static Vector2 Size => new Vector2(24, 32);
    public static Vector2 SizeFramed => new(ImGui.GetFrameHeight() * .75f, ImGui.GetFrameHeight());

    public static float GetFramedRowHeight(int rows = 1)
        => SizeFramed.Y * rows + ImUtf8.ItemSpacing.Y * (rows - 1);
    
    public static float GetRowHeight(float? h = null, int rows = 1)
        => (h ?? Size.Y) * rows + ImUtf8.ItemSpacing.Y * (rows - 1);

    public static IDalamudTextureWrap? GetGameIconOrDefault(uint iconId)
        => Svc.Texture.GetFromGameIcon(iconId).GetWrapOrDefault();

    public static IDalamudTextureWrap GetGameIconOrEmpty(uint iconId)
        => Svc.Texture.GetFromGameIcon(iconId).GetWrapOrEmpty();

    public static IDalamudTextureWrap? GetGameIconOrDefault(int iconId, int stacks)
        => Svc.Texture.GetFromGameIcon(new GameIconLookup((uint)(iconId + stacks - 1))).GetWrapOrDefault();

    /// <summary>
    ///     Draws the Moodle icon. This only draw a single image so you can use IsItemHovered() outside.
    /// </summary>
    public static void DrawMoodleIcon(int iconId, int stacks, Vector2 size)
    {
        if (Svc.Texture.GetFromGameIcon(new GameIconLookup((uint)(iconId + stacks - 1))).GetWrapOrDefault() is { } wrap)
            ImGui.Image(wrap.Handle, size);
        else
            ImGui.Dummy(size);
    }
}
