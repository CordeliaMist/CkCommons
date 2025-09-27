using CkCommons.Gui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using System.Runtime.CompilerServices;

namespace CkCommons.Classes;
public class TriStateBoolIconCheckbox(FontAwesomeIcon icon, uint crossColor = 0xFF0000FF, uint checkColor = 0xFF00FF00, uint dotColor = 0xFFD0D0D0) 
    : TriStateBoolCheckbox
{
    /// <inheritdoc/>
    protected override void RenderSymbol(TriStateBool value, Vector2 position, float size)
    {
        using var font = Svc.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push();
        var text = string.Empty + (char)icon;
        var iconSize = ImGui.CalcTextSize(text);
        var iconPos = position + (new Vector2(size) - iconSize) * 0.5f;
        switch (value.Value)
        {
            case true:
                ImGui.GetWindowDrawList().AddText(iconPos, ImGui.GetColorU32(checkColor), text);
                break;
            case false:
                ImGui.GetWindowDrawList().AddText(iconPos, ImGui.GetColorU32(crossColor), text);
                break;
            case null:
                ImGui.GetWindowDrawList().AddText(iconPos, ImGui.GetColorU32(dotColor), text);
                break;
        }
    }

    /// <summary> Draw the tri-state checkbox. </summary>
    /// <param name="label"> The label for the checkbox as a UTF8 string. HAS to be null-terminated. </param>
    /// <param name="value"> The input/output value. </param>
    /// <returns> True when <paramref name="value"/> changed in this frame. </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool DrawIconCheckbox(ReadOnlySpan<char> label, TriStateBool current, out TriStateBool newValue, bool disabled = false)
    {
        // Initialize newValue to the current state initially
        newValue = current;

        using (ImRaii.Disabled(disabled))
        {
            if (Draw(label, ref newValue))
                return true;
        }
        CkGui.AttachToolTip("This attribute will " + (newValue.Value switch
        {
            true => "be enabled.",
            false => "be disabled.",
            null => "be left as is.",
        }));
        return false;
    }
}
