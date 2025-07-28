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
    public static IEOContainer Child(string id, Vector2 size, WFlags flags = WFlags.None)
        => Child(id, size, 0, CkStyle.ChildRounding(), DFlags.None, flags);

    /// <inheritdoc cref="OtterGui.Text.EndObjects.Child"/>"
    public static IEOContainer Child(string id, Vector2 size, uint bgCol, WFlags flags = WFlags.None)
        => Child(id, size, bgCol, CkStyle.ChildRounding(), DFlags.None, flags);

    /// <summary> ImRaii.Child alternative with bgCol and rounding support. </summary>
    /// <remarks> The IEndObject returned is a EndObjectContainer, holding the inner content region size. </remarks>
    public static IEOContainer Child(string id, Vector2 size, uint bgCol, float rounding, DFlags dFlags = DFlags.None, WFlags wFlags = WFlags.None)
        => FramedChild(id, size, bgCol, rounding, 0, dFlags, wFlags);



    /// <inheritdoc cref="FramedChild(string, Vector2, uint, float, float, DFlags, WFlags)"/>/>
    public static IEOContainer FramedChild(string id, uint bgCol, float thickness = 0, uint? frameCol = null, DFlags dFlags = DFlags.None)
        => FramedChild(id, ImGui.GetContentRegionAvail(), bgCol, CkStyle.ChildRounding(), thickness, frameCol, dFlags, WFlags.None);

    /// <inheritdoc cref="FramedChild(string, Vector2, uint, float, float, DFlags, WFlags)"/>/>
    public static IEOContainer FramedChild(string id, Vector2 size, uint bgCol, uint? frameCol = null, DFlags dFlags = DFlags.None, WFlags wFlags = WFlags.None)
        => FramedChild(id, size, bgCol, CkStyle.ChildRounding(), CkStyle.ThinThickness(), frameCol, dFlags, wFlags);

    /// <inheritdoc cref="FramedChild(string, Vector2, uint, float, float, DFlags, WFlags)"/>/>
    public static IEOContainer FramedChild(string id, Vector2 size, uint bgCol, float thickness, uint? frameCol = null, DFlags dFlags = DFlags.None, WFlags wFlags = WFlags.None)
        => FramedChild(id, size, bgCol, CkStyle.ChildRounding(), thickness, frameCol, dFlags, wFlags);

    /// <summary> ImRaii.Child alternative with bgCol and rounding support. (Supports frames) </summary>
    /// <remarks> The IEndObject returned is a EndObjectContainer, holding the inner content region size. </remarks>
    public static IEOContainer FramedChild(string id, Vector2 size, uint bgCol, float rounding, float thickness, uint? frameCol = null, DFlags dFlags = DFlags.None, WFlags wFlags = WFlags.None)
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
                ImGui.GetWindowDrawList().AddRect(min, max, frameCol.HasValue ? frameCol.Value : bgCol, rounding, dFlags, thickness);
        }, success, innerSize);
    }
}
