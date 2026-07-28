using CkCommons.Gui;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using OtterGui.Text;

namespace CkCommons.Widgets;

public class FancySearchBar
{
    public static bool Draw(string id, ref string value, int len, ITFlags flags = ITFlags.None, ImGui.ImGuiInputTextCallbackPtrDelegate? callback = null)
        => DrawWithButtons(id, "Search..", ImGui.GetContentRegionAvail().X, ref value, len, 0f, null, flags, callback);

    public static bool Draw(string id, float width, ref string value, int len, ITFlags flags = ITFlags.None, ImGui.ImGuiInputTextCallbackPtrDelegate? callback = null)
        => DrawWithButtons(id, "Search..", width, ref value, len, 0f, null, flags, callback);

    public static bool Draw(string id, string hint, ref string value, int len, ITFlags flags = ITFlags.None, ImGui.ImGuiInputTextCallbackPtrDelegate? callback = null)
        => DrawWithButtons(id, hint, ImGui.GetContentRegionAvail().X, ref value, len, 0f, null, flags, callback);

    public static bool Draw(string id, string hint, float width, ref string value, int len, ITFlags flags = ITFlags.None, ImGui.ImGuiInputTextCallbackPtrDelegate? callback = null)
        => DrawWithButtons(id, hint, width, ref value, len, 0f, null, flags, callback);

    public static bool DrawWithButtons(string id, ref string value, int len, float rWidth, Action rDraw, ITFlags flags = ITFlags.None, ImGui.ImGuiInputTextCallbackPtrDelegate? callback = null)
        => DrawWithButtons(id, "Search..", ImGui.GetContentRegionAvail().X, ref value, len, rWidth, rDraw, flags, callback);

    public static bool DrawWithButtons(string id, float width, ref string value, int len, float rWidth, Action rDraw, ITFlags flags = ITFlags.None, ImGui.ImGuiInputTextCallbackPtrDelegate? callback = null)
        => DrawWithButtons(id, "Search..", width, ref value, len, rWidth, rDraw, flags, callback);

    public static bool DrawWithButtons(string id, string hint, ref string value, int len, float rWidth, Action rDraw, ITFlags flags = ITFlags.None, ImGui.ImGuiInputTextCallbackPtrDelegate? callback = null)
        => DrawWithButtons(id, hint, ImGui.GetContentRegionAvail().X, ref value, len, rWidth, rDraw, flags, callback);

    public static bool DrawWithButtons(string label, string hint, float width, ref string value, int len, float rWidth, Action? rDraw, ITFlags flags = ITFlags.None, ImGui.ImGuiInputTextCallbackPtrDelegate? callback = null)
    {
        var window = ImGuiP.GetCurrentWindow();
        if (window.SkipItems)
            return false;

        var style = ImGui.GetStyle();
        var id = ImGui.GetID(label);
        var screenPos = window.DC.CursorPos;

        var needsClear = false; // captured by callback

        var hasSearchValue = !string.IsNullOrEmpty(value);
        var icon = hasSearchValue ? FAI.TimesCircle : FAI.Search;

        var height = ImGui.GetFrameHeight();
        var iconW = CkGui.IconButtonSize(FAI.TimesCircle).X;
        
        var size = new Vector2(width, height);
        var bb = new ImRect(screenPos, screenPos + size);

        // Add to Layout
        ImGuiP.ItemSize(size, style.FramePadding.Y);
        if (!ImGuiP.ItemAdd(bb, id))
            return false;

        // Define rects for the button and input
        var iconBB = new ImRect(bb.Min, new Vector2(bb.Min.X + iconW, bb.Max.Y));
        var rightW = (rWidth > 0 && rDraw != null) ? rWidth + style.FramePadding.X : 0;
        var inputBB = new ImRect(new(bb.Min.X + iconW, bb.Min.Y), new(bb.Max.X - rightW, bb.Max.Y));

        // Render the full frame first
        ImGuiP.RenderFrame(bb.Min, bb.Max, ImGui.GetColorU32(ImGuiCol.FrameBg), true, style.FrameRounding);

        // Icon Display
        var iconHovered = false;
        var iconHeld = false;
        var iconClicked = ImGuiP.ButtonBehavior(iconBB, id, ref iconHovered, ref iconHeld);
        if (hasSearchValue && iconClicked)
        {
            value = string.Empty;
            needsClear = true;
            ImGui.SetKeyboardFocusHere(-1);
        }

        // Make custom colors here for this, not sure yet.
        var iconCol = ImGui.GetColorU32(iconHovered ? ImGuiCol.Text : ImGuiCol.TextDisabled);

        var iconPos = iconBB.Min + style.FramePadding;
        using (Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push())
            window.DrawList.AddText(icon.ToIconString(), iconPos, iconCol);

        // Input Text Area
        ImGui.SetCursorScreenPos(inputBB.Min);
        ImGui.SetNextItemWidth(inputBB.GetWidth());
        using var _ = ImRaii.PushColor(ImGuiCol.FrameBg, 0).Push(ImGuiCol.Border, 0);
        
        var ret = ImGui.InputTextWithHint(label + "##search", hint, ref value, len, flags | ITFlags.NoHorizontalScroll | ITFlags.NoUndoRedo | ITFlags.CallbackAlways, (data) =>
        {
            if (needsClear)
            {
                needsClear = false;
                data.ClearSelection();
                data.CursorPos = 0;
                data.BufDirty = true;
            }

            if (callback != null)
                return callback(data);

            return 0;
        });

        // Handle Buttons
        if (rWidth > 0 && rDraw != null)
        {
            ImUtf8.SameLineInner();
            rDraw();
        }

        return ret || iconClicked;
    }

    // Obsolute, remove when new method looks good to go.
    //public unsafe static bool Draw(string id, float width, ref string str, string hint, int length, float rWidth = 0f, Action? rButtons = null)
    //{
    //    var needsFocus = false;
    //    var height = ImUtf8.FrameHeight;
    //    var searchWidth = width - CkGui.IconButtonSize(FAI.TimesCircle).X -
    //        ((rButtons is not null) ? (rWidth + ImUtf8.ItemInnerSpacing.X*2) : ImUtf8.ItemSpacing.X*2);
    //    var size = new Vector2(width, height);
        
    //    bool ret = false;
    //    bool needsClear = false;

    //    using var group = ImRaii.Group();
    //    var pos = ImGui.GetCursorScreenPos();
    //    // Mimic a child window, because if we use one, any button actions are blocked, and wont display the popups.
    //    ImGui.Dummy(size);
    //    ImGui.GetWindowDrawList().AddRectFilled(pos, pos + size, ImGui.GetColorU32(ImGuiCol.FrameBg), ImGui.GetStyle().FrameRounding);
    //    ImGui.SetCursorScreenPos(pos);

    //    if (!str.IsNullOrEmpty())
    //    {
    //        // push the color for the button to have an invisible bg.
    //        if (CkGui.IconButton(FAI.TimesCircle, inPopup: true))
    //        {
    //            ret = true;
    //            str = string.Empty;
    //            needsClear = true;
    //            needsFocus = true;
    //        }
    //    }
    //    else
    //    {
    //        CkGui.IconButton(FAI.Search, disabled: true, inPopup: true);
    //    }

    //    // String input
    //    ImGui.SameLine(0, 0);
    //    ImGui.SetNextItemWidth(searchWidth);

    //    if (needsFocus)
    //    {
    //        ImGui.SetKeyboardFocusHere();
    //        needsFocus = false;
    //    }

    //    // the return value
    //    var localSearchStr = str;

    //    using (ImRaii.PushColor(ImGuiCol.FrameBg, 0))
    //    {
    //        var flags = ITFlags.NoHorizontalScroll | ITFlags.NoUndoRedo | ITFlags.CallbackAlways;
    //        ret |= ImGui.InputTextWithHint("##" + id, hint, ref localSearchStr, length, flags, (data) =>
    //        {
    //            if (needsClear)
    //            {
    //                needsClear = false;
    //                localSearchStr = string.Empty;

    //                data.ClearSelection();
    //                data.CursorPos = 0;
    //                data.BufDirty = true;
    //            }
    //            return 1;
    //        });
    //    }

    //    if (rWidth > 0 && rButtons is not null)
    //    {
    //        ImUtf8.SameLineInner();
    //        rButtons();
    //    }

    //    str = localSearchStr;
    //    return ret;
    //}
}
