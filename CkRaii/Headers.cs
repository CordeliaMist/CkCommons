using CkCommons.Gui;
using CkCommons.Services;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace CkCommons.Raii;
public static partial class CkRaii
{
    /// <inheritdoc cref="HeaderChild(string, Vector2, HeaderChildColors, float, HeaderFlags)"/>"
    public static IEOContainer HeaderChild(string text, Vector2 size, HeaderFlags flags = HeaderFlags.AlignCenter)
        => HeaderChild(text, size, HeaderChildColors.Default, CkStyle.HeaderRounding(), flags);


    /// <inheritdoc cref="HeaderChild(string, Vector2, HeaderChildColors, float, HeaderFlags)"/>"
    public static IEOContainer HeaderChild(string text, Vector2 size, HeaderChildColors colors, HeaderFlags flags = HeaderFlags.AlignCenter)
        => HeaderChild(text, size, colors, CkStyle.HeaderRounding(), flags);


    /// <inheritdoc cref="HeaderChild(string, Vector2, HeaderChildColors, float, HeaderFlags)"/>"
    public static IEOContainer HeaderChild(string text, Vector2 size, float rounding, HeaderFlags flags = HeaderFlags.AlignCenter)
        => HeaderChild(text, size, HeaderChildColors.Default, rounding, flags);

    /// <summary> Creates a Head with the labeled text, and a child beneath it. </summary>
    /// <remarks> The inner Width after padding is applied can be found in the returned IEndObject </remarks>
    public static IEOContainer HeaderChild(string text, Vector2 size, HeaderChildColors colors, float rounding, HeaderFlags flags = HeaderFlags.AlignCenter)
    {
        ImGui.BeginGroup();

        ImDrawListPtr wdl = ImGui.GetWindowDrawList();
        Vector2 min = ImGui.GetCursorScreenPos();
        float lineH = 2 * ImGuiHelpers.GlobalScale;
        Vector2 headerSize = new Vector2(size.X, ImGui.GetFrameHeight() + lineH);
        Vector2 max = min + headerSize;
        Vector2 linePos = min + new Vector2(0, ImGui.GetFrameHeight());

        // Draw the header.
        wdl.AddRectFilled(min, max, colors.HeaderColor, rounding, ImDrawFlags.RoundCornersTop);
        wdl.AddLine(linePos, linePos with { X = max.X }, colors.SplitColor, lineH);
        Vector2 textStart = HeaderTextOffset(size.X, ImGui.GetFrameHeight(), ImGui.CalcTextSize(text).X, flags);
        wdl.AddText(min + textStart, ImGui.GetColorU32(ImGuiCol.Text), text);

        // Adjust the cursor.
        ImGui.SetCursorScreenPos(min + new Vector2(0, headerSize.Y));
        // Correctly retrieve the height.
        float height = size.Y;
        if ((flags & HeaderFlags.SizeIncludesHeader) != 0) height -= headerSize.Y;
        if ((flags & HeaderFlags.AddPaddingToHeight) != 0) height += ImGui.GetStyle().WindowPadding.Y * 2;

        Vector2 innerSize = new Vector2(size.X, height);

        // Return the EndObjectContainer with the child, and the inner region.
        return new EndObjectContainer(
            () => HeaderChildEndAction(colors.BodyColor, rounding),
            ImGui.BeginChild("CHC_" + text, innerSize, false, WFlags.AlwaysUseWindowPadding),
            innerSize.WithoutWinPadding()
        );
    }

    /// <inheritdoc cref="ButtonHeaderChild(string, Vector2, Action, HeaderChildColors, float, HeaderFlags, string)"/>
    public static ImRaii.IEndObject ButtonHeaderChild(string text, Action act, HeaderFlags flags = HeaderFlags.AlignCenter, string tt = "")
        => ButtonHeaderChild(text, ImGui.GetContentRegionAvail(), act, HeaderChildColors.Default, CkStyle.HeaderRounding(), flags, tt);


    /// <inheritdoc cref="ButtonHeaderChild(string, Vector2, Action, HeaderChildColors, float, HeaderFlags, string)"/>
    public static ImRaii.IEndObject ButtonHeaderChild(string text, Vector2 size, Action act, HeaderFlags flags = HeaderFlags.AlignCenter, string tt = "")
        => ButtonHeaderChild(text, size, act, HeaderChildColors.Default, CkStyle.HeaderRounding(), flags, tt);


    /// <inheritdoc cref="ButtonHeaderChild(string, Vector2, Action, HeaderChildColors, float, HeaderFlags, string)"/>
    public static ImRaii.IEndObject ButtonHeaderChild(string text, Vector2 size, Action act, float rounding, HeaderFlags flags = HeaderFlags.AlignCenter, string tt = "")
        => ButtonHeaderChild(text, size, act, HeaderChildColors.Default, rounding, flags, tt);

    /// <inheritdoc cref="ButtonHeaderChild(string, Vector2, Action, HeaderChildColors, float, HeaderFlags, string)"/>
    public static ImRaii.IEndObject ButtonHeaderChild(string text, Vector2 size, Action act, HeaderChildColors colors, HeaderFlags flags = HeaderFlags.AlignCenter, string tt = "")
        => ButtonHeaderChild(text, size, act, colors, CkStyle.HeaderRounding(), flags, tt);

    /// <summary> Interactable Button Header that has a child body. </summary>
    /// <remarks> WindowPadding is always applied. Size passed in should be the size of the inner child space after padding. </remarks>
    public static ImRaii.IEndObject ButtonHeaderChild(string text, Vector2 size, Action act, HeaderChildColors colors, float rounding, HeaderFlags flags = HeaderFlags.AlignCenter, string tt = "")
    {
        ImGui.BeginGroup();

        ImDrawListPtr wdl = ImGui.GetWindowDrawList();
        Vector2 min = ImGui.GetCursorScreenPos();
        float lineH = 2 * ImGuiHelpers.GlobalScale;
        Vector2 headerSize = new Vector2(size.X, ImGui.GetFrameHeight() + lineH);
        Vector2 max = min + headerSize;
        Vector2 linePos = min + new Vector2(0, ImGui.GetFrameHeight());

        wdl.AddRectFilled(min, max, CkColor.ElementHeader.Uint(), rounding, ImDrawFlags.RoundCornersTop);
        wdl.AddLine(linePos, linePos with { X = max.X }, CkColor.ElementSplit.Uint(), lineH);

        // Text & Icon Alignment
        float textWidth = ImGui.CalcTextSize(text).X;
        Vector2 textStart = min + HeaderTextOffset(size.X, ImGui.GetFrameHeight(), textWidth, flags);
        Vector2 hoverSize = new Vector2((size.X - textWidth) / 2, ImGui.GetFrameHeight());

        // Text & Icon Drawing.
        bool isHovered = ImGui.IsMouseHoveringRect(textStart, textStart + hoverSize);
        uint col = isHovered ? ImGui.GetColorU32(ImGuiCol.ButtonHovered) : ImGui.GetColorU32(ImGuiCol.Text);
        ImGui.GetWindowDrawList().AddText(textStart, col, text);

        // Action Handling.
        if (isHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            act();

        // tooltip handling.
        if (isHovered && !string.IsNullOrEmpty(tt))
            CkGui.AttachToolTip(tt);

        // Adjust the cursor.
        ImGui.SetCursorScreenPos(min + new Vector2(0, headerSize.Y));
        // Correctly retrieve the height.
        float height = ((flags & HeaderFlags.SizeIncludesHeader) != 0) ? size.Y - headerSize.Y : size.Y;
        Vector2 innerSize = new Vector2(size.X, height);

        // Return the EndObjectContainer with the child, and the inner region.
        return new EndObjectContainer(
            () => HeaderChildEndAction(colors.BodyColor, rounding),
            ImGui.BeginChild("CHC_" + text, innerSize, false, WFlags.AlwaysUseWindowPadding),
            innerSize.WithoutWinPadding()
        );
    }

    public static ImRaii.IEndObject IconButtonHeaderChild(string text, FAI icon, Vector2 size, Action act, HeaderFlags flags = HeaderFlags.AlignCenter, string tt = "")
        => IconButtonHeaderChild(text, icon, size, act, HeaderChildColors.Default, CkStyle.HeaderRounding(), flags, tt);

    public static ImRaii.IEndObject IconButtonHeaderChild(string text, FAI icon, Vector2 size, Action act, float rounding, HeaderFlags flags = HeaderFlags.AlignCenter, string tt = "")
        => IconButtonHeaderChild(text, icon, size, act, HeaderChildColors.Default, rounding, flags, tt);

    public static ImRaii.IEndObject IconButtonHeaderChild(string text, FAI icon, Vector2 size, Action act, HeaderChildColors colors, HeaderFlags flags = HeaderFlags.AlignCenter, string tt = "")
        => IconButtonHeaderChild(text, icon, size, act, colors, CkStyle.HeaderRounding(), flags, tt);

    /// <summary> Interactable Button Header that has a child body. </summary>
    /// <remarks> WindowPadding is always applied. Size passed in should be the size of the inner child space after padding. </remarks>
    public static ImRaii.IEndObject IconButtonHeaderChild(string text, FAI icon, Vector2 size, Action act, HeaderChildColors colors, float rounding, HeaderFlags flags = HeaderFlags.AlignCenter, string tt = "")
    {
        ImGui.BeginGroup();

        ImDrawListPtr wdl = ImGui.GetWindowDrawList();
        Vector2 min = ImGui.GetCursorScreenPos();
        float lineH = 2 * ImGuiHelpers.GlobalScale;
        Vector2 headerSize = new Vector2(size.X, ImGui.GetFrameHeight() + lineH);
        Vector2 max = min + headerSize;
        Vector2 linePos = min + new Vector2(0, ImGui.GetFrameHeight());

        wdl.AddRectFilled(min, max, colors.HeaderColor, rounding, ImDrawFlags.RoundCornersTop);
        wdl.AddLine(linePos, linePos with { X = max.X }, colors.SplitColor, lineH);

        // Text & Icon Alignment
        float textWidth = ImGui.CalcTextSize(text).X;
        var textIconWidth = textWidth + ImGui.GetStyle().ItemInnerSpacing.X + CkGui.IconSize(icon).X;
        var textStart = min + HeaderTextOffset(size.X, ImGui.GetFrameHeight(), textIconWidth, flags);
        Vector2 hoverSize = new Vector2(textIconWidth, ImGui.GetFrameHeight());

        // Text & Icon Drawing.
        bool isHovered = ImGui.IsMouseHoveringRect(textStart, textStart + hoverSize);
        uint col = isHovered ? ImGui.GetColorU32(ImGuiCol.ButtonHovered) : ImGui.GetColorU32(ImGuiCol.Text);
        ImGui.GetWindowDrawList().AddText(textStart, col, text);

        var centerPos = textStart + new Vector2(textWidth + ImGui.GetStyle().ItemInnerSpacing.X, 0);
        using (Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push()) ImGui.GetWindowDrawList().AddText(centerPos, col, icon.ToIconString());

        // Action Handling.
        if (isHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            act();

        // tooltip handling.
        if (isHovered && !string.IsNullOrEmpty(tt))
            CkGui.AttachToolTip(tt);

        // Adjust the cursor.
        ImGui.SetCursorScreenPos(min + new Vector2(0, headerSize.Y));
        // Correctly retrieve the height.
        float height = ((flags & HeaderFlags.SizeIncludesHeader) != 0) ? size.Y - headerSize.Y : size.Y;
        Vector2 innerSize = new Vector2(size.X, height);

        // Return the EndObjectContainer with the child, and the inner region.
        return new EndObjectContainer(
            () => HeaderChildEndAction(colors.BodyColor, rounding),
            ImGui.BeginChild("CHC_" + text, innerSize, false, WFlags.AlwaysUseWindowPadding),
            innerSize.WithoutWinPadding()
        );
    }
    private static void HeaderChildEndAction(uint bgCol, float rounding)
    {
        ImGui.EndChild();
        ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), bgCol, rounding, ImDrawFlags.RoundCornersBottom);
        ImGui.EndGroup();
    }
}
