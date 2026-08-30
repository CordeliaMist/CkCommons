using CkCommons.Gui;
using CkCommons.Helpers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace CkCommons.RichText;

// Helper utility.
public static partial class NewRichText
{
    /// <summary>
    /// Draws a standard faint (?) marker that displays the formatting guide when hovered.
    /// </summary>
    public static void DrawHelpMarker(uint colorHover, uint colorOff)
    {
        CkGui.HoverIconText(FAI.Code, colorHover, colorOff);
        if (ImGui.IsItemHovered())
        {
            using var tooltip = ImRaii.Tooltip();
            // Optional: limit tooltip width so it doesn't span the whole screen
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            DrawRichTextGuide();
            ImGui.PopTextWrapPos();
        }
    }

    public static void DrawColorGuide()
    {
        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(4));
        ImGui.PushTextWrapPos(ImGui.GetFontSize() * 25f);
        ImGui.TextUnformatted(string.Join(", ", Enum.GetNames<XlDataUiColor>()));
        ImGui.PopTextWrapPos();
    }

    /// <summary>
    ///   Draws the actual rich text formatting guide table.
    ///   Can be used in a tooltip, a popup, or a dedicated settings tab.
    /// </summary>
    public static void DrawRichTextGuide()
    {
        CkGui.ColorText("Rich Text Formatting Guide", ImGuiColors.TankBlue);
        ImGui.Separator();
        ImGui.Spacing();
        using var t = ImRaii.Table("richtext-guide", 2, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.SizingStretchProp);
        if (t)
        {
            ImGui.TableSetupColumn("Syntax", ImGuiTableColumnFlags.WidthFixed, 180f * ImGuiHelpers.GlobalScale);
            ImGui.TableSetupColumn("Description");
            // Text Coloring
            DrawHelpRow("[color=red]...[/color]", "Changes text color. Supports named colors, hex codes (0xRRGGBB), or UI Color IDs.");
            DrawHelpRow("[stroke=blue]...[/stroke]", "Adds a colored outline. Can also use [glow=...]. Supports the same color formats.");
            // Links
            DrawHelpRow("[link=https://...]", "Creates a clickable hyperlink out of the URL.");
            DrawHelpRow("[link=URL|Text]", "Creates a clickable hyperlink with custom display text.");
            // Media
            DrawHelpRow(":emote_name:", "Displays an inline emote. (Prefix with 's~' for stickers).");
            DrawHelpRow("[img=file_name]", "Displays an image from the assets folder.");
            // Formatting
            DrawHelpRow("[line]", "Draws a horizontal separator line across the message.");
            DrawHelpRow("[para]", "Forces a paragraph break (large gap). Double pressing Enter also does this.");
        }

        ImGui.Spacing();
    }

    private static void DrawHelpRow(string syntax, string description)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        CkGui.ColorText(syntax, ImGuiColors.DalamudOrange);
        ImGui.TableNextColumn();
        ImGui.TextWrapped(description);
    }
}
