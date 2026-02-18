using CkCommons.Gui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;

namespace CkCommons.Raii;
public static partial class CkRaii
{
    /// <inheritdoc cref="HeaderChild(string, Vector2, float, HeaderFlags)"/>"
    public static IEOContainer HeaderChild(string text, Vector2 size, HeaderFlags flags = HeaderFlags.AlignCenter)
        => HeaderChild(text, size, CkStyle.HeaderRounding(), flags);

    /// <summary> Creates a Head with the labeled text, and a child beneath it. </summary>
    /// <remarks> The inner Width after padding is applied can be found in the returned IEndObject </remarks>
    public static IEOContainer HeaderChild(string text, Vector2 size, float rounding, HeaderFlags flags = HeaderFlags.AlignCenter)
    {
        ImGui.BeginGroup();

        var wdl = ImGui.GetWindowDrawList();
        var min = ImGui.GetCursorScreenPos();
        float lineH = 2 * ImGuiHelpers.GlobalScale;
        var headerSize = new Vector2(size.X, ImGui.GetFrameHeight() + lineH);
        var max = min + headerSize;
        var linePos = min + new Vector2(0, ImGui.GetFrameHeight());

        // Draw the header.
        wdl.AddRectFilled(min, max, CkCol.HChild.Uint(), rounding, DFlags.RoundCornersTop);
        wdl.AddLine(linePos, linePos with { X = max.X }, CkCol.HChildSplit.Uint(), lineH);
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
        return new EndObjectContainer(() =>
            {
                ImGui.EndChild();
                ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), CkCol.HChildBg.Uint(), rounding, DFlags.RoundCornersBottom);
                ImGui.EndGroup();
            },
            ImGui.BeginChild("CHC_" + text, innerSize, false, WFlags.AlwaysUseWindowPadding),
            innerSize.WithoutWinPadding()
        );
    }

    /// <inheritdoc cref="ButtonHeaderChild(string, Vector2, Action, float, string, HeaderFlags)"/>
    public static IEOContainer ButtonHeaderChild(string text, Action act, string? tt = null, HeaderFlags flags = HeaderFlags.AlignCenter)
        => ButtonHeaderChild(text, ImGui.GetContentRegionAvail(), act, CkStyle.HeaderRounding(), tt, flags);


    /// <inheritdoc cref="ButtonHeaderChild(string, Vector2, Action, float, string, HeaderFlags)"/>
    public static IEOContainer ButtonHeaderChild(string text, Vector2 size, Action act, string? tt = null, HeaderFlags flags = HeaderFlags.AlignCenter)
        => ButtonHeaderChild(text, size, act, CkStyle.HeaderRounding(), tt, flags);

    /// <summary> Interactable Button Header that has a child body. </summary>
    /// <remarks> WindowPadding is always applied. Size passed in should be the size of the inner child space after padding. </remarks>
    public static IEOContainer ButtonHeaderChild(string text, Vector2 size, Action act, float rounding, string? tt = null, HeaderFlags flags = HeaderFlags.AlignCenter)
    {
        ImGui.BeginGroup();
        ImDrawListPtr wdl = ImGui.GetWindowDrawList();
        Vector2 min = ImGui.GetCursorScreenPos();
        float lineH = 2 * ImGuiHelpers.GlobalScale;
        Vector2 headerSize = new Vector2(size.X, ImGui.GetFrameHeight() + lineH);
        Vector2 max = min + headerSize;
        Vector2 linePos = min + new Vector2(0, ImGui.GetFrameHeight());

        wdl.AddRectFilled(min, max, CkCol.HChild.Uint(), rounding, DFlags.RoundCornersTop);
        wdl.AddLine(linePos, linePos with { X = max.X }, CkCol.HChildSplit.Uint(), lineH);

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
            CkGui.ToolTipInternal(tt);

        // Adjust the cursor.
        ImGui.SetCursorScreenPos(min + new Vector2(0, headerSize.Y));
        // Correctly retrieve the height.
        float height = ((flags & HeaderFlags.SizeIncludesHeader) != 0) ? size.Y - headerSize.Y : size.Y;
        Vector2 innerSize = new Vector2(size.X, height);

        // Return the EndObjectContainer with the child, and the inner region.
        return new EndObjectContainer(() =>
            {
                ImGui.EndChild();
                ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), CkCol.HChildBg.Uint(), rounding, DFlags.RoundCornersBottom);
                ImGui.EndGroup();
            },
            ImGui.BeginChild("CHC_" + text, innerSize, false, WFlags.AlwaysUseWindowPadding),
            innerSize.WithoutWinPadding()
        );
    }

    public static IEOContainer IconButtonHeaderChild(string text, FAI icon, Vector2 size, Action act, HeaderFlags flags = HeaderFlags.AlignCenter, string tt = "")
        => IconButtonHeaderChild(text, icon, size, act, CkStyle.HeaderRounding(), flags, tt);

    /// <summary> Interactable Button Header that has a child body. </summary>
    /// <remarks> WindowPadding is always applied. Size passed in should be the size of the inner child space after padding. </remarks>
    public static IEOContainer IconButtonHeaderChild(string text, FAI icon, Vector2 size, Action act, float rounding, HeaderFlags flags = HeaderFlags.AlignCenter, string tt = "")
    {
        ImGui.BeginGroup();

        ImDrawListPtr wdl = ImGui.GetWindowDrawList();
        Vector2 min = ImGui.GetCursorScreenPos();
        float lineH = 2 * ImGuiHelpers.GlobalScale;
        Vector2 headerSize = new Vector2(size.X, ImGui.GetFrameHeight() + lineH);
        Vector2 max = min + headerSize;
        Vector2 linePos = min + new Vector2(0, ImGui.GetFrameHeight());

        wdl.AddRectFilled(min, max, CkCol.HChild.Uint(), rounding, DFlags.RoundCornersTop);
        wdl.AddLine(linePos, linePos with { X = max.X }, CkCol.HChildSplit.Uint(), lineH);

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
        return new EndObjectContainer(() =>
            {
                ImGui.EndChild();
                ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), CkCol.HChildBg.Uint(), rounding, DFlags.RoundCornersBottom);
                ImGui.EndGroup();
            },
            ImGui.BeginChild("CHC_" + text, innerSize, false, WFlags.AlwaysUseWindowPadding),
            innerSize.WithoutWinPadding()
        );
    }

    public static IEOContainer CustomHeaderChild(string id, Action act, HeaderFlags hf = HeaderFlags.AddPaddingToHeight, DFlags df = DFlags.None)
        => CustomHeaderChild(id, ImGui.GetContentRegionAvail(), act, CkStyle.HeaderRounding(), hf, df);


    public static IEOContainer CustomHeaderChild(string id, Vector2 size, Action act, HeaderFlags hf = HeaderFlags.AddPaddingToHeight, DFlags df = DFlags.None)
        => CustomHeaderChild(id, size, act, CkStyle.HeaderRounding(), hf, df);

    /// <summary> Custom drawn header above a child body. </summary>
    /// <remarks> WindowPadding is always applied. Size passed in should be the size of the inner child space after padding. </remarks>
    public static IEOContainer CustomHeaderChild(string id, Vector2 size, Action drawHeader, float rounding, HeaderFlags hf = HeaderFlags.AddPaddingToHeight, DFlags df = DFlags.None)
    {
        ImDrawListPtr wdl = ImGui.GetWindowDrawList();
        Vector2 startPos = ImGui.GetCursorScreenPos();
        ImGui.BeginGroup();

        // Begin a sub-group for the header
        ImGui.BeginGroup();
        drawHeader.Invoke();
        ImGui.EndGroup();

        // Measure header bounds
        Vector2 headerMin = ImGui.GetItemRectMin();
        Vector2 headerMax = ImGui.GetItemRectMax();
        float headerHeight = headerMax.Y - headerMin.Y;

        // Background/line drawing AFTER measuring header
        wdl.AddRectFilled(headerMin, headerMax, CkCol.HChild.Uint(), rounding, DFlags.RoundCornersTop);
        wdl.AddLine(new Vector2(headerMin.X, headerMax.Y), new Vector2(headerMax.X, headerMax.Y), CkCol.HChildSplit.Uint(), 2f);

        // get the height for the body child, this should be based on our flags.
        float height = ((hf & HeaderFlags.SizeIncludesHeader) != 0) ? size.Y - headerHeight : size.Y;
        ImGui.SetCursorScreenPos(headerMin with { Y = headerMax.Y });
        Vector2 innerSize = new Vector2(size.X, height);

        // Return the EndObjectContainer with the child, and the inner region.
        return new EndObjectContainer(() =>
            {
                ImGui.EndChild();
                ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), CkCol.HChildBg.Uint(), rounding, DFlags.RoundCornersBottom);
                ImGui.EndGroup();
            },
            ImGui.BeginChild("CHC_" + id, innerSize, false, WFlags.AlwaysUseWindowPadding),
            innerSize.WithoutWinPadding()
        );
    }


}
