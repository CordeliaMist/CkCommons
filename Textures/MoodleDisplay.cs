using CkCommons.Gui;
using CkCommons.RichText;
using CkCommons.Services;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using ImGuiNET;

namespace CkCommons.Textures;

// Migrate this to a static class soon.
public static class MoodleDisplay
{
    public enum StatusType
    {
        Positive,
        Negative,
        Special
    }

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
            ImGui.Image(wrap.ImGuiHandle, size);
        else
            ImGui.Dummy(size);
    }
}
