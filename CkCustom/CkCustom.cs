using CkCommons.Gui;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;

namespace CkCommons.Custom;

public static class CkCustom
{ 
    private static readonly Dictionary<string, float> _toggleAnimT = [];
    public static float TogglePillWidth => ImGui.GetFrameHeight() * 2f;

    // Needs some cleanup likely
    public static bool ButtonPillTag(string label, Vector4 color, float rounding = 90f)
    {
        var window = ImGuiP.GetCurrentWindow();
        if (window.SkipItems)
            return false;

        var id = ImGui.GetID(label);
        var style   = ImGui.GetStyle();
        var txtSize = ImGui.CalcTextSize(label);

        var width = txtSize.X + style.FramePadding.X * 4f;

        var size = new Vector2(width, ImGui.GetFrameHeight());
        var bb   = new ImRect(window.DC.CursorPos, window.DC.CursorPos + size);

        ImGuiP.ItemSize(bb, style.FramePadding.Y);
        if (!ImGuiP.ItemAdd(bb, id, null))
            return false;

        // Custom.
        var hovered = false;
        var active = false;
        var clicked = ImGuiP.ButtonBehavior(bb, id, ref hovered, ref active);

        var bgAlpha = active ? 0.45f : hovered ? 0.35f : 0.25f;
        var borderCol = hovered ? ColorHelpers.WithAlpha(color, 1.0f) : ColorHelpers.WithAlpha(color, 0.75f);

        ImGuiP.RenderNavHighlight(bb, id);
        ImGuiP.RenderFrame(bb.Min, bb.Max, CkStyle.GetFrameBg(hovered, active), false, rounding);
        // Render border
        window.DrawList.AddRect(bb.Min, bb.Max, borderCol.ToUint(), rounding, ImDrawFlags.RoundCornersAll, 1.25f * ImGuiHelpers.GlobalScale);

        var labelPos = new Vector2(bb.Min.X + style.FramePadding.X * 2, bb.Min.Y + style.FramePadding.Y);
        window.DrawList.AddText(labelPos, ImGui.GetColorU32(ImGuiCol.Text), label);
        return clicked;
    }

    public static void LabelPillTag(string text, Vector4 color, float rounding = 0f)
        => StatusPillTag(text, color.ToUint(), rounding, false);

    public static void LabelPillTag(string text, uint color, float rounding = 0f)
        => StatusPillTag(text, color, rounding, false);

    public static void StatusPillTag(string text, Vector4 color, float rounding = 90f, bool showDot = true, float pulsePeriod = 5f)
        => StatusPillTag(text, color.ToUint(), rounding, showDot, pulsePeriod);

    public static void StatusPillTag(string text, uint color, float rounding = 90f, bool showDot = true, float pulsePeriod = 5f)
    {
        var scale   = ImGuiHelpers.GlobalScale;
        var style   = ImGui.GetStyle();
        var padding = style.FramePadding;
        var h       = ImGui.GetFrameHeight();
        var txtSize = ImGui.CalcTextSize(text);

        var diameter = h * 0.25f;
        var radius   = diameter * 0.5f;
        var width = txtSize.X + padding.X * 2 + (showDot ? diameter + padding.X + style.ItemInnerSpacing.X + scale : 0f);

        // DrawnArea
        var window = ImGuiP.GetCurrentWindow();
        if (window.SkipItems)
            return;
        ImGui.Dummy(new Vector2(width, h));
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        var centerY = (min.Y + max.Y) * 0.5f;

        // Frame
        window.DrawList.AddRectFilled(min, max, CkGui.ApplyAlpha(color, 0.25f), rounding);
        window.DrawList.AddRect(min, max, color, rounding, ImDrawFlags.RoundCornersAll, 1.25f * scale);

        var cursorX = min.X + radius + padding.X;
        // Dot Display
        if (showDot)
        {
            var time = (float)ImGui.GetTime();
            var pulse = 0.5f + 0.5f * MathF.Sin(time * (MathF.Tau / pulsePeriod));
            var alpha = 0.5f + 0.5f * pulse;
            var scalePulse = 1f + 0.4f * pulse;
            var center = new Vector2(cursorX + radius, centerY);
            window.DrawList.AddCircleFilled(center, radius * scalePulse, CkGui.ApplyAlpha(color, alpha), 16);
            window.DrawList.AddCircleFilled(center, (radius + 1.5f * scale) * scalePulse, CkGui.ApplyAlpha(color, 0.25f * alpha), 18);
            cursorX += diameter + style.ItemInnerSpacing.X + scale;
        }
        // Text Display
        var textPos = new Vector2(cursorX, centerY - txtSize.Y * 0.5f);
        window.DrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);
    }

    public static bool TogglePillRightAligned(string id, ref bool value, bool disabled = false)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - TogglePillWidth);
        return ToggleButton(id, ref value, disabled);
    }

    // Can enhance performance with this later via mimicing our custom button behavior
    public static bool ToggleButton(string id, ref bool value, bool disabled = false)
    {
        var pos = ImGui.GetCursorScreenPos();
        var drawList = ImGui.GetWindowDrawList();

        var height = ImGui.GetFrameHeight();
        var width = height * 2f;
        var radius = height * 0.5f;

        var knobInset = 2f;
        var knobS = height - knobInset * 2f;

        var dl = ImGui.GetWindowDrawList();
        var min = ImGui.GetCursorScreenPos();

        var clicked = ImGui.InvisibleButton($"##toggle_{id}", new Vector2(width, height));
        var hovered = ImGui.IsItemHovered();
        var max = ImGui.GetItemRectMax();
        var changed = false;
        if (clicked && !disabled)
        {
            value = !value;
            _toggleAnimT[id] = 0f; // reset animation
            changed = true;
        }

        // Advance animation if present
        if (_toggleAnimT.TryGetValue(id, out float t))
        {
            t = MathF.Min(1f, t + ImGui.GetIO().DeltaTime / 0.22f);
            if (t >= 1f)
                _toggleAnimT.Remove(id);
            else
                _toggleAnimT[id] = t;
        }
        else
        {
            t = 1f; // no animation, fully settled
        }

        // Ease (simple cubic out, no overshoot needed here)
        var eased = 1f - MathF.Pow(1f - t, 3f);

        // Background
        var bgCol = value ? CkCol.GoldBase.Vec4().WithAlpha(0.12f) : ImGuiColors.ParsedGrey;
        var borderCol = value ? CkCol.GoldDeep.Vec4() : CkCol.Silver.Vec4();
        var knobCol = value ? CkCol.GoldDeep.Vec4() : ImGui.GetColorU32(ImGuiCol.TextDisabled).ToVec4();
        if (hovered && !disabled) borderCol = value ? CkCol.Gold.Vec4() : CkCol.GoldDeep.Vec4();

        // Affect disabled coloring
        if (disabled)
        {
            bgCol = ColorHelpers.WithAlpha(bgCol, bgCol.W * 0.5f);
            borderCol = ColorHelpers.WithAlpha(borderCol, borderCol.W * 0.5f);
            knobCol = ColorHelpers.WithAlpha(knobCol, knobCol.W * .5f);
        }

        dl.AddRectFilled(min, max, bgCol.ToUint());
        dl.AddRect(min, max, borderCol.ToUint(), 0f, ImDrawFlags.None, 1f);

        // Knob
        var knobLeftX = min.X + knobInset;
        var knobRightX = max.X - knobS - knobInset;

        var startX = value ? knobLeftX : knobRightX;
        var endX = value ? knobRightX : knobLeftX;
        var knobX = startX + (endX - startX) * eased;
        var knobY = min.Y + knobInset;

        var knobMin = new Vector2(knobX, knobY);
        var knobMax = knobMin + new Vector2(knobS, knobS);
        if (value)
        {
            // Halo: 3 concentric squares
            for (int g = 3; g >= 1; g--)
            {
                var pad = g * 1.6f * ImGuiHelpers.GlobalScale;
                var glowCol = CkCol.GoldBase.Vec4().WithAlpha(0.20f / g).ToUint();
                dl.AddRectFilled(knobMin - new Vector2(pad, pad), knobMax + new Vector2(pad, pad), glowCol);
            }
        }
        dl.AddRectFilled(knobMin, knobMax, knobCol.ToUint());

        if (value)
        {
            var inset = 4f * ImGuiHelpers.GlobalScale;
            dl.AddRect(knobMin + new Vector2(inset, inset),
                       knobMax - new Vector2(inset, inset),
                       CkCol.GoldDeep.Uint(), 0f, ImDrawFlags.None, 1f);
        }
        else
        {
            dl.AddRect(knobMin, knobMax, new Vector4(0.245f, 0.257f, 0.304f, 1.0f).ToUint(), 0f, ImDrawFlags.None, 1f);
        }

        return changed;
    }
}