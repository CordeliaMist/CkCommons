using CkCommons.Gui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using ImGuiNET;

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
        var labelWidth = size.X * Math.Clamp(widthSpan, 0f, 1f);
        var offset = (df & DFlags.RoundCornersTopLeft) != 0 ? rounding : ImGui.GetStyle().WindowPadding.X;

        // Calculate the drawn header size.
        Vector2 hSize = new(labelWidth, ImGui.GetFrameHeight());
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
                wdl.AddRectFilled(min, ImGui.GetItemRectMax(), ColorsLC.Default.BG, rounding, df);
                // Partial-width: draw shadow + label
                var labelDFlags =  DFlags.RoundCornersBottomRight | ((df & DFlags.RoundCornersTopLeft) != 0 ? DFlags.RoundCornersTopLeft : DFlags.None);
                wdl.AddRectFilled(min, min + hSize + new Vector2(stroke), ColorsLC.Default.Shadow, rounding, labelDFlags);
                wdl.AddRectFilled(min, min + hSize, ColorsLC.Default.Label, rounding, labelDFlags);
                // add the text, centered to the height of the header, left aligned.
                wdl.AddText(min + new Vector2(offset, (hSize.Y - ImGui.GetTextLineHeight()) / 2), ImGui.GetColorU32(ImGuiCol.Text), text);
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

            wdl.AddRectFilled(min, ImGui.GetItemRectMax(), ColorsLC.Default.BG, rounding, df);
            // Determine where the label ends.
            var labelMax = new Vector2(max.X, min.Y + ImGui.GetFrameHeight());
            // we are only drawing the labels now, so adjust our flags for them.
            var labelFlags = df & ~DFlags.RoundCornersBottom;
            // Full-width: draw label + underline with thickness = fade
            wdl.AddRectFilled(min, labelMax, ColorsLC.Default.Label, rounding, labelFlags);
            var underlineMin = new Vector2(min.X, labelMax.Y);
            var underlineMax = new Vector2(max.X, labelMax.Y + stroke);
            wdl.AddRectFilled(underlineMin, underlineMax, ColorsLC.Default.Shadow);
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
            wdl.AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), ColorsLC.Default.BG, rounding, df);
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            // make sure that if the dFlags include DFlags.RoundCornersTopLeft, to apply that flag.
            var labelDFlags = DFlags.RoundCornersBottomRight | ((df & DFlags.RoundCornersTopLeft) != 0 ? DFlags.RoundCornersTopLeft : DFlags.None);
            wdl.AddRectFilled(labelMin, labelMax + new Vector2(stroke), ColorsLC.Default.Shadow, rounding, labelDFlags);
            var labelCol = hovered ? ColorsLC.Default.LabelHovered : ColorsLC.Default.Label;
            wdl.AddRectFilled(labelMin, labelMax, labelCol, rounding, labelDFlags);
            // add the text, centered to the height of the header, left aligned.
            wdl.AddText(min + new Vector2(offset, (labelSize.Y - ImGui.GetTextLineHeight()) / 2), ImGui.GetColorU32(ImGuiCol.Text), text);
        },
            success,
            outerSize.WithoutWinPadding(),
            innerSize
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
        
        var labelMin = ImGui.GetCursorScreenPos();
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
            wdl.AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), ColorsLC.Default.BG, rounding, df);
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            // we are only drawing the labels now, so adjust our flags for them.
            var labelFlags = df & ~DFlags.RoundCornersBottom;
            // Full-width: draw label + underline with thickness = fade
            wdl.AddRectFilled(labelMin, labelMax, ColorsLC.Default.Label, rounding, labelFlags);
            var underlineMin = new Vector2(min.X, labelMax.Y);
            var underlineMax = new Vector2(max.X, labelMax.Y + stroke);
            wdl.AddRectFilled(underlineMin, underlineMax, ColorsLC.Default.Shadow);
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
            wdl.AddRectFilled(min, max, ColorsLC.Default.BG, rounding, df);
            // we are only drawing the labels now, so adjust our flags for them.
            var labelFlags = df & ~DFlags.RoundCornersBottom;
            // make sure that if the dFlags include DFlags.RoundCornersTopLeft, to apply that flag.
            DFlags labelDFlags = DFlags.RoundCornersBottomRight | ((df & DFlags.RoundCornersTopLeft) != 0 ? DFlags.RoundCornersTopLeft : DFlags.None);
            wdl.AddRectFilled(pos, labelMax + new Vector2(stroke), ColorsLC.Default.Shadow, rounding, labelDFlags);
            uint labelCol = hovered ? ColorsLC.Default.LabelHovered : ColorsLC.Default.Label;
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
            wdl.AddRectFilled(min, max, ColorsLC.Default.BG, rounding, df);
            // we are only drawing the labels now, so adjust our flags for them.
            var labelFlags = df & ~DFlags.RoundCornersBottom;
            // Full-width: draw label + underline with thickness = fade
            wdl.AddRectFilled(pos, labelMax, ColorsLC.Default.Label, rounding, labelFlags);
            var underlineMin = new Vector2(min.X, labelMax.Y);
            var underlineMax = new Vector2(max.X, labelMax.Y + stroke);
            wdl.AddRectFilled(underlineMin, underlineMax, ColorsLC.Default.Shadow);
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
            wdl.AddRectFilled(min, max, ColorsLC.Default.BG, rounding, df & ~DFlags.RoundCornersTop);
            // we are only drawing the labels now, so adjust our flags for them.
            var labelFlags = df & ~DFlags.RoundCornersBottom;
            // make sure that if the dFlags include DFlags.RoundCornersTopLeft, to apply that flag.
            DFlags labelDFlags = DFlags.RoundCornersBottomRight | ((df & DFlags.RoundCornersTopLeft) != 0 ? DFlags.RoundCornersTopLeft : DFlags.None);
            wdl.AddRectFilled(pos, labelMax + new Vector2(stroke), ColorsLC.Default.Shadow, rounding, labelDFlags);
            wdl.AddRectFilled(pos, labelMax, ColorsLC.Default.Label, rounding, labelDFlags);
            ImGui.EndGroup();
        },
            success,
            outerSize.WithoutWinPadding(),
            innerSize
        );
    }

    /// <inheritdoc cref="ChildLabelCustomFull(string, Vector2, float, float, Action, ImDrawFlags, LabelFlags)"/>
    public static IEOLabelContainer ChildLabelCustomFull(string id, Vector2 size, Action label, DFlags df = DFlags.None, LabelFlags lf = LabelFlags.None)
        => ChildLabelCustomFull(id, size, CkStyle.ChildRounding(), CkStyle.FrameThickness(), label, df, lf);

    /// <inheritdoc cref="ChildLabelCustomFull(string, Vector2, float, float, Action, ImDrawFlags, LabelFlags)"/>
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
            wdl.AddRectFilled(min, max, ColorsLC.Default.BG, rounding, df & ~DFlags.RoundCornersTop);
            // we are only drawing the labels now, so adjust our flags for them.
            var labelFlags = df & ~DFlags.RoundCornersBottom;
            // Full-width: draw label + underline with thickness = fade
            wdl.AddRectFilled(pos, labelMax, ColorsLC.Default.Label, rounding, labelFlags);
            var underlineMin = new Vector2(min.X, labelMax.Y);
            var underlineMax = new Vector2(max.X, labelMax.Y + stroke);
            wdl.AddRectFilled(underlineMin, underlineMax, ColorsLC.Default.Shadow);
            ImGui.EndGroup();
        },
            success,
            outerSize.WithoutWinPadding(),
            innerSize
        );
    }



    public static IEOContainer LabelHeaderChild(Vector2 size, string label, float labelWidth, DFlags cFlags = DFlags.None, DFlags lFlags = DFlags.None)
        => LabelHeaderChild(size, label, labelWidth, ImGui.GetFrameHeight(),  CkStyle.ChildRounding(), cFlags, lFlags);

    public static IEOContainer LabelHeaderChild(Vector2 size, string label, float labelWidth, float labelOffset, DFlags cFlags = DFlags.None, DFlags lFlags = DFlags.None)
        => LabelHeaderChild(size, label, labelWidth, labelOffset,  CkStyle.ChildRounding(), cFlags, lFlags);

    public static IEOContainer LabelHeaderChild(Vector2 size, string label, float labelWidth, float labelOffset, float rounding, DFlags cFlags = DFlags.None, DFlags lFlags = DFlags.None)
        => LabelHeaderChild(size, label, labelWidth, labelOffset, rounding, HeaderChildColors.Default, cFlags, lFlags);

    public static IEOContainer LabelHeaderChild(Vector2 size, string label, float labelWidth, float labelOffset, float rounding, HeaderChildColors colors, DFlags cFlags = DFlags.None, DFlags lFlags = DFlags.None)
        => LabelHeaderChild(size, label, labelWidth, labelOffset, rounding, ImGui.GetStyle().WindowPadding.X/2, colors, cFlags, lFlags);

    public static IEOContainer LabelHeaderChild(Vector2 size, string label, float labelWidth, float labelOffset, float rounding, float thickness, DFlags cFlags, DFlags lFlags)
        => LabelHeaderChild(size, label, labelWidth, labelOffset, rounding, thickness, HeaderChildColors.Default, cFlags, lFlags);

    /// <summary> Creates a child object (no padding) with a nice colored background and label. </summary>
    /// <remarks> The label will not have a hitbox, and you will be able to draw overtop it. This is dont for cases that desire no padding. </remarks>
    public static IEOContainer LabelHeaderChild(Vector2 size, string label, float labelWidth, float labelOffset, float rounding, float thickness,
        HeaderChildColors colors, DFlags childFlags = DFlags.None, DFlags labelFlags = DFlags.RoundCornersBottomRight)
    {
        var labelH = ImGui.GetTextLineHeightWithSpacing();
        var textSize = ImGui.CalcTextSize(label);
        // Get inner height below header.
        var innerHeight = Math.Min(size.Y, ImGui.GetContentRegionAvail().Y - labelH);
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
                wdl.AddRectFilled(pos, max, colors.BodyColor, rounding, childFlags);

                // Now draw out the label header.
                var labelRectSize = new Vector2(labelWidth, labelH);
                wdl.AddRectFilled(pos, pos + labelRectSize + new Vector2(thickness), colors.SplitColor, rounding, labelFlags);
                wdl.AddRectFilled(pos, pos + labelRectSize, colors.HeaderColor, rounding, labelFlags);

                // add the text, centered to the height of the header, left aligned.
                var textStart = new Vector2(labelOffset, (labelH - textSize.Y) / 2);
                wdl.AddText(pos + textStart, ImGui.GetColorU32(ImGuiCol.Text), label);
            }, 
            ImGui.BeginChild(label, size, false, WFlags.AlwaysUseWindowPadding),
            size.WithoutWinPadding()
        );
    }

}
