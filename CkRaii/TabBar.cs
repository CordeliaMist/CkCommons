using CkCommons.Widgets;
using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;

namespace CkCommons.Raii;

// Improve this to have better compatibility with other CkRaii objects later.
// (as in better conditional return structs that provide inner regions.)

public static partial class CkRaii
{
    // remove colors later likely once we finalize CkStyle.
    public static IEOContainer TabBarChild(string id, uint col, uint hoverCol, uint offCol, LabelFlags lFlags, out IFancyTab? selected, params IFancyTab[] tabs)
        => TabBarChild(id, ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y, FancyTabBar.Rounding, col, hoverCol, offCol, lFlags, out selected, tabs);

    public static IEOContainer TabBarChild(string id, float rounding, uint col, uint hoverCol, uint offCol, LabelFlags lFlags, out IFancyTab? selected, params IFancyTab[] tabs)
        => TabBarChild(id, ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y, rounding, col, hoverCol, offCol, lFlags, out selected, tabs);

    public static IEOContainer TabBarChild(string id, float width, float rounding, uint col, uint hoverCol, uint offCol, LabelFlags lFlags, out IFancyTab? selected, params IFancyTab[] tabs)
        => TabBarChild(id, width, ImGui.GetContentRegionAvail().Y, rounding, col, hoverCol, offCol, lFlags, out selected, tabs);
    
    public static IEOContainer TabBarChild(string id, float width, float height, float rounding, uint col, uint hoverCol, uint offCol, LabelFlags lFlags, out IFancyTab? selected, params IFancyTab[] tabs)
    {
        ImGui.BeginGroup();
        FancyTabBar.DrawBar(id, width, rounding, col, hoverCol, offCol, out selected, tabs);
        var shouldPad = (lFlags & LabelFlags.PadInnerChild) != 0;
        // if the size includes the header, cleave it off by the bar height to get the correct inner height.
        if ((lFlags & LabelFlags.SizeIncludesHeader) != 0)
            height -= FancyTabBar.FullHeight;
        // we also want to check if we should add padding to the height. Put in else if so this and SizeIncludesHeader dont work together.
        else if ((lFlags & LabelFlags.AddPaddingToHeight) != 0)
            height = height.AddWinPadY();

        // if we want to resize the height to the available region do so here.
        if ((lFlags & LabelFlags.ResizeHeightToAvailable) != 0)
            height = Math.Min(height, ImGui.GetContentRegionAvail().Y);

        // calculate the inner size based on padding rule.
        var childSize = new Vector2(width, height);
        var innerSize = shouldPad ? childSize.WithoutWinPadding() : childSize;
        var success = ImGui.BeginChild(id, childSize, false, shouldPad ? WFlags.AlwaysUseWindowPadding : WFlags.None);
        return new EndObjectContainer(() =>
        {
            ImGui.EndChild();
            ImGui.EndGroup();
            // border frame it.
            ImGui.GetWindowDrawList().AddRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), 
                col, rounding, DFlags.RoundCornersAll, 1.5f * ImGuiHelpers.GlobalScale);
        }, success, innerSize);
    }
}
