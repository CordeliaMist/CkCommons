using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace CkCommons.Raii;
public static partial class CkRaii
{
    public static Vector2 HeaderTextOffset(float headerWidth, float headerHeight, float textWidth, HeaderFlags align)
        => align switch
        {
            HeaderFlags.AlignLeft => new Vector2(ImGui.GetStyle().FramePadding.X, (headerHeight - ImGui.GetTextLineHeight()) / 2),
            HeaderFlags.AlignRight => new Vector2(headerWidth - textWidth - ImGui.GetStyle().FramePadding.X, (headerHeight - ImGui.GetTextLineHeight()) / 2),
            _ => new Vector2((headerWidth - textWidth) / 2, (headerHeight - ImGui.GetTextLineHeight()) / 2), // Center is default.
        };

    // used by ImGui.Child and ImGui.Group
    private struct EndUnconditionally(Action endAction, bool success) : ImRaii.IEndObject
    {
        private Action EndAction { get; } = endAction;
        public bool Success { get; } = success;
        public bool Disposed { get; private set; } = false;

        public void Dispose()
        {
            if (Disposed)
                return;

            EndAction();
            Disposed = true;
        }
    }

    /// <summary> An IEndObject that serves for ImRaii.EndUnconditionally, exclusively for containers. </summary>
    /// <remarks> This should only be used for unconditionally ended ImGui.Group objects. </remarks>
    private struct EndObjectContainer(Action endAction, bool success, Vector2 innerRegion) : IEOContainer
    {
        private Action EndAction { get; } = endAction;
        public Vector2 InnerRegion { get; } = innerRegion;
        public bool Success { get; } = success;
        public bool Disposed { get; private set; } = false;

        public void Dispose()
        {
            if (Disposed)
                return;

            EndAction();
            Disposed = true;
        }
    }

    /// <summary> An IEndObject extention of EndObjectContainer, for advanced container objects built from CkRaii. </summary>
    /// <remarks> This should only be used for unconditionally ended ImGui.Group and ImGui.Child objects. </remarks>
    private struct EndObjectLabelContainer(Action endAction, bool success, Vector2 inner, Vector2 innerNoLabel) : IEOLabelContainer
    {
        private Action EndAction { get; } = endAction;
        public Vector2 InnerRegion { get; } = inner;
        public Vector2 InnerNoLabel { get; } = innerNoLabel;
        public bool Success { get; } = success;
        public bool Disposed { get; private set; } = false;

        public void Dispose()
        {
            if (Disposed)
                return;

            EndAction();
            Disposed = true;
        }
    }

    public interface IEOLabelContainer : IEOContainer
    {
        /// <summary> The region of the main container. </summary>
        Vector2 InnerNoLabel { get; }
    }

    public interface IEOContainer : ImRaii.IEndObject
    {
        /// <summary> The inner region below the label. </summary>
        Vector2 InnerRegion { get; }
    }
}
