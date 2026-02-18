using CkCommons.Gui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using Dalamud.Bindings.ImGui;
using OtterGui.Text;

namespace CkCommons.Raii;
public static partial class CkRaii
{
    public static IEOLabelContainer ChildLabelText(Vector2 size, float widthSpan, string text, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
        => ChildLabelText(size, widthSpan, text, ImGui.GetStyle().WindowPadding.X, CkStyle.FrameThickness(), df, lf);

    public static IEOLabelContainer ChildLabelText(Vector2 size, float widthSpan, string text, float rounding, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
        => ChildLabelText(size, widthSpan, text, rounding, CkStyle.FrameThickness(), df, lf);

    /// <summary> Constructs a Label Child object with a text based header. </summary>
    /// <remarks> The IEOLabelContainer contains the size of the label region and the inner region. </remarks>
    public static IEOLabelContainer ChildLabelText(Vector2 size, float widthSpan, string text, float rounding, float stroke, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
    {
        var width = size.X * Math.Clamp(widthSpan, 0f, 1f);
        var offset = (df & DFlags.RoundCornersTopLeft) != 0 ? rounding : ImGui.GetStyle().WindowPadding.X;

        // Calculate the drawn header size.
        Vector2 hSize = new(width, ImGui.GetFrameHeight());
        var headerStrokeHeight = hSize.Y + stroke;
        // If size includes header, return size, otherwise, if we should add padding, add padding, but regardless, add header height.
        var outerHeight = ((lf & LabelFlags.SizeIncludesHeader) != 0)
            ? size.Y : (lf & LabelFlags.AddPaddingToHeight) != 0
                ? size.Y.AddWinPadY() + headerStrokeHeight : size.Y + headerStrokeHeight;

        var outerSize = new Vector2(size.X, outerHeight);
        var innerSize = outerSize.WithoutWinPadding() - new Vector2(0, headerStrokeHeight);
        // Begin the outer child.
        var success = ImGui.BeginChild($"##LabelTextChild-{text}", outerSize, false, WFlags.AlwaysUseWindowPadding);
        ImGui.Dummy(hSize + new Vector2(stroke - ImGui.GetStyle().ItemSpacing.Y));
        // Return the end object, closing the draw on the child.
        return new EndObjectLabelContainer(() =>
            {
                ImGui.EndChild();
                var min = ImGui.GetItemRectMin();
                var wdl = ImGui.GetWindowDrawList();
                // Draw out the child BG.
                wdl.AddRectFilled(min, ImGui.GetItemRectMax(), CkCol.LChildBg.Uint(), rounding, df);
                // Partial-width: draw shadow + label
                var labelDFlags =  DFlags.RoundCornersBottomRight | ((df & DFlags.RoundCornersTopLeft) != 0 ? DFlags.RoundCornersTopLeft : DFlags.None);
                wdl.AddRectFilled(min, min + hSize + new Vector2(stroke), CkCol.LChildSplit.Uint(), rounding, labelDFlags);
                wdl.AddRectFilled(min, min + hSize, CkCol.LChild.Uint(), rounding, labelDFlags);
                // add the text, centered to the height of the header, left aligned.
                wdl.AddText(min + new Vector2(offset, (hSize.Y - ImGuiNative.GetTextLineHeight()) / 2), ImGui.GetColorU32(ImGuiCol.Text), text);
            },
            success,
            outerSize.WithoutWinPadding(),
            innerSize
        );
    }

    public static IEOLabelContainer ChildLabelTextFull(Vector2 size, string text, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
        => ChildLabelTextFull(size, text, ImGui.GetStyle().WindowPadding.X, CkStyle.FrameThickness(), df, lf);

    public static IEOLabelContainer ChildLabelTextFull(Vector2 size, string text, float rounding, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
        => ChildLabelTextFull(size, text, rounding, CkStyle.FrameThickness(), df, lf);

    /// <summary> Constructs a Label Child object with a text based header. </summary>
    /// <remarks> The IEOLabelContainer contains the size of the label region and the inner region. </remarks>
    public static IEOLabelContainer ChildLabelTextFull(Vector2 size, string text, float rounding, float stroke, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
    {
        var offset = (df & DFlags.RoundCornersTopLeft) != 0 ? rounding : ImGui.GetStyle().WindowPadding.X;

        // Calculate the drawn header size.
        Vector2 hSize = new(size.X, ImGui.GetFrameHeight());
        // If size includes header, return size, otherwise, if we should add padding, add padding, but regardless, add header height.
        var headerStrokeHeight = hSize.Y + stroke;
        var outerHeight = ((lf & LabelFlags.SizeIncludesHeader) != 0)
            ? size.Y : (lf & LabelFlags.AddPaddingToHeight) != 0
                ? size.Y.AddWinPadY() + headerStrokeHeight : size.Y + headerStrokeHeight;

        var outerSize = new Vector2(size.X, outerHeight);
        var innerSize = outerSize.WithoutWinPadding() - new Vector2(0, headerStrokeHeight);
        // Begin the outer child.
        var success = ImGui.BeginChild($"##LabelFullTextChild-{text}", outerSize, false, WFlags.AlwaysUseWindowPadding);
        ImGui.Dummy(hSize + new Vector2(stroke - ImGui.GetStyle().ItemSpacing.Y));
        // Return the end object, closing the draw on the child.
        return new EndObjectLabelContainer(() =>
        {
            ImGui.EndChild();
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            var wdl = ImGui.GetWindowDrawList();

            wdl.AddRectFilled(min, ImGui.GetItemRectMax(), CkCol.LChildBg.Uint(), rounding, df);
            // Determine where the label ends.
            var labelMax = new Vector2(max.X, min.Y + ImGui.GetFrameHeight());
            // we are only drawing the labels now, so adjust our flags for them.
            var labelFlags = df & ~DFlags.RoundCornersBottom;
            // Full-width: draw label + underline with thickness = fade
            wdl.AddRectFilled(min, labelMax, CkCol.LChild.Uint(), rounding, labelFlags);
            var underlineMin = new Vector2(min.X, labelMax.Y);
            var underlineMax = new Vector2(max.X, labelMax.Y + stroke);
            wdl.AddRectFilled(underlineMin, underlineMax, CkCol.LChildSplit.Uint());
            // add the text, centered to the height of the header, left aligned.
            wdl.AddText(min + new Vector2(offset, (hSize.Y - ImGui.GetTextLineHeight()) / 2), ImGui.GetColorU32(ImGuiCol.Text), text);
        },
            success,
            outerSize.WithoutWinPadding(),
            innerSize
        );
    }

    public static IEOLabelContainer ChildLabelButton(Vector2 size, float widthSpan, string text, Action<ImGuiMouseButton>? clicked, string? tt = null, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
        => ChildLabelButton(size, widthSpan, text, CkStyle.ChildRounding(), CkStyle.FrameThickness(), clicked, tt, df, lf);

    public static IEOLabelContainer ChildLabelButton(Vector2 size, float widthSpan, string text, float rounding, Action<ImGuiMouseButton>? clicked, string? tt = null, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
        => ChildLabelButton(size, widthSpan, text, rounding, CkStyle.FrameThickness(), clicked, tt, df, lf);

    /// <summary>
    ///     Interactable label header within a padded child. <para/>
    ///     If you intend to make this scrollable, make another child inside.
    /// </summary>
    /// <remarks> Note that the dummy covering the header is part of the child. </remarks>
    public static IEOLabelContainer ChildLabelButton(Vector2 size, float widthSpan, string text, float rounding, float stroke, Action<ImGuiMouseButton>? clicked, string? tt = null, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
    {
        string tooltip = tt.IsNullOrWhitespace() ? "Double Click to Interact--SEP--Right-Click to Cancel" : tt;
        var offset = (df & DFlags.RoundCornersTopLeft) != 0 ? rounding : ImGui.GetStyle().WindowPadding.X;
        var labelSize = new Vector2(size.X * Math.Clamp(widthSpan, 0f, 1f), ImGui.GetFrameHeight());
        var labelStrokeHeight = labelSize.Y + stroke;
        // If size includes header, return size, otherwise, if we should add padding, add padding, but regardless, add header height.
        var outerHeight = ((lf & LabelFlags.SizeIncludesHeader) != 0)
            ? size.Y : (lf & LabelFlags.AddPaddingToHeight) != 0
                ? size.Y.AddWinPadY() + labelStrokeHeight : size.Y + labelStrokeHeight;

        var outerSize = new Vector2(size.X, outerHeight);
        var innerSize = outerSize.WithoutWinPadding() - new Vector2(0, labelStrokeHeight);
        var labelMin = ImGui.GetCursorScreenPos();
        var labelMax = labelMin + labelSize;
        var hovered = ImGui.IsMouseHoveringRect(labelMin, labelMax);
        if (hovered)
        {
            if (!string.IsNullOrEmpty(tt)) CkGui.ToolTipInternal(tt);
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)) clicked?.Invoke(ImGuiMouseButton.Left);
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right)) clicked?.Invoke(ImGuiMouseButton.Right);
        }

        // Begin the outer child.
        var success = ImGui.BeginChild($"##ChildLabelButton-{text}", outerSize, false, WFlags.AlwaysUseWindowPadding);
        ImGui.Dummy(labelSize + new Vector2(stroke - ImGui.GetStyle().ItemSpacing.Y));

        // Return the end object, closing the draw on the child.
        return new EndObjectLabelContainer(() =>
        {
            ImGui.EndChild();
            var wdl = ImGui.GetWindowDrawList();
            wdl.AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), CkCol.LChildBg.Uint(), rounding, df);
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            // make sure that if the dFlags include DFlags.RoundCornersTopLeft, to apply that flag.
            var labelDFlags = DFlags.RoundCornersBottomRight | ((df & DFlags.RoundCornersTopLeft) != 0 ? DFlags.RoundCornersTopLeft : DFlags.None);
            wdl.AddRectFilled(labelMin, labelMax + new Vector2(stroke), CkCol.LChildSplit.Uint(), rounding, labelDFlags);
            var labelCol = hovered ? CkCol.LChildHovered.Uint() : CkCol.LChild.Uint();
            wdl.AddRectFilled(labelMin, labelMax, labelCol, rounding, labelDFlags);
            // add the text, centered to the height of the header, left aligned.
            wdl.AddText(min + new Vector2(offset, (labelSize.Y - ImGui.GetTextLineHeight()) / 2), ImGui.GetColorU32(ImGuiCol.Text), text);
        },
            success,
            outerSize.WithoutWinPadding(), // these should be flipped.
            innerSize // these should be flipped (too late to deal with this)
        );
    }

    public static IEOLabelContainer ChildLabelButtonFull(Vector2 size, string text, Action<ImGuiMouseButton>? clicked, string? tt = null, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
    => ChildLabelButtonFull(size, text, CkStyle.ChildRounding(), CkStyle.FrameThickness(), clicked, tt, df, lf);

    public static IEOLabelContainer ChildLabelButtonFull(Vector2 size, string text, float rounding, Action<ImGuiMouseButton>? clicked, string? tt = null, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
        => ChildLabelButtonFull(size, text, rounding, CkStyle.FrameThickness(), clicked, tt, df, lf);

    /// <summary>
    ///     Interactable label header within a padded child. <para/>
    ///     If you intend to make this scrollable, make another child inside.
    /// </summary>
    /// <remarks> Note that the dummy covering the header is part of the child. </remarks>
    public static IEOLabelContainer ChildLabelButtonFull(Vector2 size, string text, float rounding, float stroke, Action<ImGuiMouseButton>? clicked, string? tt = null, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
    {
        string tooltip = tt.IsNullOrWhitespace() ? "Double Click to Interact--SEP--Right-Click to Cancel" : tt;
        var offset = (df & DFlags.RoundCornersTopLeft) != 0 ? rounding : ImGui.GetStyle().WindowPadding.X;
        var labelSize = new Vector2(size.X, ImGui.GetFrameHeight());
        var labelStrokeHeight = labelSize.Y + stroke;
        // If size includes header, return size, otherwise, if we should add padding, add padding, but regardless, add header height.
        var outerHeight = ((lf & LabelFlags.SizeIncludesHeader) != 0)
            ? size.Y : (lf & LabelFlags.AddPaddingToHeight) != 0
                ? size.Y.AddWinPadY() + labelStrokeHeight : size.Y + labelStrokeHeight;

        var outerSize = new Vector2(size.X, outerHeight);
        var innerSize = outerSize.WithoutWinPadding() - new Vector2(0, labelStrokeHeight);
        // Begin the outer child.
        var success = ImGui.BeginChild($"##ChildLabelButton-{text}", outerSize, false, WFlags.AlwaysUseWindowPadding);
        ImGui.Dummy(labelSize + new Vector2(stroke - ImGui.GetStyle().ItemSpacing.Y));
        
        var labelMin = ImGui.GetItemRectMin();
        var labelMax = labelMin + labelSize;
        var hovered = ImGui.IsMouseHoveringRect(labelMin, labelMax);
        if (hovered)
        {
            if (!string.IsNullOrEmpty(tt)) CkGui.ToolTipInternal(tt);
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)) clicked?.Invoke(ImGuiMouseButton.Left);
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right)) clicked?.Invoke(ImGuiMouseButton.Right);
        }

        // Return the end object, closing the draw on the child.
        return new EndObjectLabelContainer(() =>
        {
            ImGui.EndChild();
            var wdl = ImGui.GetWindowDrawList();
            wdl.AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), CkCol.LChildBg.Uint(), rounding, df);
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();

            var labelMax = new Vector2(max.X, min.Y + labelSize.Y);
            // we are only drawing the labels now, so adjust our flags for them.
            var labelFlags = df & ~DFlags.RoundCornersBottom;
            // Full-width: draw label + underline with thickness = fade
            var labelCol = hovered ? CkCol.LChildHovered.Uint() : CkCol.LChild.Uint();
            wdl.AddRectFilled(min, labelMax, labelCol, rounding, labelFlags);
            var underlineMin = new Vector2(min.X, labelMax.Y);
            var underlineMax = new Vector2(max.X, labelMax.Y + stroke);
            wdl.AddRectFilled(underlineMin, underlineMax, CkCol.LChildSplit.Uint());
            // add the text, centered to the height of the header, left aligned.
            wdl.AddText(min + new Vector2(offset, (labelSize.Y - ImGui.GetTextLineHeight()) / 2), ImGui.GetColorU32(ImGuiCol.Text), text);
        },
            success,
            outerSize.WithoutWinPadding(),
            innerSize
        );
    }

    public static IEOLabelContainer ChildLabelCustomButton(string id, Vector2 size, Action label, Action<ImGuiMouseButton>? clicked, string? tt = null, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
        => ChildLabelCustomButton(id, size, label, CkStyle.ChildRounding(), CkStyle.FrameThickness(), clicked, tt, df, lf);

    public static IEOLabelContainer ChildLabelCustomButton(string id, Vector2 size, float rounding, Action label, Action<ImGuiMouseButton>? clicked, string? tt = null, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
        => ChildLabelCustomButton(id, size, label, rounding, CkStyle.FrameThickness(), clicked, tt, df, lf);

    /// <summary>
    ///     Interactable label header within a padded child. <para/>
    ///     If you intend to make this scrollable, make another child inside.
    /// </summary>
    /// <remarks> Note that the dummy covering the header is part of the child. </remarks>
    public static IEOLabelContainer ChildLabelCustomButton(string id, Vector2 size, Action label, float rounding, float stroke, Action<ImGuiMouseButton>? clicked, string? tt = null, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
    {
        // Begin the group that spans the outer area.
        var wdl = ImGui.GetWindowDrawList();
        var pos = ImGui.GetCursorScreenPos();
        ImGui.BeginGroup();
        // Handle drawing the label.
        ImGui.BeginGroup();
        label?.Invoke();
        ImGui.EndGroup();
        var labelMax = ImGui.GetItemRectMax();
        var labelSize = labelMax - pos;
        var labelStrokeHeight = labelSize.Y + stroke;
        var hovered = ImGui.IsMouseHoveringRect(pos, labelMax);
        if (hovered)
        {
            if (!string.IsNullOrEmpty(tt)) CkGui.ToolTipInternal(tt);
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)) clicked?.Invoke(ImGuiMouseButton.Left);
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right)) clicked?.Invoke(ImGuiMouseButton.Right);
        }

        // If size includes header, return size, otherwise, if we should add padding, add padding, but regardless, add header height.
        var outerHeight = ((lf & LabelFlags.SizeIncludesHeader) != 0)
            ? size.Y : (lf & LabelFlags.AddPaddingToHeight) != 0
                ? size.Y.AddWinPadY() + labelStrokeHeight : size.Y + labelStrokeHeight;

        var outerSize = new Vector2(size.X, outerHeight);
        ImGui.SetCursorScreenPos(pos);
        bool success = ImGui.BeginChild($"##ChildLabelCustomButton-{id}", outerSize, false, WFlags.AlwaysUseWindowPadding);
        ImGui.Dummy(labelSize + new Vector2(stroke - ImGui.GetStyle().ItemSpacing.Y));
        var innerSize = outerSize.WithoutWinPadding() - new Vector2(0, labelStrokeHeight);

        // Return the end object, closing the draw on the child.
        return new EndObjectLabelContainer(() =>
        {
            ImGui.EndChild();
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            wdl.AddRectFilled(min, max, CkCol.LChildBg.Uint(), rounding, df);
            // we are only drawing the labels now, so adjust our flags for them.
            var labelFlags = df & ~DFlags.RoundCornersBottom;
            // make sure that if the dFlags include DFlags.RoundCornersTopLeft, to apply that flag.
            DFlags labelDFlags = DFlags.RoundCornersBottomRight | ((df & DFlags.RoundCornersTopLeft) != 0 ? DFlags.RoundCornersTopLeft : DFlags.None);
            wdl.AddRectFilled(pos, labelMax + new Vector2(stroke), CkCol.LChildSplit.Uint(), rounding, labelDFlags);
            var labelCol = hovered ? CkCol.LChildHovered.Uint() : CkCol.LChild.Uint();
            wdl.AddRectFilled(pos, labelMax, labelCol, rounding, labelDFlags);
            ImGui.EndGroup();
        },
            success,
            outerSize.WithoutWinPadding(),
            innerSize
        );
    }

    public static IEOLabelContainer ChildLabelCustomButtonFull(string id, Vector2 size, Action label, Action<ImGuiMouseButton>? clicked, string? tt = null, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
        => ChildLabelCustomButtonFull(id, size, CkStyle.ChildRounding(), CkStyle.FrameThickness(), label, clicked, tt, df, lf);

    public static IEOLabelContainer ChildLabelCustomButtonFull(string id, Vector2 size, float rounding, Action label, Action<ImGuiMouseButton>? clicked, string? tt = null, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
        => ChildLabelCustomButtonFull(id, size, rounding, CkStyle.FrameThickness(), label, clicked, tt, df, lf);

    /// <summary>
    ///     Interactable label header within a padded child. <para/>
    ///     If you intend to make this scrollable, make another child inside.
    /// </summary>
    /// <remarks> Note that the dummy covering the header is part of the child. </remarks>
    public static IEOLabelContainer ChildLabelCustomButtonFull(string id, Vector2 size, float rounding, float stroke, Action label, Action<ImGuiMouseButton>? clicked, string? tt = null, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
    {
        // Begin the group that spans the outer area.
        var wdl = ImGui.GetWindowDrawList();
        var pos = ImGui.GetCursorScreenPos();
        ImGui.BeginGroup();

        // Handle drawing the label.
        using (ImRaii.Group())
        {
            ImGui.Dummy(new Vector2(size.X, ImGui.GetFrameHeight()));
            ImGui.SetCursorScreenPos(pos);
            label?.Invoke();
        }
        var labelMax = ImGui.GetItemRectMax();
        var labelSize = labelMax - pos;
        var hovered = ImGui.IsMouseHoveringRect(pos, labelMax);
        if (hovered)
        {
            if (!string.IsNullOrEmpty(tt)) CkGui.ToolTipInternal(tt);
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)) clicked?.Invoke(ImGuiMouseButton.Left);
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right)) clicked?.Invoke(ImGuiMouseButton.Right);
        }

        var labelStrokeHeight = labelSize.Y + stroke;
        // If size includes header, return size, otherwise, if we should add padding, add padding, but regardless, add header height.
        var outerHeight = ((lf & LabelFlags.SizeIncludesHeader) != 0)
            ? size.Y : (lf & LabelFlags.AddPaddingToHeight) != 0
                ? size.Y.AddWinPadY() + labelStrokeHeight : size.Y + labelStrokeHeight;

        var outerSize = new Vector2(size.X, outerHeight);
        var innerSize = outerSize.WithoutWinPadding() - new Vector2(0, labelStrokeHeight);

        ImGui.SetCursorScreenPos(pos);
        bool success = ImGui.BeginChild($"##ChildLabelCustomButton-{id}", outerSize, false, WFlags.AlwaysUseWindowPadding);
        ImGui.Dummy(labelSize + new Vector2(stroke - ImGui.GetStyle().ItemSpacing.Y));

        // Return the end object, closing the draw on the child.
        return new EndObjectLabelContainer(() =>
        {
            ImGui.EndChild();
            var wdl = ImGui.GetWindowDrawList();
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            wdl.AddRectFilled(min, max, CkCol.LChildBg.Uint(), rounding, df);
            // we are only drawing the labels now, so adjust our flags for them.
            var labelFlags = df & ~DFlags.RoundCornersBottom;
            // Full-width: draw label + underline with thickness = fade
            wdl.AddRectFilled(pos, labelMax, CkCol.LChild.Uint(), rounding, labelFlags);
            var underlineMin = new Vector2(min.X, labelMax.Y);
            var underlineMax = new Vector2(max.X, labelMax.Y + stroke);
            wdl.AddRectFilled(underlineMin, underlineMax, CkCol.LChildSplit.Uint());
        },
            success,
            outerSize.WithoutWinPadding(),
            innerSize
        );
    }


    /// <summary> 
    ///     Custom drawn label, that is attached to a child. <para/>
    ///     If you intend to make this scrollable, make another child inside.
    /// </summary>
    /// <remarks> Note that the dummy covering the header is part of the child. </remarks>
    public static IEOLabelContainer ChildLabelCustom(string id, Vector2 size, Action label, float rounding, float stroke, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
    {
        // Begin the group that spans the outer area.
        var wdl = ImGui.GetWindowDrawList();
        var pos = ImGui.GetCursorScreenPos();
        ImGui.BeginGroup();
        // Handle drawing the label.
        ImGui.BeginGroup();
        label?.Invoke();
        ImGui.EndGroup();
        var labelMax = ImGui.GetItemRectMax();
        var labelSize = labelMax - pos;
        var labelStrokeHeight = labelSize.Y + stroke;
        // If size includes header, return size, otherwise, if we should add padding, add padding, but regardless, add header height.
        var outerHeight = ((lf & LabelFlags.SizeIncludesHeader) != 0)
            ? size.Y : (lf & LabelFlags.AddPaddingToHeight) != 0
                ? size.Y.AddWinPadY() + labelStrokeHeight : size.Y + labelStrokeHeight;

        var outerSize = new Vector2(size.X, outerHeight);
        ImGui.SetCursorScreenPos(pos + new Vector2(0, labelStrokeHeight));
        bool success = ImGui.BeginChild($"##CL_CustomFullBody_{id}", outerSize, false, WFlags.AlwaysUseWindowPadding);
        ImGui.Dummy(labelSize + new Vector2(stroke - ImGui.GetStyle().ItemSpacing.Y));
        var innerSize = outerSize.WithoutWinPadding() - new Vector2(0, labelStrokeHeight);

        // Return the end object, closing the draw on the child.
        return new EndObjectLabelContainer(() =>
        {
            ImGui.EndChild();
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            wdl.AddRectFilled(min, max, CkCol.LChildBg.Uint(), rounding, df & ~DFlags.RoundCornersTop);
            // we are only drawing the labels now, so adjust our flags for them.
            var labelFlags = df & ~DFlags.RoundCornersBottom;
            // make sure that if the dFlags include DFlags.RoundCornersTopLeft, to apply that flag.
            DFlags labelDFlags = DFlags.RoundCornersBottomRight | ((df & DFlags.RoundCornersTopLeft) != 0 ? DFlags.RoundCornersTopLeft : DFlags.None);
            wdl.AddRectFilled(pos, labelMax + new Vector2(stroke), CkCol.LChildSplit.Uint(), rounding, labelDFlags);
            wdl.AddRectFilled(pos, labelMax, CkCol.LChild.Uint(), rounding, labelDFlags);
            ImGui.EndGroup();
        },
            success,
            outerSize.WithoutWinPadding(),
            innerSize
        );
    }

    /// <inheritdoc cref="ChildLabelCustomFull(string, Vector2, float, float, Action, DFlags, LabelFlags)"/>
    public static IEOLabelContainer ChildLabelCustomFull(string id, Vector2 size, Action label, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
        => ChildLabelCustomFull(id, size, CkStyle.ChildRounding(), CkStyle.FrameThickness(), label, df, lf);

    /// <inheritdoc cref="ChildLabelCustomFull(string, Vector2, float, float, Action, DFlags, LabelFlags)"/>
    public static IEOLabelContainer ChildLabelCustomFull(string id, Vector2 size, float rounding, Action label, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
        => ChildLabelCustomFull(id, size, rounding, CkStyle.FrameThickness(), label, df, lf);

    /// <summary> 
    ///     Custom drawn label, that is attached to a child. <para/>
    ///     If you intend to make this scrollable, make another child inside.
    /// </summary>
    /// <remarks> Note that the dummy covering the header is part of the child. </remarks>
    public static IEOLabelContainer ChildLabelCustomFull(string id, Vector2 size, float rounding, float stroke, Action label, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
    {
        // Begin the group that spans the outer area.
        var wdl = ImGui.GetWindowDrawList();
        var pos = ImGui.GetCursorScreenPos();
        ImGui.BeginGroup();

        // Handle drawing the label.
        ImGui.BeginGroup();
        label?.Invoke();
        ImGui.EndGroup();
        var labelMax = ImGui.GetItemRectMax();
        var labelSize = labelMax - pos;
        var labelStrokeHeight = labelSize.Y + stroke;
        // If size includes header, return size, otherwise, if we should add padding, add padding, but regardless, add header height.
        var outerHeight = ((lf & LabelFlags.SizeIncludesHeader) != 0)
            ? size.Y : (lf & LabelFlags.AddPaddingToHeight) != 0
                ? size.Y.AddWinPadY() + labelStrokeHeight : size.Y + labelStrokeHeight;

        var outerSize = new Vector2(size.X, outerHeight);
        ImGui.SetCursorScreenPos(pos + new Vector2(0, labelStrokeHeight));
        var innerSize = outerSize - new Vector2(0, labelStrokeHeight);
        bool success = ImGui.BeginChild($"##CL_CustomFullBody_{id}", innerSize, false, WFlags.AlwaysUseWindowPadding);

        // Return the end object, closing the draw on the child.
        return new EndObjectLabelContainer(() =>
        {
            ImGui.EndChild();
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            wdl.AddRectFilled(min, max, CkCol.LChildBg.Uint(), rounding, df & ~DFlags.RoundCornersTop);
            // we are only drawing the labels now, so adjust our flags for them.
            var labelFlags = df & ~DFlags.RoundCornersBottom;
            // Full-width: draw label + underline with thickness = fade
            wdl.AddRectFilled(pos, labelMax, CkCol.LChild.Uint(), rounding, labelFlags);
            var underlineMin = new Vector2(min.X, labelMax.Y);
            var underlineMax = new Vector2(max.X, labelMax.Y + stroke);
            wdl.AddRectFilled(underlineMin, underlineMax, CkCol.LChildSplit.Uint());
            ImGui.EndGroup();
        },
            success,
            outerSize.WithoutWinPadding(),
            innerSize
        );
    }



    public static IEOContainer LabelHeaderChild(Vector2 region, string label, float width, DFlags cFlags = DFlags.None, DFlags lFlags = DFlags.None)
        => LabelHeaderChild(region, label, width, ImUtf8.FrameHeight,  CkStyle.ChildRounding(), cFlags, lFlags);

    public static IEOContainer LabelHeaderChild(Vector2 region, string label, float width, float offset, DFlags cFlags = DFlags.None, DFlags lFlags = DFlags.None)
        => LabelHeaderChild(region, label, width, offset, CkStyle.ChildRounding(), cFlags, lFlags);

    public static IEOContainer LabelHeaderChild(Vector2 region, string label, float width, float offset, float rounding, DFlags cFlags = DFlags.None, DFlags lFlags = DFlags.None)
        => LabelHeaderChild(region, label, width, offset, rounding, cFlags, lFlags);

    /// <summary>
    ///     Creates a child object (no padding) with a nice colored background and label. <para />
    ///     Label has no hitbox, you can draw overtop it. (Not for cases that desire 0 padding)
    /// </summary>
    public static IEOContainer LabelHeaderChild(Vector2 region, string label, float width, float offset, float rounding, float thickness, DFlags childFlags = DFlags.None, DFlags labelFlags = DFlags.RoundCornersBottomRight)
    {
        var labelH = ImGui.GetTextLineHeightWithSpacing();
        var textSize = ImGui.CalcTextSize(label);
        // Get inner height below header.
        var innerHeight = Math.Min(region.Y, ImGui.GetContentRegionAvail().Y - labelH);
        // Get full childHeight.
        // The pos to know absolute min.
        var pos = ImGui.GetCursorScreenPos();

        // Outer group.
        ImGui.BeginGroup();

        ImGui.SetCursorScreenPos(pos + new Vector2(0, labelH));
        // Draw out the child.
        return new EndObjectContainer(() =>
            {
                ImGui.EndChild();
                ImGui.EndGroup();
                var max = ImGui.GetItemRectMax();
                var wdl = ImGui.GetWindowDrawList();

                // Draw out the child BG.
                wdl.AddRectFilled(pos, max, CkCol.LChildBg.Uint(), rounding, childFlags);

                // Now draw out the label header.
                var labelRectSize = new Vector2(width, labelH);
                wdl.AddRectFilled(pos, pos + labelRectSize + new Vector2(thickness), CkCol.LChildSplit.Uint(), rounding, labelFlags);
                wdl.AddRectFilled(pos, pos + labelRectSize, CkCol.LChild.Uint(), rounding, labelFlags);

                // add the text, centered to the height of the header, left aligned.
                var textStart = new Vector2(offset, (labelH - textSize.Y) / 2);
                wdl.AddText(pos + textStart, ImGui.GetColorU32(ImGuiCol.Text), label);
            }, 
            ImGui.BeginChild(label, region, false, WFlags.AlwaysUseWindowPadding),
            region.WithoutWinPadding()
        );
    }

}
