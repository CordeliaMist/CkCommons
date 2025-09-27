using CkCommons.Gui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using OtterGui.Text.Widget;
using OtterGuiInternal.Utility;
using System.Runtime.CompilerServices;

namespace CkCommons.Classes;

public class TriStateBoolCheckbox(uint crossColor = 0xFF0000FF, uint checkColor = 0xFF00FF00, uint dotColor = 0xFFD0D0D0)
    : MultiStateCheckbox<TriStateBool>
{
    /// <inheritdoc/>
    protected override void RenderSymbol(TriStateBool value, Vector2 position, float size)
    {
        switch (value.Value)
        {
            case true:
                SymbolHelpers.RenderCheckmark(ImGui.GetWindowDrawList(), position, ImGui.GetColorU32(checkColor), size);
                break;
            case false:
                SymbolHelpers.RenderCross(ImGui.GetWindowDrawList(), position, ImGui.GetColorU32(crossColor), size);
                break;
            case null:
                SymbolHelpers.RenderDot(ImGui.GetWindowDrawList(), position, ImGui.GetColorU32(dotColor), size);
                break;
        }
    }

    /// <summary> Draw the tri-state checkbox. </summary>
    /// <returns> True when <paramref name="current"/> changed in this frame. </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool Draw(ReadOnlySpan<char> label, TriStateBool current, out TriStateBool newValue, bool disabled = false)
    {
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

    /// <inheritdoc/>
    protected override TriStateBool NextValue(TriStateBool value)
        => value.NextValue();

    /// <inheritdoc/>
    protected override TriStateBool PreviousValue(TriStateBool value)
        => value.PreviousValue();
}
