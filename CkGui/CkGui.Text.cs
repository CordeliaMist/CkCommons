using CkCommons;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using OtterGui;
using OtterGui.Text;
using System.Runtime.CompilerServices;

namespace CkCommons.Gui;

// Partial Class for Text Display Helpers.
public static partial class CkGui
{

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void RightAligned(string text, float offset = 0)
    {
        offset = ImGui.GetContentRegionAvail().X - offset - ImGui.CalcTextSize(text).X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
        ImGui.TextUnformatted(text);
    }

    public static void RightAlignedColor(string text, uint color, float offset = 0)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.Text, color);
        RightAligned(text, offset);
    }

    public static void RightAlignedColor(string text, Vector4 color, float offset = 0)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.Text, color);
        RightAligned(text, offset);
    }

    /// <seealso cref="ImUtf8.TextRightAligned(ReadOnlySpan{byte}, float)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void RightFrameAligned(string text, float offset = 0)
    {
        offset = ImGui.GetContentRegionAvail().X - offset - ImGui.CalcTextSize(text).X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
        ImGui.TextUnformatted(text); // Used to be ImUtf8, FYI
    }

    public static void RightFrameAlignedColor(string text, uint color, float offset = 0)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.Text, color);
        RightFrameAligned(text, offset);
    }

    public static void RightFrameAlignedColor(string text, Vector4 color, float offset = 0)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.Text, color);
        RightFrameAligned(text, offset);
    }

    /// <summary> An Unformatted Text version of ImGui.TextColored accepting UINT </summary>
    public static void TextInline(string text, bool inner = true)
    {
        if (inner) ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
        else ImGui.SameLine();

        ImGui.TextUnformatted(text);
    }


    public static void TextFrameAlignedInline(string text, bool inner = true)
    {
        if (inner)
            ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
        else
            ImGui.SameLine();

        ImUtf8.TextFrameAligned(text);
    }

    /// <summary> An Unformatted Text version of ImGui.TextColored accepting UINT </summary>
    public static void ColorText(string text, uint color)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(text);
    }

    /// <summary> An Unformatted Text version of ImGui.TextColored accepting UINT </summary>
    public static void ColorTextInline(string text, uint color, bool inner = true)
    {
        if (inner) ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
        else ImGui.SameLine();

        using var _ = ImRaii.PushColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(text);
    }

    /// <summary> An Frame-Aligned Text version of ImGui.TextColored accepting UINT </summary>
    public static void ColorTextFrameAligned(string text, uint color)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.Text, color);
        ImUtf8.TextFrameAligned(text);
    }

    /// <summary> An Frame-Aligned Text version of ImGui.TextColored accepting UINT </summary>
    public static void ColorTextFrameAlignedInline(string text, uint color, bool inner = true)
    {
        if (inner) ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
        else ImGui.SameLine();

        using var _ = ImRaii.PushColor(ImGuiCol.Text, color);
        ImUtf8.TextFrameAligned(text);
    }

    /// <summary> An Unformatted Text version of ImGui.TextColored </summary>
    public static void ColorText(string text, Vector4 color)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(text);
    }

    /// <summary> An Unformatted Text version of ImGui.TextColored </summary>
    public static void ColorTextInline(string text, Vector4 color, bool inner = true)
    {
        if (inner) ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
        else ImGui.SameLine();

        using var _ = ImRaii.PushColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(text);
    }

    /// <summary> An Frame-Aligned Text version of ImGui.TextColored accepting UINT </summary>
    public static void ColorTextFrameAligned(string text, Vector4 color)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.Text, color);
        ImUtf8.TextFrameAligned(text);
    }

    /// <summary> An Frame-Aligned Text version of ImGui.TextColored accepting UINT </summary>
    public static void ColorTextFrameAlignedInline(string text, Vector4 color, bool inner = true)
    {
        if (inner) ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
        else ImGui.SameLine();

        using var _ = ImRaii.PushColor(ImGuiCol.Text, color);
        ImUtf8.TextFrameAligned(text);
    }

    /// <summary> Displays colored text based on the boolean value of true or false. </summary>
    public static void ColorTextBool(string text, bool value)
    {
        Vector4 color = value ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed;
        ColorText(text, color);
    }

    /// <summary> Displays colored text based on the boolean value of true or false. </summary>
    /// <remarks> Can provide custom colors if desired. </remarks>
    public static void ColorTextBool(string text, bool value, Vector4 colorTrue = default, Vector4 colorFalse = default)
    {
        Vector4 color = value
            ? (colorTrue == default) ? ImGuiColors.HealerGreen : colorTrue
            : (colorFalse == default) ? ImGuiColors.DalamudRed : colorFalse;

        ColorText(text, color);
    }

    public static void CenterTextAligned(string text, float? width = null)
    {
        float offset = ((width ?? ImGui.GetContentRegionAvail().X) - ImGui.CalcTextSize(text).X) / 2;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
        ImUtf8.TextFrameAligned(text);
    }

    public static void ColorTextCentered(string text, Vector4 color, float? width = null)
    {
        float offset = ((width ?? ImGui.GetContentRegionAvail().X) - ImGui.CalcTextSize(text).X) / 2;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
        ColorText(text, color);
    }

    public static void CenterColorTextAligned(string text, Vector4 color, float? width = null)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.Text, color);
        CenterTextAligned(text, width);
    }

    public static void CenterColorTextAligned(string text, uint color, float? width = null)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.Text, color);
        CenterTextAligned(text, width);
    }

    /// <summary> What it says on the tin. </summary>
    public static void ColorTextWrapped(string text, Vector4 color)
    {
        using var raiicolor = ImRaii.PushColor(ImGuiCol.Text, color);
        TextWrapped(text);
    }

    /// <summary> Helper function to draw the outlined font in ImGui. </summary>
    public static void OutlinedFont(string text, Vector4 fontColor, Vector4 outlineColor, int thickness)
    {
        Vector2 original = ImGui.GetCursorPos();
        using (ImRaii.PushColor(ImGuiCol.Text, outlineColor))
            OutlinedFontOutline(original, text, thickness);

        using (ImRaii.PushColor(ImGuiCol.Text, fontColor))
            OutlinedFontText(original, text);
    }

    public static void OutlinedFont(string text, uint fontColor, uint outlineColor, int thickness)
    {
        Vector2 original = ImGui.GetCursorPos();
        using (ImRaii.PushColor(ImGuiCol.Text, outlineColor))
            OutlinedFontOutline(original, text, thickness);

        using (ImRaii.PushColor(ImGuiCol.Text, fontColor))
            OutlinedFontText(original, text);
    }

    private static void OutlinedFontOutline(Vector2 original, string text, int thickness)
    {
        ImGui.SetCursorPos(original with { Y = original.Y - thickness });
        ImGui.TextUnformatted(text);
        ImGui.SetCursorPos(original with { X = original.X - thickness });
        ImGui.TextUnformatted(text);
        ImGui.SetCursorPos(original with { Y = original.Y + thickness });
        ImGui.TextUnformatted(text);
        ImGui.SetCursorPos(original with { X = original.X + thickness });
        ImGui.TextUnformatted(text);
        ImGui.SetCursorPos(original with { X = original.X - thickness, Y = original.Y - thickness });
        ImGui.TextUnformatted(text);
        ImGui.SetCursorPos(original with { X = original.X + thickness, Y = original.Y + thickness });
        ImGui.TextUnformatted(text);
        ImGui.SetCursorPos(original with { X = original.X - thickness, Y = original.Y + thickness });
        ImGui.TextUnformatted(text);
        ImGui.SetCursorPos(original with { X = original.X + thickness, Y = original.Y - thickness });
        ImGui.TextUnformatted(text);
    }
    private static void OutlinedFontText(Vector2 original, string text)
    {
        ImGui.SetCursorPos(original);
        ImGui.TextUnformatted(text);
        ImGui.SetCursorPos(original);
        ImGui.TextUnformatted(text);
    }

    public static Vector2 CalcFontTextSize(string text, IFontHandle fontHandle = null!)
    {
        if (fontHandle is null)
            return ImGui.CalcTextSize(text);

        using (fontHandle.Push())
            return ImGui.CalcTextSize(text);
    }

    // Because ImGui.CalcTextSize is unreliable for larger font scales.
    public static unsafe Vector2 CalcTextSizeFontPtr(ImFontPtr fontPtr, string text)
    {
        if (!fontPtr.IsLoaded() || string.IsNullOrEmpty(text))
            return Vector2.Zero;

        float width = 0;
        float scale = fontPtr.Scale; // Important for ImGui fonts!
        ushort? prevChar = null;

        for (int i = 0; i < text.Length; i++)
        {
            ushort current = text[i];

            if (prevChar.HasValue)
                width += fontPtr.GetDistanceAdjustmentForPair(prevChar.Value, current) * scale;

            ImFontGlyphPtr glyph = fontPtr.FindGlyph(current);
            if (glyph.NativePtr is null)
                continue;

            width += glyph.AdvanceX * scale;
            prevChar = current;
        }

        return new Vector2(width, fontPtr.FontSize);
    }

    public static void CopyableDisplayText(string text, string tooltip = "Click to copy")
    {
        // then when the item is clicked, copy it to clipboard so we can share with others
        if (ImGui.IsItemClicked())
        {
            ImGui.SetClipboardText(text);
        }
        AttachToolTip(tooltip);
    }

    public static void TextWrapped(string text)
    {
        ImGui.PushTextWrapPos(0);
        ImGui.TextUnformatted(text);
        ImGui.PopTextWrapPos();
    }

    /// <summary> Draws iconText centered within ImGui.GetFrameHeight() square. </summary>
    public static void FramedIconText(FAI icon, Vector4 color)
        => FramedIconText(icon, CkGui.Color(color));

    /// <summary> Draws iconText centered within ImGui.GetFrameHeight() square. </summary>
    public static void FramedIconText(FAI icon, uint? color = null)
    {
        Vector2 region = new Vector2(ImGui.GetFrameHeight());

        using var font = Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push();
        // Get the text size.
        string text = icon.ToIconString();
        Vector2 iconSize = ImGui.CalcTextSize(text);
        Vector2 currentPos = ImGui.GetCursorScreenPos();
        Vector2 iconPosition = currentPos + (region - iconSize) * 0.5f;
        // Draw a dummy to fill the frame region.
        ImGui.Dummy(region);
        ImGui.GetWindowDrawList().AddText(iconPosition, color ?? ImGui.GetColorU32(ImGuiCol.Text), text);
    }

    public static void FramedHoverIconText(FAI icon, uint hoverCol, uint? color = null)
    {
        var region = new Vector2(ImGui.GetFrameHeight());
        var col = color ?? ImGui.GetColorU32(ImGuiCol.Text);

        using var font = Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push();
        // Get the text size.
        string text = icon.ToIconString();
        Vector2 iconSize = ImGui.CalcTextSize(text);
        Vector2 currentPos = ImGui.GetCursorScreenPos();
        Vector2 iconPosition = currentPos + (region - iconSize) * 0.5f;
        // Draw a dummy to fill the frame region.
        ImGui.Dummy(region);
        if (ImGui.IsItemHovered())
            col = hoverCol;
        ImGui.GetWindowDrawList().AddText(iconPosition, col, text);
    }

    public static void IconText(FAI icon, uint color)
    {
        FontText(icon.ToIconString(), Svc.PluginInterface.UiBuilder.IconFontHandle, color);
    }

    public static void IconText(FAI icon, Vector4? color = null)
    {
        IconText(icon, color == null ? ImGui.GetColorU32(ImGuiCol.Text) : ImGui.GetColorU32(color.Value));
    }

    public static void FontText(string text, IFontHandle font, Vector4? color = null)
    {
        FontText(text, font, color == null ? ImGui.GetColorU32(ImGuiCol.Text) : ImGui.GetColorU32(color.Value));
    }

    public static void FontText(string text, IFontHandle font, uint color)
    {
        using var pushedFont = font.Push();
        using var pushedColor = ImRaii.PushColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(text);
    }

    public static void FontTextCentered(string text, IFontHandle font, Vector4? color = null)
    {
        FontTextCentered(text, font, color == null ? ImGui.GetColorU32(ImGuiCol.Text) : ImGui.GetColorU32(color.Value));
    }

    public static void FontTextCentered(string text, IFontHandle font, uint color)
    {
        using var pushedFont = font.Push();
        using var pushedColor = ImRaii.PushColor(ImGuiCol.Text, color);
        ImGuiUtil.Center(text);
    }

    // Helper function to draw an input text for a set width, with an icon drawn right aligned.
    public static bool IconInputText(string label, float width, FAI icon, string hint, ref string input, int length, ITFlags flags = ITFlags.None)
    {
        using var _ = ImRaii.Group();
        // Draw input text with hint below.
        ImGui.SetNextItemWidth(width);
        var changed = ImGui.InputTextWithHint(label, hint, ref input, (uint)length, flags);
        ImGui.SetCursorScreenPos(ImGui.GetItemRectMax() - new Vector2(ImGui.GetFrameHeight()));
        //ImGui.SameLine(ImGui.GetItemRectSize().X - ImGui.GetFrameHeight());
        FramedIconText(icon, ImGui.GetColorU32(ImGuiCol.TextDisabled));
        return changed;
    }

    // Helper function to draw an input text for a set width, with an icon drawn right aligned.
    public static bool IconInputTextOuter(string id, float width, FAI icon, string hint, ref string input, int length, ITFlags flags = ITFlags.None)
    {
        using var _ = ImRaii.Group();
        // Draw input text with hint below.
        ImGui.SetNextItemWidth(width - ImGui.GetFrameHeight() - ImGui.GetStyle().ItemInnerSpacing.X);
        var changed = ImGui.InputTextWithHint(id, hint, ref input, (uint)length, flags);
        ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
        FramedIconText(icon, ImGui.GetColorU32(ImGuiCol.TextDisabled));
        return changed;
    }
}
