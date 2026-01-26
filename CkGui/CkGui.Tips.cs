using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using OtterGui.Text;

namespace CkCommons.Gui;

// Partial Class for Text Display Helpers.
public static partial class CkGui
{
    public const string TipSep = "--SEP--";
    public const string TipNL = "--NL--";
    public const string TipCol = "--COL--";
  
    /// <summary> A helper function to attach a tooltip to a section in the UI currently hovered. </summary>
    /// <remarks> If the string is null, empty, or whitespace, will do early return at no performance impact. </remarks>
    public static void AttachToolTip(string? text, Vector4? color = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        // if the item is currently hovered, with the ImGuiHoveredFlags set to allow when disabled
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ToolTipInternal(text, color);
    }

    /// <summary> A helper function to attach a tooltip to a section in the UI currently hovered. </summary>
    /// <remarks> If the string is null, empty, or whitespace, will do early return at no performance impact. </remarks>
    public static void AttachToolTip(string? text, bool disabled, Vector4? color = null)
    {
        if (disabled || string.IsNullOrWhiteSpace(text))
            return;

        // if the item is currently hovered, with the ImGuiHoveredFlags set to allow when disabled
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.RectOnly | ImGuiHoveredFlags.AllowWhenDisabled))
            ToolTipInternal(text, color);
    }

    /// <summary> A helper function to attach a tooltip to a section in the UI currently hovered. </summary>
    /// <remarks> If the string is null, empty, or whitespace, will do early return at no performance impact. </remarks>
    public static void AttachToolTipRect(Vector2 min, Vector2 max, string? text, Vector4? color = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        // if the item is currently hovered, with the ImGuiHoveredFlags set to allow when disabled
        if (ImGui.IsMouseHoveringRect(min, max))
            ToolTipInternal(text, color);
    }

    public static void ToolTipInternal(string text, Vector4? color = null)
    {
        using var s = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.One * 6f)
            .Push(ImGuiStyleVar.WindowRounding, 4f)
            .Push(ImGuiStyleVar.PopupBorderSize, 1f);
        using var c = ImRaii.PushColor(ImGuiCol.Border, ImGuiColors.ParsedPink);

        ImGui.BeginTooltip();
        TextWrappedTooltipFormat(text, ImGui.GetFontSize() * 35f, color);
        ImGui.EndTooltip();
    }

    public static void HelpText(string helpText, bool inner = false, uint? offColor = null)
    {
        if (inner)
            ImUtf8.SameLineInner();
        else
            ImGui.SameLine();

        bool hovering = ImGui.IsMouseHoveringRect(ImGui.GetCursorScreenPos(), ImGui.GetCursorScreenPos() + new Vector2(ImGui.GetTextLineHeight()));
        FramedIconText(FAI.QuestionCircle, hovering ? ImGui.GetColorU32(ImGuiColors.TankBlue) : offColor ?? ImGui.GetColorU32(ImGuiCol.TextDisabled));
        AttachToolTip(helpText);
    }

    public static void HelpText(string helpText, Vector4 tooltipCol, bool inner = false, uint? offColor = null)
    {
        if (inner)
            ImUtf8.SameLineInner();
        else
            ImGui.SameLine();

        bool hovering = ImGui.IsMouseHoveringRect(ImGui.GetCursorScreenPos(), ImGui.GetCursorScreenPos() + new Vector2(ImGui.GetTextLineHeight()));
        FramedIconText(FAI.QuestionCircle, hovering ? ImGui.GetColorU32(ImGuiColors.TankBlue) : offColor ?? ImGui.GetColorU32(ImGuiCol.TextDisabled));
        AttachToolTip(helpText, color: tooltipCol);
    }

    public static void HelpText(string helpText, uint tooltipCol, bool inner = false, uint? offColor = null)
    {
        if (inner)
            ImUtf8.SameLineInner();
        else
            ImGui.SameLine();

        bool hovering = ImGui.IsMouseHoveringRect(ImGui.GetCursorScreenPos(), ImGui.GetCursorScreenPos() + new Vector2(ImGui.GetTextLineHeight()));
        FramedIconText(FAI.QuestionCircle, hovering ? ImGui.GetColorU32(ImGuiColors.TankBlue) : offColor ?? ImGui.GetColorU32(ImGuiCol.TextDisabled));
        AttachToolTip(helpText, color: ColorHelpers.RgbaUintToVector4(tooltipCol));
    }


    [GeneratedRegex($"({TipSep}|{TipNL}|{TipCol})", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    public static partial Regex TooltipTokenRegex();
}
