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
    public const HFlags TipHoverFlags = HFlags.RectOnly | HFlags.AllowWhenDisabled;

    private static bool ShowTooltip(string? text, HFlags hoverFlags)
        => ImGui.IsItemHovered(hoverFlags) && !string.IsNullOrWhiteSpace(text);

    /// <summary> A helper function to attach a tooltip to a section in the UI currently hovered. </summary>
    /// <remarks> If the string is null, empty, or whitespace, will do early return at no performance impact. </remarks>
    public static void AttachTooltip(string? text, HFlags hoverFlags = TipHoverFlags)
    {
        if (ShowTooltip(text, hoverFlags))
            ToolTipInternal(text!);
    }

    /// <inheritdoc cref="AttachTooltip(string?, HFlags)"/>"
    public static void AttachTooltip(string? text, uint color, HFlags hoverFlags = TipHoverFlags)
    {
        if (ShowTooltip(text, hoverFlags))
            ToolTipInternal(text!, colorUint: color);
    }

    /// <inheritdoc cref="AttachTooltip(string?, HFlags)"/>"
    public static void AttachTooltip(string? text, Vector4 color, HFlags hoverFlags = TipHoverFlags)
    {
        if (ShowTooltip(text, hoverFlags))
            ToolTipInternal(text!, color);
    }

    /// <inheritdoc cref="AttachTooltip(string?, HFlags)"/>"
    public static void AttachTooltip(string? text, bool disabled, HFlags hoverFlags = TipHoverFlags)
    {
        if (!disabled && !ShowTooltip(text, hoverFlags))
            ToolTipInternal(text!);
    }

    /// <inheritdoc cref="AttachTooltip(string?, HFlags)"/>"
    public static void AttachTooltip(string? text, bool disabled, uint color, HFlags hoverFlags = TipHoverFlags)
    {
        if (!disabled && ShowTooltip(text, hoverFlags))
            ToolTipInternal(text!, colorUint: color);
    }

    /// <inheritdoc cref="AttachTooltip(string?, HFlags)"/>"
    public static void AttachTooltip(string? text, bool disabled, Vector4 color, HFlags hoverFlags = TipHoverFlags)
    {
        if (!disabled && ShowTooltip(text, hoverFlags))
            ToolTipInternal(text!, color);
    }

    /// <inheritdoc cref="AttachTooltip(string?, HFlags)"/>"
    public static void AttachToolTipRect(Vector2 min, Vector2 max, string? text)
    {
        if (!string.IsNullOrWhiteSpace(text) && ImGui.IsMouseHoveringRect(min, max))
            ToolTipInternal(text);
    }

    /// <inheritdoc cref="AttachTooltip(string?, HFlags)"/>"
    public static void AttachToolTipRect(Vector2 min, Vector2 max, string? text, uint color)
    {
        if (!string.IsNullOrWhiteSpace(text) && ImGui.IsMouseHoveringRect(min, max))
            ToolTipInternal(text, colorUint: color);
    }

    /// <inheritdoc cref="AttachTooltip(string?, HFlags)"/>"
    public static void AttachToolTipRect(Vector2 min, Vector2 max, string? text, Vector4 color)
    {
        if (!string.IsNullOrWhiteSpace(text) && ImGui.IsMouseHoveringRect(min, max))
            ToolTipInternal(text, color);
    }

    public static void ToolTipInternal(string text, Vector4? color = null, uint? colorUint = null)
    {
        using var s = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.One * 6f)
            .Push(ImGuiStyleVar.WindowRounding, 4f)
            .Push(ImGuiStyleVar.PopupBorderSize, 1f);
        using var c = ImRaii.PushColor(ImGuiCol.Border, CkCol.TipFrame.Vec4Ref());

        ImGui.BeginTooltip();

        if (color.HasValue)
            TextWrappedTooltipFormat(text, ImGui.GetFontSize() * 35f, color.Value);
        else if (colorUint.HasValue)
            TextWrappedTooltipFormat(text, ImGui.GetFontSize() * 35f, colorUint.Value);
        else
            TextWrappedTooltipFormat(text, ImGui.GetFontSize() * 35f);

        ImGui.EndTooltip();
    }

    public static void TextWrappedTooltipFormat(string text, float wrapWidth)
    {
        ImGui.PushTextWrapPos(wrapWidth);
        // Split the text by regex.
        var tokens = TooltipTokenRegex().Split(text);
        // if there were no tokens, just print the text unformatted
        if (tokens.Length <= 1)
        {
            ImGui.TextUnformatted(text);
            ImGui.PopTextWrapPos();
            return;
        }

        // Otherwise, parse it!
        var firstLineSegment = true;
        foreach (var token in tokens)
        {
            switch (token)
            {
                case TipSep: ImGui.Separator(); break;
                case TipNL: ImGui.NewLine(); break;

                default:
                    if (string.IsNullOrEmpty(token))
                        continue; // Skip empty tokens

                    if (!firstLineSegment)
                        ImGui.SameLine(0, 0);
                    
                    ImGui.TextUnformatted(token);
                    firstLineSegment = false;
                    break;
            }
        }
        ImGui.PopTextWrapPos();
    }

    public static void TextWrappedTooltipFormat(string text, float wrapWidth, Vector4 color)
    {
        ImGui.PushTextWrapPos(wrapWidth);
        // Split the text by regex.
        var tokens = TooltipTokenRegex().Split(text);
        // if there were no tokens, just print the text unformatted
        if (tokens.Length <= 1)
        {
            ImGui.TextUnformatted(text);
            ImGui.PopTextWrapPos();
            return;
        }

        // Otherwise, parse it!
        var useColor = false;
        var firstLineSegment = true;

        foreach (var token in tokens)
        {
            switch (token)
            {
                case TipSep: ImGui.Separator(); break;
                case TipNL: ImGui.NewLine(); break;
                case TipCol: useColor = !useColor; break;

                default:
                    if (string.IsNullOrEmpty(token))
                        continue; // Skip empty tokens

                    if (!firstLineSegment)
                        ImGui.SameLine(0, 0);

                    if (useColor)
                        ColorText(token, color);
                    else
                        ImGui.TextUnformatted(token);

                    firstLineSegment = false;
                    break;
            }
        }
        ImGui.PopTextWrapPos();
    }

    public static void TextWrappedTooltipFormat(string text, float wrapWidth, uint color)
    {
        ImGui.PushTextWrapPos(wrapWidth);
        // Split the text by regex.
        var tokens = TooltipTokenRegex().Split(text);
        // if there were no tokens, just print the text unformatted
        if (tokens.Length <= 1)
        {
            ImGui.TextUnformatted(text);
            ImGui.PopTextWrapPos();
            return;
        }

        // Otherwise, parse it!
        var useColor = false;
        var firstLineSegment = true;

        foreach (var token in tokens)
        {
            switch (token)
            {
                case TipSep: ImGui.Separator(); break;
                case TipNL: ImGui.NewLine(); break;
                case TipCol: useColor = !useColor; break;

                default:
                    if (string.IsNullOrEmpty(token))
                        continue; // Skip empty tokens

                    if (!firstLineSegment)
                        ImGui.SameLine(0, 0);

                    if (useColor)
                        ColorText(token, color);
                    else
                        ImGui.TextUnformatted(token);

                    firstLineSegment = false;
                    break;
            }
        }
        ImGui.PopTextWrapPos();
    }

    public static void HelpText(string helpText, bool inner = false, uint? offColor = null)
    {
        if (inner)
            ImUtf8.SameLineInner();
        else
            ImGui.SameLine();

        bool hovering = ImGui.IsMouseHoveringRect(ImGui.GetCursorScreenPos(), ImGui.GetCursorScreenPos() + new Vector2(ImGui.GetTextLineHeight()));
        FramedIconText(FAI.QuestionCircle, hovering ? ImGui.GetColorU32(ImGuiColors.TankBlue) : offColor ?? ImGui.GetColorU32(ImGuiCol.TextDisabled));
        AttachTooltip(helpText);
    }

    public static void HelpText(string text, Vector4 tooltipCol, bool inner = false, uint? offColor = null)
    {
        if (inner)
            ImUtf8.SameLineInner();
        else
            ImGui.SameLine();

        bool hovering = ImGui.IsMouseHoveringRect(ImGui.GetCursorScreenPos(), ImGui.GetCursorScreenPos() + new Vector2(ImGui.GetTextLineHeight()));
        FramedIconText(FAI.QuestionCircle, hovering ? ImGui.GetColorU32(ImGuiColors.TankBlue) : offColor ?? ImGui.GetColorU32(ImGuiCol.TextDisabled));
        AttachTooltip(text, color: tooltipCol);
    }

    public static void HelpText(string helpText, uint tooltipCol, bool inner = false, uint? offColor = null)
    {
        if (inner)
            ImUtf8.SameLineInner();
        else
            ImGui.SameLine();

        bool hovering = ImGui.IsMouseHoveringRect(ImGui.GetCursorScreenPos(), ImGui.GetCursorScreenPos() + new Vector2(ImGui.GetTextLineHeight()));
        FramedIconText(FAI.QuestionCircle, hovering ? ImGui.GetColorU32(ImGuiColors.TankBlue) : offColor ?? ImGui.GetColorU32(ImGuiCol.TextDisabled));
        AttachTooltip(helpText, color: ColorHelpers.RgbaUintToVector4(tooltipCol));
    }


    [GeneratedRegex($"({TipSep}|{TipNL}|{TipCol})", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    public static partial Regex TooltipTokenRegex();
}
