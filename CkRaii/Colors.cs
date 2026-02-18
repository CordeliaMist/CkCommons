using Dalamud.Bindings.ImGui;

namespace CkCommons.Raii;

public static partial class CkRaii
{
    /// <summary>
    ///     Push a CkCol temporarily and automatically pop on Dispose.
    /// </summary>
    public static Color PushColor(CkCol var, Vector4 color, bool condition = true)
        => new Color().Push(var, color, condition);

    public static Color PushColor(CkCol var, uint color, bool condition = true)
        => new Color().Push(var, color, condition);

    public sealed class Color : IDisposable
    {
        // how many this instance pushed
        private int _count;

        public Color Push(CkCol var, Vector4 color, bool condition = true)
        {
            if (!condition)
                return this;

            CkColors.PushColor(var, color);
            _count++;
            return this;
        }

        public Color Push(CkCol var, uint color, bool condition = true)
            => Push(var, color.ToVec4(), condition);

        public void Pop(int num = 1)
        {
            num = Math.Min(num, _count);
            _count -= num;
            ImGui.PopStyleColor(num);
        }

        public void Dispose()
            => Pop(_count);
    }
}
