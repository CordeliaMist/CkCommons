using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using OtterGuiInternal;

namespace CkCommons.Gui;

// Partial Class for Text Display Helpers.
public static partial class CkGui
{
    #region Underlined
    public static void TextUnderlined(string text, uint color)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.Text, color);
        TextUnderlined(text);
    }

    public static void TextUnderlined(string text, Vector4 color)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.Text, color);
        TextUnderlined(text);
    }

    public static void TextUnderlined(string text)
    {
        var size = ImGui.CalcTextSize(text);
        var cur = ImGui.GetCursorScreenPos();
        cur.Y += size.Y;
        ImGui.GetWindowDrawList().PathLineTo(cur);
        cur.X += size.X;
        ImGui.GetWindowDrawList().PathLineTo(cur);
        ImGui.GetWindowDrawList().PathStroke(ImGuiColors.DalamudWhite.ToUint());
        ImGui.TextUnformatted(text);
    }
    #endregion

    #region DropShadows
    // Remove all constant Sin/Cosine calculations for every TextShadowed call on every frame.
    // Evaluates once, then remains static, signifigantly improving performance.
    private static readonly Vector2[][] PrecomputedJitters = GenerateJitters();

    private static Vector2[][] GenerateJitters()
    {
        var jitters = new Vector2[13][];
        for (int i = 1; i <= 12; i++)
        {
            jitters[i] = new Vector2[i];
            for (int j = 0; j < i; j++)
            {
                float angle = (j / (float)i) * (MathF.PI * 2f);
                jitters[i][j] = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            }
        }
        return jitters;
    }

    // Add some direct methods here.
    public static void AddTextShadowed(this ImDrawListPtr dl, string text, Vector2 pos, uint txtCol, uint fadeCol, Vector2? offset = null, float radius = 1.5f, int quality = 4)
    {
        quality = Math.Clamp(quality, 1, 12);
        var distance = (offset ?? Vector2.One) * ImGuiHelpers.GlobalScale;
        var fadeRadius = radius * ImGuiHelpers.GlobalScale;

        // Get the true fadeCol for each pass.
        var perPassFadeCol = (fadeCol & 0x00FFFFFF) | ((((fadeCol >> 24) & 0xFF) / (uint)quality) << 24);
        // Fetch precalced layout via O(1) lookup
        var jitters = PrecomputedJitters[quality];
        var halfRadius = fadeRadius * .5f;
        var basePos = pos + distance;
        // Draw out the passes
        for (int i = 0; i < quality; i++)
        {
            var thisOffset = jitters[i] * halfRadius;
            dl.AddText(basePos + thisOffset, perPassFadeCol, text);
        }
        // Then base text
        dl.AddText(pos, txtCol, text);
    }

    public static void AddTextShadowed(this ImDrawListPtr dl, ImFontPtr font, float fontSize, Vector2 pos, string text, uint txtCol, uint fadeCol, Vector2? offset = null, float radius = 1.5f, int quality = 4)
    {
        quality = Math.Clamp(quality, 1, 12);
        var distance = (offset ?? Vector2.One) * ImGuiHelpers.GlobalScale;
        var fadeRadius = radius * ImGuiHelpers.GlobalScale;

        // Get the true fadeCol for each pass.
        var perPassFadeCol = (fadeCol & 0x00FFFFFF) | ((((fadeCol >> 24) & 0xFF) / (uint)quality) << 24);
        // Fetch precalced layout via O(1) lookup
        var jitters = PrecomputedJitters[quality];
        var halfRadius = fadeRadius * .5f;
        var basePos = pos + distance;

        // Draw out the passes using the font/size overload
        for (int i = 0; i < quality; i++)
        {
            var thisOffset = jitters[i] * halfRadius;
            dl.AddText(font, fontSize, basePos + thisOffset, perPassFadeCol, text);
        }

        // Then base text
        dl.AddText(font, fontSize, pos, txtCol, text);
    }

    public static void TextShadowed(string text, Vector2? offset = null, float radius = 1.5f, int quality = 6)
        => TextShadowedInternal(text, null, 0xFF000000, offset, radius, quality);

    public static void TextShadowed(string text, Vector4 fadeCol, Vector2? offset = null, float radius = 1.5f, int quality = 6)
        => TextShadowedInternal(text, null, fadeCol, offset, radius, quality);

    public static void TextShadowed(string text, uint fadeCol, Vector2? offset = null, float radius = 1.5f, int quality = 6)
        => TextShadowedInternal(text, null, fadeCol, offset, radius, quality);

    public static void TextShadowed(string text, Vector4 textCol, Vector4 fadeCol, Vector2? offset = null, float radius = 1.5f, int quality = 6)
        => TextShadowedInternal(text, textCol, fadeCol, offset, radius, quality);

    public static void TextShadowed(string text, uint textCol, uint fadeCol, Vector2? offset = null, float radius = 1.5f, int quality = 6)
        => TextShadowedInternal(text, textCol, fadeCol, offset, radius, quality);


    private static void TextShadowedInternal(string text, uint? textCol, uint fadeCol, Vector2? offset = null, float radius = 1.5f, int quality = 6)
    {
        if (string.IsNullOrEmpty(text))
            return;
        var winPtr = ImGuiInternal.GetCurrentWindow();
        quality = Math.Clamp(quality, 1, 12);
        var distance = (offset ?? Vector2.One) * ImGuiHelpers.GlobalScale;
        var fadeRadius = radius * ImGuiHelpers.GlobalScale;
        DrawDropShadow(winPtr, text, fadeCol, distance, fadeRadius, quality);

        if (textCol.HasValue)
            CkGui.ColorText(text, textCol.Value);
        else
            ImGui.TextUnformatted(text);
    }

    private static void TextShadowedInternal(string text, Vector4? textCol, Vector4 fadeCol, Vector2? offset = null, float radius = 1.5f, int quality = 6)
    {
        if (string.IsNullOrEmpty(text))
            return;
        var winPtr = ImGuiInternal.GetCurrentWindow();
        quality = Math.Clamp(quality, 1, 12);
        var distance = (offset ?? Vector2.One) * ImGuiHelpers.GlobalScale;
        var fadeRadius = radius * ImGuiHelpers.GlobalScale;
        DrawDropShadow(winPtr, text, fadeCol.ToUint(), distance, fadeRadius, quality);

        if (textCol.HasValue)
            CkGui.ColorText(text, textCol.Value);
        else
            ImGui.TextUnformatted(text);
    }

    private static void DrawDropShadow(ImGuiWindowPtr winPtr, string text, uint fadeCol, Vector2 offset, float radius, int quality)
    {
        if (winPtr.SkipItems)
            return;
        // Get the true fadeCol for each pass.
        var perPassFadeCol = (fadeCol & 0x00FFFFFF) | ((((fadeCol >> 24) & 0xFF) / (uint)quality) << 24);
        // Fetch precalced layout via O(1) lookup
        var jitters = PrecomputedJitters[quality];
        var halfRadius = radius * .5f;
        var basePos = winPtr.DC.CursorPos + offset;
        // Draw out the passes
        for (int i = 0; i < quality; i++)
        {
            var thisOffset = jitters[i] * halfRadius;
            winPtr.DrawList.AddText(basePos + thisOffset, perPassFadeCol, text);
        }
    }
    #endregion

    #region DropShadow Wrapped
    public static void TextWrappedShadowed(string text, float wrapWidth = 0f, Vector2? offset = null, float radius = 1.5f, int quality = 6)
        => TextWrappedShadowedInternal(text, null, 0xFF000000, wrapWidth, offset, radius, quality);

    public static void TextWrappedShadowed(string text, Vector4 fadeCol, float wrapWidth = 0f, Vector2? offset = null, float radius = 1.5f, int quality = 6)
        => TextWrappedShadowedInternal(text, null, fadeCol, wrapWidth, offset, radius, quality);

    public static void TextWrappedShadowed(string text, uint fadeCol, float wrapWidth = 0f, Vector2? offset = null, float radius = 1.5f, int quality = 6)
        => TextWrappedShadowedInternal(text, null, fadeCol, wrapWidth, offset, radius, quality);

    public static void TextWrappedShadowed(string text, Vector4 textCol, Vector4 fadeCol, float wrapWidth = 0f, Vector2? offset = null, float radius = 1.5f, int quality = 6)
        => TextWrappedShadowedInternal(text, textCol, fadeCol, wrapWidth, offset, radius, quality);

    public static void TextWrappedShadowed(string text, uint textCol, uint fadeCol, float wrapWidth = 0f, Vector2? offset = null, float radius = 1.5f, int quality = 6)
        => TextWrappedShadowedInternal(text, textCol, fadeCol, wrapWidth, offset, radius, quality);

    private static void TextWrappedShadowedInternal(string text, uint? textCol, uint fadeCol, float wrapWidth, Vector2? offset = null, float radius = 1.5f, int quality = 6)
    {
        if (string.IsNullOrEmpty(text))
            return;
        var winPtr = ImGuiInternal.GetCurrentWindow();
        quality = Math.Clamp(quality, 1, 12);
        var textWidth = wrapWidth > 0f ? wrapWidth : ImGui.GetContentRegionAvail().X;
        var distance = (offset ?? Vector2.One) * ImGuiHelpers.GlobalScale;
        var fadeRadius = radius * ImGuiHelpers.GlobalScale;
        DrawDropShadowWrapped(winPtr, text, fadeCol, distance, fadeRadius, quality, textWidth);

        ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + textWidth);
        if (textCol.HasValue)
            CkGui.ColorText(text, textCol.Value);
        else
            ImGui.TextUnformatted(text);
        ImGui.PopTextWrapPos();
    }

    private static void TextWrappedShadowedInternal(string text, Vector4? textCol, Vector4 fadeCol, float wrapWidth, Vector2? offset = null, float radius = 1.5f, int quality = 6)
    {
        if (string.IsNullOrEmpty(text))
            return;
        var winPtr = ImGuiInternal.GetCurrentWindow();
        quality = Math.Clamp(quality, 1, 12);
        var textWidth = wrapWidth > 0f ? wrapWidth : ImGui.GetContentRegionAvail().X;
        var distance = (offset ?? Vector2.One) * ImGuiHelpers.GlobalScale;
        var fadeRadius = radius * ImGuiHelpers.GlobalScale;
        DrawDropShadowWrapped(winPtr, text, fadeCol.ToUint(), distance, fadeRadius, quality, textWidth);

        if (textCol.HasValue)
            CkGui.ColorText(text, textCol.Value);
        else
            ImGui.TextUnformatted(text);
    }

    private static void DrawDropShadowWrapped(ImGuiWindowPtr winPtr, string text, uint fadeCol, Vector2 offset, float radius, int quality, float wrapWidth)
    {
        if (winPtr.SkipItems)
            return;
        // Get the true fadeCol for each pass.
        var perPassFadeCol = (fadeCol & 0x00FFFFFF) | ((((fadeCol >> 24) & 0xFF) / (uint)quality) << 24);

        // Must provide with font and size
        var font = ImGui.GetFont();
        var fontSize = ImGui.GetFontSize();

        // Fetch precalced layout via O(1) lookup
        var jitters = PrecomputedJitters[quality];
        var halfRadius = radius * .5f;
        var basePos = winPtr.DC.CursorPos + offset;
        // Draw out the passes
        for (int i = 0; i < quality; i++)
        {
            var thisOffset = jitters[i] * halfRadius;
            winPtr.DrawList.AddText(font, fontSize, basePos + thisOffset, perPassFadeCol, text, wrapWidth);
        }
    }
    #endregion
}