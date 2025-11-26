using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using OtterGui;
using OtterGui.Text;

namespace CkCommons.Gui;

// Primary Partial Class
public static partial class CkGui
{
    public static float GetSeparatorVWidth(float? width = null)
        => ImGui.GetStyle().ItemSpacing.X * 2 + (width ?? 1 * ImGuiHelpers.GlobalScale);

    public static float GetSeparatorHeight(float? height = null)
        => height + ImGui.GetStyle().ItemSpacing.Y * 2 ?? ImGui.GetStyle().ItemSpacing.Y * 3;

    public static float GetSeparatorSpacedHeight(float? height = null)
        => height + ImGui.GetStyle().ItemSpacing.Y * 4 ?? ImGui.GetStyle().ItemSpacing.Y * 5;

    public static void Separator(float? width = null, float? height = null)
    {
        ImGui.Dummy(new Vector2(width ?? ImGui.GetContentRegionAvail().X, height ?? ImGui.GetStyle().ItemSpacing.Y));
    }


    public static void Separator(uint col, float? width = null, float? height = null)
    {
        ImGui.Dummy(new Vector2(width ?? ImGui.GetContentRegionAvail().X, height ?? ImGui.GetStyle().ItemSpacing.Y));
        ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), col);
    }

    public static void SeparatorSpaced(float? height = null, float? width = null)
    {
        ImGui.Spacing();
        ImGui.Dummy(new Vector2(width ?? ImGui.GetContentRegionAvail().X, height ?? ImGui.GetStyle().ItemSpacing.Y));
        ImGui.Spacing();
    }

    public static void SeparatorSpaced(uint col, float? height = null, float? width = null)
    {
        ImGui.Spacing();
        ImGui.Dummy(new Vector2(width ?? ImGui.GetContentRegionAvail().X, height ?? ImGui.GetStyle().ItemSpacing.Y));
        ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), col);
        ImGui.Spacing();
    }

    public static void SeparatorV(float? width = null, uint? col = null, float? height = null)
    {
        ImGui.SameLine(0, ImUtf8.ItemSpacing.X);
        var lineHeight = height ?? ImGui.GetContentRegionAvail().Y;
        var lineWidth = width ?? 1 * ImGuiHelpers.GlobalScale;
        col ??= ImGui.GetColorU32(ImGuiCol.Border);
        ImGui.Dummy(new Vector2(lineWidth, lineHeight));
        ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), col.Value);
        ImGui.SameLine();
    }

    public static void TextLineSeparatorV(float? width = null, uint? col = null)
        => SeparatorV(width, col, ImGui.GetTextLineHeight());

    public static void FrameSeparatorV(float? width = null, uint? col = null)
        => SeparatorV(width, col, ImGui.GetFrameHeight());
   }
