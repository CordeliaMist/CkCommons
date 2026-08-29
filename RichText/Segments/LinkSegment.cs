using CkCommons.Gui;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using OtterGui.Text;
using System.Diagnostics;

namespace CkCommons.RichText;

public class LinkSegment(string url, string displayText) : IRichSegment
{
    private bool _isInline = false;
    public string Url => url;
    public string DisplayText => displayText;

    public void Draw(RichStringContext ctx)
    {
        if (_isInline)
            ImGui.SameLine(0, 0);

        CkGui.ColorText(DisplayText, 0xFFEEAA55);
        if (ImGui.IsItemHovered())
        {
            // Change mouse cursor to hand pointer
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

            // Underline the text.
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            min.Y = max.Y;
            ImGui.GetWindowDrawList().AddLine(min, max, 0xFFEEAA55);

            // Attach tooltip
            using (ImRaii.Tooltip())
            {
                ImGui.TextUnformatted("Opens: ");
                CkGui.ColorTextInline(url, ImGuiColors.DalamudGrey);
                // Warning
                CkGui.ColorText("Must SHIFT + Click to open.", ImGuiColors.DalamudGrey2);
            }

            if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && ImGui.GetIO().KeyShift)
                Util.OpenLink(url);
        }
    }

    public void UpdateCache(ref RichStringContext ctx, int segmentIdx)
    {
        var prevWidth = ctx.CurrLineWidth;
        var width = ImGui.CalcTextSize(DisplayText).X;

        if (prevWidth + width > ctx.WrapWidth)
        {
            ctx.CurrLineWidth = width;
            ctx.LineCount++;
            _isInline = false;
        }
        else
        {
            ctx.CurrLineWidth = prevWidth + width;
            _isInline = segmentIdx > 0 && prevWidth > 0f;
        }
    }
}