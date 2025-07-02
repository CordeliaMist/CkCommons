using CkCommons.Gui;
using CkCommons.Services;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using ImGuiNET;
using static System.Net.Mime.MediaTypeNames;

namespace CkCommons.Raii;
public static partial class CkRaii
{
    public static IEOLabelContainer LabelChildText(Vector2 size, float widthSpan, string text, DFlags dFlag = DFlags.None)
        => LabelChildText(size, widthSpan, text, ImGui.GetStyle().WindowPadding.X, CkStyle.FrameThickness(), ColorsLC.Default, dFlag);

    public static IEOLabelContainer LabelChildText(Vector2 size, float widthSpan, string text, float rounding, DFlags dFlag = DFlags.None)
        => LabelChildText(size, widthSpan, text, rounding, CkStyle.FrameThickness(), ColorsLC.Default, dFlag);

    public static IEOLabelContainer LabelChildText(Vector2 size, float widthSpan, string text, float rounding, ColorsLC col, DFlags dFlag = DFlags.None)
        => LabelChildText(size, widthSpan, text, rounding, CkStyle.FrameThickness(), col, dFlag, WFlags.None);

    public static IEOLabelContainer LabelChildText(Vector2 size, float widthSpan, string text, float rounding, float fade, DFlags dFlag = DFlags.None)
        => LabelChildText(size, widthSpan, text, rounding, fade, ColorsLC.Default, dFlag, WFlags.None);

    /// <summary> Constructs a Label Child object with a text based header. </summary>
    /// <param name="size"> The size of the child object. </param>
    /// <param name="widthSpan"> The ammount of <paramref name="size"/>'s width should contain the header. From 0.1-1 </param>
    /// <param name="text"> The text to display in the header. </param>
    /// <param name="col"> The colors to use for the header. </param>
    /// <param name="rounding"> The rounding to use for the header. </param>
    /// <param name="fade"> How thick the outline around the header is. </param>
    /// <param name="dFlag"> Determines what corners are rounded on the child. </param>
    /// <param name="wFlags"> Any additional flags for the label child object. </param>
    /// <remarks> The IEOLabelContainer contains the size of the label region and the inner region. </remarks>
    public static IEOLabelContainer LabelChildText(Vector2 size, float widthSpan, string text, float rounding, float fade, ColorsLC col, DFlags dFlag, WFlags wFlags = WFlags.None)
    {
        float labelWidth = size.X * Math.Clamp(widthSpan, 0f, 1f);
        float offset = (dFlag & DFlags.RoundCornersTopLeft) != 0 ? rounding : ImGui.GetStyle().WindowPadding.X;

        bool fullWidth = widthSpan >= 1f;
        Vector2 hSize = new(labelWidth, ImGui.GetFrameHeight());
        Vector2 dummySize = hSize;
        if (!fullWidth) dummySize += new Vector2(0, fade);

        // Begin the child object.
        bool success = ImGui.BeginChild($"##LabelChild-{text}", new Vector2(size.X, size.Y + ImGui.GetFrameHeightWithSpacing()), false, wFlags | WFlags.AlwaysUseWindowPadding);
        ImGui.Dummy(dummySize);

        // Return the end object, closing the draw on the child.
        return new EndObjectLabelContainer(() =>
            {
                ImGui.EndChild();
                Vector2 min = ImGui.GetItemRectMin();
                Vector2 max = ImGui.GetItemRectMax();
                var wdl = ImGui.GetWindowDrawList();

                // Draw out the child BG.
                wdl.AddRectFilled(min, max, col.BG, rounding, dFlag);

                // Determine where the label ends.
                Vector2 labelMin = min;
                Vector2 labelMax = fullWidth ? new Vector2(max.X, min.Y + ImGui.GetFrameHeight()) : min + hSize;

                if (!fullWidth)
                {
                    // Partial-width: draw shadow + label
                    DFlags labelDFlags =  DFlags.RoundCornersBottomRight | ((dFlag & DFlags.RoundCornersTopLeft) != 0 ? DFlags.RoundCornersTopLeft : DFlags.None);
                    Vector2 labelShadow = hSize + new Vector2(fade);
                    wdl.AddRectFilled(min, labelMax + new Vector2(fade), col.Shadow, rounding, labelDFlags);
                    wdl.AddRectFilled(labelMin, labelMax, col.Label, rounding, labelDFlags);
                }
                else
                {
                    // we are only drawing the labels now, so adjust our flags for them.
                    var labelFlags = dFlag & ~DFlags.RoundCornersBottom;
                    // Full-width: draw label + underline with thickness = fade
                    wdl.AddRectFilled(labelMin, labelMax, col.Label, rounding, labelFlags);
                    var underlineMin = new Vector2(min.X, labelMax.Y);
                    var underlineMax = new Vector2(max.X, labelMax.Y + fade);
                    wdl.AddRectFilled(underlineMin, underlineMax, col.Shadow);
                }

                // add the text, centered to the height of the header, left aligned.
                Vector2 textStart = new Vector2(offset, (hSize.Y - ImGui.GetTextLineHeight()) / 2);
                wdl.AddText(min + textStart, ImGui.GetColorU32(ImGuiCol.Text), text);
            },
            success,
            size.WithoutWinPadding(),
            hSize
        );
    }

    public static IEOLabelContainer LabelChildAction(string id, Vector2 size, float widthSpan, Func<bool> label, Action<ImGuiMouseButton>? clicked, string tt, DFlags dFlag = DFlags.None)
        => LabelChildAction(id, size, widthSpan, label,  CkStyle.ChildRounding(), clicked, tt, dFlag);

    public static IEOLabelContainer LabelChildAction(string id, Vector2 size, float widthSpan, Func<bool> label, ColorsLC col, Action<ImGuiMouseButton>? clicked, string tt, DFlags dFlag = DFlags.None)
        => LabelChildAction(id, size, widthSpan, label,  CkStyle.ChildRounding(), col, clicked, tt, dFlag);

    public static IEOLabelContainer LabelChildAction(string id, Vector2 size, float widthSpan, Func<bool> label, float rounding, Action<ImGuiMouseButton>? clicked, string tt, DFlags dFlag = DFlags.None)
        => LabelChildAction(id, size, widthSpan, label, rounding, CkStyle.FrameThickness(), ColorsLC.Default, clicked, tt, dFlag);

    public static IEOLabelContainer LabelChildAction(string id, Vector2 size, float widthSpan, Func<bool> label, float rounding, float fade, Action<ImGuiMouseButton>? clicked, string tt, DFlags dFlag = DFlags.None)
        => LabelChildAction(id, size, widthSpan, label, rounding, fade, ColorsLC.Default, clicked, tt, dFlag);

    public static IEOLabelContainer LabelChildAction(string id, Vector2 size, float widthSpan, Func<bool> label, float rounding, ColorsLC col, Action<ImGuiMouseButton>? clicked, string tt, DFlags dFlag = DFlags.None)
        => LabelChildAction(id, size, widthSpan, label, rounding, CkStyle.FrameThickness(), col, clicked, tt, dFlag);

    /// <summary> 
    ///     Interactable label header within a padded child. <para/>
    ///     Please note that your label draws are not intended to be taller than ImGui.GetFrameHeight()
    /// </summary>
    /// <remarks> Note that the dummy covering the header is part of the child. If you intend to make this scrollable, make another child inside. </remarks>
    public static IEOLabelContainer LabelChildAction(string id, Vector2 size, float widthSpan, Func<bool> drawLabel, float rounding, float fade, ColorsLC col, Action<ImGuiMouseButton>? clicked, string tt, DFlags dFlag = DFlags.None)
    {
        string tooltip = tt.IsNullOrWhitespace() ? "Double Click to Interact--SEP--Right-Click to Cancel" : tt;
        ImDrawListPtr wdl = ImGui.GetWindowDrawList();
        Vector2 pos = ImGui.GetCursorScreenPos();
        ImGui.BeginGroup();
        // Begin the child object.
        bool success = ImGui.BeginChild($"##LabelChildActionOuter-{id}", size);

        // Handle drawing the label.
        float labelHeight = ImGui.GetFrameHeight();
        float labelWidth = size.X * Math.Clamp(widthSpan, 0f, 1f);
        bool fullWidth = widthSpan >= 1f;
        Vector2 labelSize = new(labelWidth, labelHeight);
        Vector2 clipMin = ImGui.GetCursorScreenPos();
        Vector2 clipMax = clipMin + labelSize;

        ImGui.PushClipRect(clipMin, clipMax, true);
        ImGui.BeginGroup();
        ImGui.Dummy(labelSize);
        ImGui.SetCursorScreenPos(clipMin);
        bool disabled = drawLabel.Invoke();
        ImGui.EndGroup();
        ImGui.PopClipRect();

        var labelMin = ImGui.GetItemRectMin();
        var labelMax = ImGui.GetItemRectMax();
        var hovered = ImGui.IsMouseHoveringRect(labelMin, labelMax);
        CkGui.AttachToolTipRect(labelMin, labelMax, tooltip);
        if (!disabled && hovered)
        {
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)) clicked?.Invoke(ImGuiMouseButton.Left);
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right)) clicked?.Invoke(ImGuiMouseButton.Right);
        }

        // Draw the padded Child (The inner contents we actually draw in).
        ImGui.SetCursorScreenPos(pos);
        success &= ImGui.BeginChild($"##LabelChildAction-{id}", size, false, WFlags.AlwaysUseWindowPadding);
        
        // Draw the dummy inside the child, so it spans the label size, ensuring we cannot draw in that space.
        Vector2 labelThickness = (labelMax - labelMin) + new Vector2(fade);
        ImGui.Dummy(labelThickness - ImGui.GetStyle().ItemSpacing - ImGui.GetStyle().WindowPadding / 2);

        // Return the end object, closing the draw on the child.
        // Return the end object, closing the draw on the child.
        return new EndObjectLabelContainer(() =>
        {
            ImGui.EndChild();
            wdl.AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), col.BG, rounding, dFlag);
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();


            if (!fullWidth)
            {
                // make sure that if the dFlags include DFlags.RoundCornersTopLeft, to apply that flag.
                DFlags labelDFlags = DFlags.RoundCornersBottomRight | ((dFlag & DFlags.RoundCornersTopLeft) != 0 ? DFlags.RoundCornersTopLeft : DFlags.None);
                wdl.AddRectFilled(labelMin, labelMin + labelThickness, col.Shadow, rounding, labelDFlags);
                uint labelCol = disabled ? col.Label : hovered ? col.LabelHovered : col.Label;
                wdl.AddRectFilled(labelMin, labelMax, labelCol, rounding, labelDFlags);
            }
            else
            {
                // we are only drawing the labels now, so adjust our flags for them.
                var labelFlags = dFlag & ~DFlags.RoundCornersBottom;
                // Full-width: draw label + underline with thickness = fade
                wdl.AddRectFilled(labelMin, labelMax, col.Label, rounding, labelFlags);
                var underlineMin = new Vector2(min.X, labelMax.Y);
                var underlineMax = new Vector2(max.X, labelMax.Y + fade);
                wdl.AddRectFilled(underlineMin, underlineMax, col.Shadow);
            }

            // end outer child.
            ImGui.EndChild();
            ImGui.EndGroup();
        },
            success,
            size.WithoutWinPadding(),
            (labelMax - labelMin)
        );
    }

    // LOOK into further later, it will help cleanup a lot of what is in this section. (for now just deal with paramater hell)
    // Pushes overtop the previous child's MinRect and draws an interactable label.
    //public static IEOLabelContainer LabelOnPrevChild(string id, Vector2 size, float rounding, float fade, DFlags flags)
    //{
    //    ImGui.PushClipRect(clipMin, clipMax, true);
    //    ImGui.BeginGroup();
    //    ImGui.Dummy(labelSize);
    //    ImGui.SetCursorScreenPos(clipMin);
    //    bool disabled = drawLabel.Invoke();
    //    ImGui.EndGroup();
    //    ImGui.PopClipRect();
    //}

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
        float labelH = ImGui.GetTextLineHeightWithSpacing();
        Vector2 textSize = ImGui.CalcTextSize(label);
        // Get inner height below header.
        float innerHeight = Math.Min(size.Y, ImGui.GetContentRegionAvail().Y - labelH);
        // Get full childHeight.
        // The pos to know absolute min.
        Vector2 pos = ImGui.GetCursorScreenPos();

        // Outer group.
        ImGui.BeginGroup();

        ImGui.SetCursorScreenPos(pos + new Vector2(0, labelH));
        // Draw out the child.
        return new EndObjectContainer(() =>
            {
                ImGui.EndChild();
                ImGui.EndGroup();
                Vector2 max = ImGui.GetItemRectMax();
                ImDrawListPtr wdl = ImGui.GetWindowDrawList();

                // Draw out the child BG.
                wdl.AddRectFilled(pos, max, colors.BodyColor, rounding, childFlags);

                // Now draw out the label header.
                Vector2 labelRectSize = new Vector2(labelWidth, labelH);
                wdl.AddRectFilled(pos, pos + labelRectSize + new Vector2(thickness), colors.SplitColor, rounding, labelFlags);
                wdl.AddRectFilled(pos, pos + labelRectSize, colors.HeaderColor, rounding, labelFlags);

                // add the text, centered to the height of the header, left aligned.
                Vector2 textStart = new Vector2(labelOffset, (labelH - textSize.Y) / 2);
                wdl.AddText(pos + textStart, ImGui.GetColorU32(ImGuiCol.Text), label);
            }, 
            ImGui.BeginChild(label, size, false, WFlags.AlwaysUseWindowPadding),
            size.WithoutWinPadding()
        );
    }

}
