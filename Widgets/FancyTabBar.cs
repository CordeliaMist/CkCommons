using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using OtterGuiInternal.Structs;
using System.Diagnostics.CodeAnalysis;

namespace CkCommons.Widgets;

// A Requirement contract for any tab items being passed into a fancy tab bar.
public interface IFancyTab
{
    /// <summary> The Label, doubling as the ID of the tab item </summary>
    public string Label { get; }

    /// <summary> The tooltip displayed when you hover this tab item. Can use CkGui Encoding. </summary>
    public string Tooltip { get; }

    /// <summary> If the tab should be currently disabled or not. </summary>
    public bool   Disabled { get; }


    /// <summary> The function to draw the content region remaining within the child object. </summary>
    public void DrawContents(float width);
}

// Alternative Storage could be defining id's as ints and using IFancyTab?[] Selections = new IFancyTab?[MaxTabBarConst];
// But we are not needing to hyper optimize right now, and to be quite honest, im really dead with all this UI stuff right now.

public static class FancyTabBar
{
    public static float BarHeight => ImGui.GetFrameHeightWithSpacing();
    public static float FullHeight => BarHeight + ImGui.GetStyle().ItemInnerSpacing.Y;
    public static float Rounding => BarHeight * .65f;
    public static float RoundingInner => BarHeight * .5f;

    // Internal State Storage for the current item references from various drawcalls with different ids.
    // If this ever REALLY becomes an issue we can switch to an array with ID-type casting. But for now, this is fine.
    private static readonly Dictionary<string, IFancyTab?> _selectedStorage = new();

    public static void SelectTab(string id, IFancyTab toSelect, params IFancyTab[] tabs)
    {
        // do not select if the ID does not exist.
        if (!_selectedStorage.ContainsKey(id))
            return;
        // Select it and set it.
        if (tabs.Contains(toSelect))
            _selectedStorage[id] = toSelect;
    }

    /// <summary> This WILL end with the cursorpos at the point you can draw the content region at. </summary>
    /// <returns> If a new tab was selected that is different from the last frame selection. </returns>
    public static bool DrawBar(string id, float width, float rounding, uint col, uint hoverCol, uint contrastCol, [NotNullWhen(true)] out IFancyTab? selected, params IFancyTab[] tabs)
    {
        if (!_selectedStorage.ContainsKey(id))
            _selectedStorage[id] = tabs.FirstOrDefault();

        selected = _selectedStorage[id];
        var firstTab = true;

        // Get the window drawlist from the outer tab bar.
        var wdl = ImGui.GetWindowDrawList();

        var heightWithSplit = BarHeight + ImGui.GetStyle().ItemInnerSpacing.Y;
        using (ImRaii.Child("FancyTabBar" + id, new ImVec2(width, heightWithSplit)))
        {
            // Get the window draw list from the child of the tab bars row child.
            var childWdl = ImGui.GetWindowDrawList();
            childWdl.AddLine(ImGui.GetItemRectMin() + new ImVec2(0, BarHeight), ImGui.GetItemRectMin() + new ImVec2(width, BarHeight), col, 2f);
            childWdl.AddLine(ImGui.GetItemRectMin() + new ImVec2(0, heightWithSplit), ImGui.GetItemRectMin() + new ImVec2(width, heightWithSplit), CkColor.ElementSplit.Uint(), 2f);

            ImGui.SameLine(0, rounding);
            foreach (var tab in tabs)
            {
                var isSelected = selected?.Label == tab.Label;
                // We pass in the drawlist from inside if the selected, and outside if not.
                // This means the layers are drawn such that all `wdl` items are drawn first, with `childWdl` drawn on top of it.
                // (This occurs due to being a nested child.) (Allowing us to manipulate blending from different draw lists)
                if (DrawTab(tab.Label, isSelected ? childWdl : wdl, isSelected, tab.Disabled, firstTab))
                    selected = tab;
                // provide spacing for the curves to be drawn.
                ImGui.SameLine(0, BarHeight);
                firstTab = false;
            }
        }
        // Here we get the itemrectsize and min from this child to calculate the exact position below it to begin drawing the content child in.
        var barMin = ImGui.GetItemRectMin();
        var barMax = ImGui.GetItemRectMax();
        wdl.AddRectFilled(barMin, barMax, contrastCol, rounding, DFlags.RoundCornersTop);

        var stateChanged = selected != _selectedStorage[id];
        _selectedStorage[id] = selected;

        // Then draw out the contents.
        ImGui.SetCursorScreenPos(new ImVec2(barMin.X, barMax.Y));

        return stateChanged;

        bool DrawTab(string label, ImDrawListPtr wdl, bool selected, bool disabled, bool first)
        {
            using var group = ImRaii.Group();
            var textSize = ImGui.CalcTextSize(label);
            var tabRegion = new ImVec2(textSize.X, BarHeight);

            // Draw an invisible button to capture the click.
            var clicked = ImGui.InvisibleButton("##TabItem" + label, tabRegion);
            var tabRect = new ImRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
            var hovered = !disabled && ImGui.IsMouseHoveringRect(tabRect.Min, tabRect.Max);

            // Only draw the frame if it is hovered or selected.
            if (selected || hovered)
            {
                // Otherwise, we should display either the selected or hovered state.
                var radLarge = Rounding;
                var radSmall = BarHeight - radLarge;
                // Calculations made in order of point.
                var leftCircleLower = new ImVec2(tabRect.MinX - BarHeight, tabRect.MaxY - radSmall);
                var leftCircleUpper = new ImVec2(tabRect.MinX, tabRect.MaxY - radSmall);
                var rightCircleUpper = new ImVec2(tabRect.MaxX, tabRect.MaxY - radSmall);
                var rightCircleLower = new ImVec2(tabRect.MaxX + BarHeight, tabRect.MaxY - radSmall);

                // We need to start in bottom center because in order for ANY inverse curves to process it must have LoS to the origin point.
                var bottomCenter = tabRect.Max - new ImVec2(tabRegion.X / 2, 0);
                wdl.PathClear();
                wdl.PathLineTo(bottomCenter); // Bottom Center (Origin)
                if (first) wdl.PathLineTo(new ImVec2(tabRect.MinX - radLarge, tabRect.MaxY)); // Bottom Left inward Arc (First Frame only)
                else wdl.PathArcTo(leftCircleLower, radSmall, float.Pi / 2, 0); // Bottom Left inward Arc (Normal Frames)
                wdl.PathArcTo(leftCircleUpper, radLarge, float.Pi, 3 * float.Pi / 2); // Top Left outward Arc
                wdl.PathArcTo(rightCircleUpper, radLarge, 3 * float.Pi / 2, 2 * float.Pi); // Top Right outward Arc
                wdl.PathArcTo(rightCircleLower, radSmall, float.Pi, float.Pi / 2); // Bottom Right inward Arc

                // Determine the color based on if the tab is selected vs hovered.
                var color = (selected, hovered) switch
                {
                    (true, true) => hoverCol,
                    (true, false) => col,
                    _ => contrastCol
                };
                wdl.PathFillConvex(color);
            }

            // Draw aligned to the frame padding, centered, the text.
            var textPos = new ImVec2(tabRect.MinX, tabRect.MinY + (tabRegion.Y - textSize.Y) / 2);
            ImGui.GetWindowDrawList().AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), label);

            return clicked;
        }
    }
}
