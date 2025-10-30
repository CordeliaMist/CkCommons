using Dalamud.Bindings.ImGui;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CkCommons.Gui;
#nullable disable

// DragDrop helpers, pulled from ECommons.ImGuiMethods.ImGuiDragDrop for reference, credit to them for original code. This is an adaptation.
public static partial class CkGui
{
    /// <summary> A helper function to attach a tooltip to a section in the UI currently hovered. </summary>
    public static unsafe void SetDragDropPayload<T>(ImU8String type, T data, ImGuiCond cond = 0) where T : struct
    {
        var span = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref data, 1));
        ImGui.SetDragDropPayload(type, span, cond);
    }

    public static unsafe bool AcceptDragDropPayload<T>(string type, out T payload, ImGuiDragDropFlags flags = ImGuiDragDropFlags.None) where T : struct
    {
        ImGuiPayload* pload = ImGui.AcceptDragDropPayload(type, flags);
        payload = (pload != null) ? Unsafe.Read<T>(pload->Data) : default;
        return pload != null;
    }

    public static unsafe void SetDragDropPayload(ImU8String type, string data, ImGuiCond cond = 0)
    {
        Span<byte> utf8Bytes = stackalloc byte[Encoding.UTF8.GetByteCount(data)];
        Encoding.UTF8.GetBytes(data, utf8Bytes);
        ReadOnlySpan<byte> span = utf8Bytes;
        ImGui.SetDragDropPayload(type, span, cond);
    }

    public static unsafe bool AcceptDragDropPayload(string type, out string payload, ImGuiDragDropFlags flags = ImGuiDragDropFlags.None)
    {
        ImGuiPayload* payloadPtr = ImGui.AcceptDragDropPayload(type, flags);
        payload = (payloadPtr != null) ? Encoding.Default.GetString((byte*)payloadPtr->Data, payloadPtr->DataSize) : null;
        return payloadPtr != null;
    }
}
