using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using OtterGui.Text;

namespace CkCommons.Gui;

// Primary Partial Class
public static partial class CkGui
{
    public static float GetSeparatorVWidth(float? width = null, bool inner = false)
        => 2 * (inner ? ImUtf8.ItemInnerSpacing.X : ImUtf8.ItemSpacing.X) + (width ?? 1 * ImGuiHelpers.GlobalScale);

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

    public static void SeparatorV(uint? col = null, float? height = null, bool inner = false)
    {
        var jumpDist = inner ? ImUtf8.ItemInnerSpacing.X : ImUtf8.ItemSpacing.X;
        ImGui.SameLine(0, jumpDist * 2);
        var pos = ImGui.GetCursorScreenPos();
        pos -= new Vector2(jumpDist, 0);
        var lineHeight = height ?? ImGui.GetContentRegionAvail().Y;
        col ??= ImGui.GetColorU32(ImGuiCol.Border);
        ImGui.GetWindowDrawList().AddLine(pos, pos + new Vector2(0, lineHeight), col.Value, ImGuiHelpers.GlobalScale);
    }

    public static void TextLineSeparatorV(uint? col = null, bool inner = false)
        => SeparatorV(col, ImGui.GetTextLineHeight(), inner);

    public static void FrameSeparatorV(uint? col = null, bool inner = false)
        => SeparatorV(col, ImGui.GetFrameHeight(), inner);
}
