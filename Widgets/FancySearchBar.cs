using CkCommons.Gui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using Dalamud.Bindings.ImGui;
using OtterGui.Text;

namespace CkCommons.Widgets;

public class FancySearchBar
{
    // WIP - At the moment the clear text does not appear to do much, unsure why currently. Look into how otter clears text probably.
    public unsafe static bool Draw(string id, float width, string hint, ref string str, int textLen, float rWidth = 0f, Action? rButtons = null)
    {
        var needsFocus = false;
        var height = ImGui.GetTextLineHeight() + (ImGui.GetStyle().FramePadding.Y * 2);
        var searchWidth = width - CkGui.IconButtonSize(FAI.TimesCircle).X -
            ((rButtons is not null) ? (rWidth + ImGui.GetStyle().ItemInnerSpacing.X * 2) : ImGui.GetStyle().ItemSpacing.X*2);
        var size = new Vector2(width, height);
        var ret = false;

        using var group = ImRaii.Group();
        var pos = ImGui.GetCursorScreenPos();
        // Mimic a child window, because if we use one, any button actions are blocked, and wont display the popups.
        ImGui.Dummy(size);
        ImGui.GetWindowDrawList().AddRectFilled(pos, pos + size, ImGui.GetColorU32(ImGuiCol.FrameBg), ImGui.GetStyle().FrameRounding);
        ImGui.SetCursorScreenPos(pos);

        if (!str.IsNullOrEmpty())
        {
            // push the color for the button to have an invisible bg.
            if (CkGui.IconButton(FAI.TimesCircle, inPopup: true))
            {
                str = string.Empty;
                needsClear = true;
                needsFocus = true;
            }
        }
        else
        {
            CkGui.IconButton(FAI.Search, disabled: true, inPopup: true);
        }

        // String input
        ImUtf8.SameLineInner();
        ImGui.SetNextItemWidth(searchWidth);

        if (needsFocus)
        {
            ImGui.SetKeyboardFocusHere();
            needsFocus = false;
        }

        // the return value
        var localSearchStr = str;

        using (ImRaii.PushColor(ImGuiCol.FrameBg, 0))
        {
            var flags = ITFlags.NoHorizontalScroll | ITFlags.NoUndoRedo | ITFlags.CallbackAlways;
            ret = ImGui.InputTextWithHint("##" + id, hint, ref localSearchStr, textLen, flags, (data) =>
            {
                if (needsClear)
                {
                    needsClear = false;
                    localSearchStr = string.Empty;

                    data.ClearSelection();
                    data.CursorPos = 0;
                    data.BufDirty = true;
                }
                return 1;
            });
        }

        if (rWidth > 0 && rButtons is not null)
        {
            ImUtf8.SameLineInner();
            rButtons();
        }

        str = localSearchStr;
        return ret;
    }

    public static bool needsClear = false;
}
