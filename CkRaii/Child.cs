using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace CkCommons.Raii;
public static partial class CkRaii
{
    /// <inheritdoc cref="OtterGui.Text.EndObjects.Child"/>"
    public static ImRaii.IEndObject Child(string id)
        => new EndUnconditionally(() => ImGui.EndChild(), ImGui.BeginChild(id));

    /// <inheritdoc cref="OtterGui.Text.EndObjects.Child"/>"
    public static IEOContainer Child(string id, Vector2 size, DFlags dFlags = DFlags.None, WFlags wFlags = WFlags.None)
        => Child(id, size, 0, CkStyle.ChildRounding(), dFlags, wFlags);

    /// <inheritdoc cref="OtterGui.Text.EndObjects.Child"/>"
    public static IEOContainer Child(string id, Vector2 size, uint bgCol, DFlags dFlags = DFlags.None, WFlags wFlags = WFlags.None)
        => Child(id, size, bgCol, CkStyle.ChildRounding(), dFlags, wFlags);

    /// <summary> ImRaii.Child alternative with bgCol and rounding support. </summary>
    /// <remarks> The IEndObject returned is a EndObjectContainer, holding the inner content region size. </remarks>
    public static IEOContainer Child(string id, Vector2 size, uint bgCol, float rounding, DFlags dFlags = DFlags.None, WFlags wFlags = WFlags.None)
        => FramedChild(id, size, bgCol, 0, rounding, dFlags, wFlags);



    /// <inheritdoc cref="FramedChild(string, Vector2, uint, uint, float, float, DFlags, WFlags)"/>/>
    public static IEOContainer FramedChild(string id, uint bgCol, uint frameCol, float thickness = 0, DFlags dFlags = DFlags.None)
        => FramedChild(id, ImGui.GetContentRegionAvail(), bgCol, frameCol, CkStyle.ChildRounding(), thickness, dFlags, WFlags.None);

    /// <inheritdoc cref="FramedChild(string, Vector2, uint, float, float, DFlags, WFlags)"/>/>
    public static IEOContainer FramedChild(string id, Vector2 size, uint bgCol, uint frameCol, DFlags dFlags = DFlags.None, WFlags wFlags = WFlags.None)
        => FramedChild(id, size, bgCol, frameCol, CkStyle.ChildRounding(), CkStyle.ThinThickness(), dFlags, wFlags);

    /// <inheritdoc cref="FramedChild(string, Vector2, uint, float, float, DFlags, WFlags)"/>/>
    public static IEOContainer FramedChild(string id, Vector2 size, uint bgCol, uint frameCol, float thickness, DFlags dFlags = DFlags.None, WFlags wFlags = WFlags.None)
        => FramedChild(id, size, bgCol, frameCol, CkStyle.ChildRounding(), thickness, dFlags, wFlags);

    /// <summary> ImRaii.Child alternative with bgCol and rounding support. (Supports frames) </summary>
    /// <remarks> The IEndObject returned is a EndObjectContainer, holding the inner content region size. </remarks>
    public static IEOContainer FramedChild(string id, Vector2 size, uint bgCol, uint frameCol, float rounding, float thickness, DFlags dFlags = DFlags.None, WFlags wFlags = WFlags.None)
    {
        var success = ImGui.BeginChild(id, size, false, wFlags);
        var innerSize = (wFlags & WFlags.AlwaysUseWindowPadding) != 0 ? size.WithoutWinPadding() : size;
        return new EndObjectContainer(() =>
        {
            ImGui.EndChild();
            Vector2 min = ImGui.GetItemRectMin();
            Vector2 max = ImGui.GetItemRectMax();

            // Draw out the child BG.
            if (bgCol is not 0)
                ImGui.GetWindowDrawList().AddRectFilled(min, max, bgCol, rounding, dFlags);
            // Draw out the frame.
            if (thickness is not 0)
                ImGui.GetWindowDrawList().AddRect(min, max, frameCol, rounding, dFlags, thickness);
        }, success, innerSize);
    }
}
